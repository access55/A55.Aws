using A55.Extensions.Configuration.Aws.SecretsManager.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace A55.Extensions.Configuration.Aws.SecretsManager.Core;
sealed class EmptyConfigurationProvider : ConfigurationProvider { }
class SecretsManagerConfigurationSourceBase : IConfigurationSource
{
    readonly IKeyPathsRenderer settings;
    readonly int maxListSecretsSize;
    readonly ISecretsManagerService? secretsManagerService;
    readonly ILogger logger;

    public SecretsManagerConfigurationSourceBase(
        ISecretsManagerService secretsManagerService,
        ILogger logger,
        AwsSharedSettings awsSharedSettings,
        IKeyPathsRenderer settings,
        int maxListSecretsSize
    )
    {
        this.settings = settings;
        this.maxListSecretsSize = maxListSecretsSize;
        this.logger = logger;

        logger.LogInformation("Adding SecretsManager configuration source");
        if (!awsSharedSettings.ReadSettingsFromSecretsManager)
        {
            logger.LogInformation("Skipping SecretsManager configuration provider ({SettingsKey}={SettingsValue})",
                nameof(awsSharedSettings.ReadSettingsFromSecretsManager),
                awsSharedSettings.ReadSettingsFromSecretsManager
            );
            return;
        }

        this.secretsManagerService = secretsManagerService;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        secretsManagerService is null
            ? new EmptyConfigurationProvider()
            : new SecretsManagerConfigurationProvider(secretsManagerService, settings, maxListSecretsSize, logger);
}
