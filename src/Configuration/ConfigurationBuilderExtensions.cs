using A55.Extensions.Configuration.Aws.SecretsManager;
using A55.Extensions.Configuration.Aws.SecretsManager.Core;
using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace A55.Extensions.Configuration;

public static class ConfigurationBuilderExtensions
{
    const int DefaultMaxListSecretsCount = 100;
    const string DefaultRootPath = "settings";

    static IEnumerable<string> DefaultPaths => new[] {"shared"};

    public static IConfigurationBuilder AddSecretsManager(
        this IConfigurationBuilder builder,
        IHostEnvironment environment,
        params string[] paths
    ) => builder.AddSecretsManager(
        environment.EnvironmentName,
        environment.ApplicationName,
        paths: DefaultPaths
            .Concat(paths)
            .Append(environment.ApplicationName.ToLower()));

    public static IConfigurationBuilder AddSecretsManager(
        this IConfigurationBuilder builder,
        IHostEnvironment environment
    ) => builder.AddSecretsManager(
        environment.EnvironmentName,
        environment.ApplicationName);

    public static IConfigurationBuilder AddSecretsManager(
        this IConfigurationBuilder builder,
        string environmentName,
        string applicationName,
        string rootPath = DefaultRootPath,
        AwsSharedSettings? settings = null,
        IAmazonSecretsManager? secretsManagerClient = null,
        IEnumerable<string>? paths = null,
        int maxListSecretsCount = DefaultMaxListSecretsCount
    )
    {
        var sharedSettings = settings ?? GetAwsSharedSettings(builder);
        return builder.Add(
            new SecretsManagerConfigurationSource(
                sharedSettings,
                new KeyPathSettings(
                    environmentName,
                    rootPath,
                    paths ?? DefaultPaths.Append(applicationName.ToLower())
                ),
                secretsManagerClient ?? AmazonSecretsManagerClientFactory.Create(sharedSettings),
                maxListSecretsCount)
        );
    }

    static AwsSharedSettings GetAwsSharedSettings(IConfigurationBuilder builder)
    {
        var tempConfig = builder.Build();
        var settings = tempConfig.Get<AwsSharedSettings>();
        return settings;
    }
}
