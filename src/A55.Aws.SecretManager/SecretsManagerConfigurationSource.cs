using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
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
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Information)
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

        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;
        logger.LogInformation("Listing secrets for {ApplicationName} on {EnvironmentName}",
            projectName, environmentName);

        var SecretsManagerClient = AmazonSecretsManagerClientFactory.Create(settings);
        var secrets = new AwsSecretsManager(SecretsManagerClient, environmentName, projectName)
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
                Data[$"{dataKey}:{item.Key}"] = item.Value;
        }

        if (secrets.TryGetValue("db", out var dbJsonCredentials))
        {
            logger.LogInformation("Found db key: mapping it to ConnectionStrings:DefaultConnection");
            var credentials = JsonNode.Parse(dbJsonCredentials)?.AsObject();
            if (credentials is null)
                return;

            var defaultConnectionString =
                new NpgsqlConnectionStringBuilder
                {
                    Host = credentials["host"]?.ToString(),
                    Port = int.TryParse(credentials["port"]?.ToString(), out var port) ? port : 5432,
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

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder builder, string? applicationName = null)
    {
        var tempConfig = builder.Build();
        var settings = tempConfig.Get<AwsSettings>();
        var projectName = applicationName ?? tempConfig.GetValue<string>("SecretsManagerProjectKey");

        return builder.Add(new AwsSecretsManagerConfigurationSource(settings, projectName));
    }
}
