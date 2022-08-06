namespace A55.Extensions.Configuration.Aws.SecretsManager.Abstraction;

interface ISecretsManagerService
{
    Task<IDictionary<string, string>> GetProjectSecrets(IEnumerable<string> paths, int maxResults);
    ValueTask<string> GetSecretString(string keyName);
}
