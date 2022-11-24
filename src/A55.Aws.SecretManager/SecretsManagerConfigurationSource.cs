using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Npgsql;

namespace A55.Aws.SecretsManager;

class AwsSecretsManagerConfigurationSource : IConfigurationSource
{
    readonly AwsSettings settings;
    readonly string projectName;

    public AwsSecretsManagerConfigurationSource(
        AwsSettings settings,
        string projectName)
    {
        this.settings = settings;
        this.projectName = projectName;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new AwsSecretsManagerConfigurationProvider(settings, projectName);
}

class AwsSecretsManagerConfigurationProvider : ConfigurationProvider
{
    readonly AwsSettings settings;
    readonly string projectName;

    public AwsSecretsManagerConfigurationProvider(AwsSettings settings, string projectName)
    {
        this.settings = settings;
        this.projectName = projectName;
    }

    public override void Load()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                              Environments.Development;

        using var configuration = new ConfigurationManager();
        configuration
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                         Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true,
                reloadOnChange: true)
            .AddEnvironmentVariables();

        var logLevel = configuration.GetValue<LogLevel?>("Logging:LogLevel:Default") ??
                       LogLevel.Information;
        using var loggerFactory =
            LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(logLevel)
                .AddSimpleConsole(c =>
                {
                    c.IncludeScopes = true;
                    c.SingleLine = true;
                    c.ColorBehavior = LoggerColorBehavior.Disabled;
                }));

        var logger = loggerFactory.CreateLogger<AwsSecretsManagerConfigurationProvider>();

        logger.LogInformation("Loading secret manager resources");
        if (!settings.ReadSettingsFromSecretsManager)
        {
            logger.LogInformation("Skipping secret manager resources");
            return;
        }

        logger.LogInformation("Listing secrets for {ApplicationName} on {EnvironmentName}",
            projectName, environmentName);

        var secretsManagerClient = AmazonSecretsManagerClientFactory.Create(settings);
        var secrets = new AwsSecretsManager(secretsManagerClient, environmentName, projectName)
            .GetProjectSecrets().GetAwaiter().GetResult();

        foreach (var (key, value) in secrets)
        {
            var dataKey = key.Replace("/", ":");
            if (string.IsNullOrWhiteSpace(value))
            {
                logger.LogInformation("Key {DataKey} is empty", dataKey);
                continue;
            }

            var jsonData =
                new ConfigurationBuilder()
                    .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(value)))
                    .Build()
                    .AsEnumerable()
                    .ToDictionary(x => x.Key, x => x.Value);

            logger.LogInformation("Loading settings key {DataKey} with values of {{{Keys}}}",
                dataKey, string.Join(",", jsonData.Keys));

            foreach (var item in jsonData)
            {
                if (item.Value is null)
                    continue;

                var keyPath = dataKey is ":" ? item.Key : $"{dataKey}:{item.Key}";
                Data[keyPath] = item.Value;
            }
        }

        if (secrets.TryGetValue("db", out var dbJsonCredentials))
        {
            logger.LogInformation(
                "Found db key: mapping it to ConnectionStrings:DefaultConnection");
            var credentials = JsonNode.Parse(dbJsonCredentials)?.AsObject();
            if (credentials is null)
                return;

            var defaultConnectionString =
                new NpgsqlConnectionStringBuilder
                {
                    Host = credentials["host"]?.ToString(),
                    Port =
                        int.TryParse(credentials["port"]?.ToString(), out var port) ? port : 5432,
                    Username = credentials["user"]?.ToString(),
                    Password = credentials["password"]?.ToString(),
                    Database = credentials["name"]?.ToString(),
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true
                };
            Data["ConnectionStrings:DefaultConnection"] = defaultConnectionString.ToString();
        }

        logger.LogInformation("Finish secret manager resource load");
    }
}

/// <summary>
/// Secrets Manager Configuration extensions
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Add Secrets Manager Configuration source
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder builder,
        string? applicationName = null)
    {
        var tempConfig = builder.Build();
        var settings = tempConfig.Get<AwsSettings>();
        var projectName =
            applicationName ?? tempConfig.GetValue<string>("SecretsManagerProjectKey");

        return builder.Add(new AwsSecretsManagerConfigurationSource(settings, projectName));
    }
}
