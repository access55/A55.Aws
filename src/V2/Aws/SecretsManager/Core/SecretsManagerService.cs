using A55.Extensions.Configuration.Aws.SecretsManager.Abstraction;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;

namespace A55.Extensions.Configuration.Aws.SecretsManager.Core;

class SecretsManagerService : ISecretsManagerService
{
    readonly IAmazonSecretsManager secretsManagerClient;
    readonly SecretsManagerCache cache;

    public SecretsManagerService(IAmazonSecretsManager secretsManagerClient)
    {
        this.secretsManagerClient = secretsManagerClient;
        cache = new(secretsManagerClient);
    }

    public async Task<IDictionary<string, string>> GetProjectSecrets(IEnumerable<string> paths, int maxResults)
    {
        var secretsResponse = await secretsManagerClient.ListSecretsAsync(new()
        {
            MaxResults = maxResults, Filters = new() {new() {Key = "name", Values = paths.ToList()}}
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

    public async ValueTask<string> GetSecretString(string keyName) =>
        await cache.GetSecretString(keyName);
}
