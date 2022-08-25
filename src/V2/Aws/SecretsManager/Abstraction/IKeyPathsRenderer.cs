namespace A55.Extensions.Configuration.Aws.SecretsManager.Core;

interface IKeyPathsRenderer
{
    public IEnumerable<string> GetPaths();
}
