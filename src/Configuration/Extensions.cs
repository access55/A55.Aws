using A55.Extensions.Configuration.Aws;
using A55.Extensions.Configuration.Aws.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace A55.Extensions.Configuration;

static class InternalExtensions
{
    public static bool IsNullOrWhiteSpace(this string? str) => string.IsNullOrWhiteSpace(str);
    public static bool IsNullOrEmpty(this string? str) => string.IsNullOrEmpty(str);
    public static string JoinAsString<T>(this IEnumerable<T> items, string separador) => string.Join(separador, items);
}

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSecretsManager(
        this IConfigurationBuilder builder,
        IHostEnvironment environment)
    {
        var tempConfig = builder.Build();
        var settings = tempConfig.Get<AwsSharedSettings>();
        return builder.AddSecretsManager(settings, environment);
    }

    public static IConfigurationBuilder AddSecretsManager(
        this IConfigurationBuilder builder,
        AwsSharedSettings settings,
        IHostEnvironment environment) =>
        builder.Add(new AwsSecretsManagerConfigurationSource(settings, environment));
}
