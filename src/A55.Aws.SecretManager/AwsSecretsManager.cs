using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace A55.Aws.SecretsManager;

using Amazon.SecretsManager.Extensions.Caching;
using Microsoft.Extensions.Hosting;

class AwsSecretsManager
{
    readonly IAmazonSecretsManager secretsManagerClient;
    readonly SecretsManagerCache cache;

    readonly string projectName;
    readonly string envAlias;

    public AwsSecretsManager(
        IAmazonSecretsManager secretsManagerClient,
        string environmentName,
        string projectName
    )
    {
        this.secretsManagerClient = secretsManagerClient;
        this.projectName = projectName.ToLowerInvariant();
        cache = new SecretsManagerCache(this.secretsManagerClient);

        envAlias =
            envAliasDict.TryGetValue(environmentName, out var envName)
                ? envName
                : throw new InvalidOperationException(
                    $"Cant find short name for environment {environmentName}");
    }

    readonly Dictionary<string, string> envAliasDict = new()
    {
        [Environments.Development] = "dev",
        [Environments.Staging] = "stg",
        [Environments.Production] = "prd",
    };

    public async Task<IDictionary<string, string>> GetProjectSecrets()
    {
        var baseKeys = new List<string>
        {
            "/settings/shared/",
            $"/settings/{projectName}/shared",
            $"/settings/{projectName}/{envAlias}"
        };

        var secretsResponse = await secretsManagerClient.ListSecretsAsync(new()
        {
            MaxResults = 100,
            Filters = new List<Filter>
            {
                new()
                {
                    Key = "name",
                    Values = baseKeys,
                }
            }
        });

        string GetKeyName(string path)
        {
            var key = Path.GetFileName(path);
            return key == envAlias || key == "shared" ? "/" : key;
        }

        var secretsQuery =
            secretsResponse.SecretList
                .Select(secret => secret.Name)
                .Select(async secretName => (
                    Key: GetKeyName(secretName),
                    Value: await GetSecretString(secretName)));

        var result = await Task.WhenAll(secretsQuery);
        return result.ToDictionary(x => x.Key, x => x.Value);
    }

    public Task<string> GetSecretString(string keyName) => cache.GetSecretString(keyName);
}

static class AmazonSecretsManagerClientFactory
{
    public static IAmazonSecretsManager Create(AwsSettings config)
    {
        var regionEndpoint = RegionEndpoint.GetBySystemName(config.RegionShared);
        if (!string.IsNullOrWhiteSpace(config.AccessKeyIdShared))
            return new AmazonSecretsManagerClient(config.ToCredentials(), regionEndpoint);

        SharedCredentialsFile sharedFile = new();
        if (sharedFile.TryGetProfile("default", out var profile) &&
            AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials))
            return new AmazonSecretsManagerClient(credentials, regionEndpoint);

        throw new InvalidOperationException("No AWS profile found");
    }
}
