namespace A55.Extensions.Configuration.Aws.SecretsManager.Abstraction;

interface IEnvAliasMapper
{
    string From(string environment);
}
