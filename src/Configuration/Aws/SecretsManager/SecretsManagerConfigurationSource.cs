using A55.Extensions.Configuration.Aws.SecretsManager.Core;
using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace A55.Extensions.Configuration.Aws.SecretsManager;

public sealed class AwsSecretsManagerConfigurationSource : IConfigurationSource
{
    readonly AwsSharedSettings settings;
    readonly IHostEnvironment environment;
    readonly AwsSecretsManager secretsManager;

    public AwsSecretsManagerConfigurationSource(
        AwsSharedSettings settings,
        IHostEnvironment environment) :
        this(settings, environment, SecretsManagerClientFactory.Create(settings))
    {
    }

    public AwsSecretsManagerConfigurationSource(
        AwsSharedSettings settings,
        IHostEnvironment environment,
        IAmazonSecretsManager secretsManager)
    {
        this.settings = settings;
        this.environment = environment;
        this.secretsManager =
            new AwsSecretsManager(secretsManager, environment.EnvironmentName, environment.ApplicationName);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new SecretsManagerConfigurationProvider(secretsManager, settings);
}
