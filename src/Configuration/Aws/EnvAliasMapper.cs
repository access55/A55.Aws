using Microsoft.Extensions.Hosting;

namespace A55.Extensions.Configuration.Aws;

static class EnvAlias
{
    static readonly Dictionary<string, string> envAliasDict = new()
    {
        [Environments.Development] = "dev", [Environments.Staging] = "stg", [Environments.Production] = "prd",
    };

    public static string From(string environment) =>
        envAliasDict.TryGetValue(environment, out var alias)
            ? alias
            : throw new InvalidOperationException(
                $"Cant find alias for environment {environment}");
}
