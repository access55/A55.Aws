using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Hosting;

namespace A55.Extensions.Configuration.Aws.SecretsManager.Core;

class AwsSecretsManager
{
    readonly IAmazonSecretsManager secretsManagerClient;
    readonly SecretsManagerCache cache;
    readonly string env;
    readonly string appName;

    public AwsSecretsManager(
        IAmazonSecretsManager secretsManagerClient,
        string environmentName,
        string applicationName
    )
    {
        this.secretsManagerClient = secretsManagerClient;
        cache = new(this.secretsManagerClient);

        env = EnvAlias.From(environmentName);
        this.appName = applicationName.ToLower();
    }

    public async Task<IDictionary<string, string>> GetProjectSecrets()
    {
        var secretsResponse = await secretsManagerClient.ListSecretsAsync(new()
        {
            MaxResults = 100,
            Filters = new List<Filter>
            {
                new()
                {
                    Key = "name",
                    Values = new List<string> {"/settings/shared", $"/settings/{appName}/{env}/"}
                }
            }
        });

        var secretsQuery =
            secretsResponse.SecretList
                .Select(secret => secret.Name)
                .Select(async secretName => (
                    Key: Path.GetFileName(secretName),
                    Value: await GetSecretString(secretName)));

        var result =
            (await Task.WhenAll(secretsQuery))
            .ToDictionary(x => x.Key, x => x.Value);

        return result;
    }

    public Task<string> GetSecretString(string keyName) => cache.GetSecretString(keyName);
}
