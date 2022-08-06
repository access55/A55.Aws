using A55.Extensions.Configuration.Aws.SecretsManager.Abstraction;

namespace A55.Extensions.Configuration.Aws.SecretsManager.Core;

record KeyPathSettings(
    string EnvironmentName,
    string BasePath,
    IEnumerable<string> PathKeys
);

class KeyEnvPathsRenderer : IKeyPathsRenderer
{
    readonly KeyPathSettings settings;
    readonly string envAlias;

    public KeyEnvPathsRenderer(IEnvAliasMapper mapper, KeyPathSettings settings)
    {
        this.envAlias = mapper.From(settings.EnvironmentName);
        this.settings = settings;
    }

    string MapBase(string path) =>
        path.StartsWith("/")
            ? path
            : $"{settings.BasePath}{path}";

    string MapEnv(string path) =>
        path.EndsWith("/")
            ? path
            : $"{path}/{envAlias}";

    public IEnumerable<string> GetPaths() =>
        settings
            .PathKeys
            .Select(MapBase)
            .Select(MapEnv);
}
