using A55.Extensions.Configuration.Aws.SecretsManager.Abstraction;
using Microsoft.Extensions.Hosting;

namespace A55.Extensions.Configuration.Aws.SecretsManager.Core;

class EnvAliasMapper : IEnvAliasMapper
{
    const string Prd = "prd";
    const string Dev = "dev";
    const string Stg = "stg";

    readonly Dictionary<string, string> envAliasDict;

    public EnvAliasMapper()
    : this (Dev, Stg, Prd)
    { }

    public EnvAliasMapper(string dev, string staging, string prod) =>
     envAliasDict = new()
        {
            [Environments.Development] = dev,
            [Environments.Staging] = staging,
            [Environments.Production] = prod
        };

    public string From(string environment) =>
        envAliasDict.TryGetValue(environment, out var alias)
            ? alias
            : environment;
}
