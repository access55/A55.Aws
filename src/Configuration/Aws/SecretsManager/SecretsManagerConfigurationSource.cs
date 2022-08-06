using A55.Extensions.Configuration.Aws.SecretsManager.Core;
using Amazon.SecretsManager;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace A55.Extensions.Configuration.Aws.SecretsManager;

sealed class SecretsManagerConfigurationSource : SecretsManagerConfigurationSourceBase
{
    public SecretsManagerConfigurationSource(
        AwsSharedSettings settings,
        KeyPathSettings keyPathSettings,
        IAmazonSecretsManager secretsManagerClient,
        int maxListSecretsCount
    ) : base(
        new SecretsManagerService(secretsManagerClient),
        GetLogger(),
        settings,
        new KeyEnvPathsRenderer(new EnvAliasMapper(), keyPathSettings),
        maxListSecretsCount
    )
    {
    }

    static ILogger GetLogger()
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddSimpleConsole(c =>
            {
                c.IncludeScopes = true;
                c.SingleLine = true;
                c.ColorBehavior = LoggerColorBehavior.Disabled;
            }));

        return loggerFactory.CreateLogger<SecretsManagerConfigurationSource>();
    }
}
