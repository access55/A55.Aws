using System.Text;
using System.Text.Json.Nodes;
using A55.Extensions.Configuration.Aws.SecretsManager.Core;
using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Npgsql;

namespace A55.Extensions.Configuration.Aws.SecretsManager;

class SecretsManagerConfigurationProvider : ConfigurationProvider
{
    readonly AwsSharedSettings settings;
    readonly AwsSecretsManager secretsManager;

    internal SecretsManagerConfigurationProvider(
        AwsSecretsManager secretsManager,
        AwsSharedSettings settings)
    {
        this.settings = settings;
        this.secretsManager = secretsManager;
    }

    public override void Load()
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Trace)
            .AddSimpleConsole(c =>
            {
                c.IncludeScopes = true;
                c.SingleLine = true;
                c.ColorBehavior = LoggerColorBehavior.Disabled;
            }));

        var logger = loggerFactory.CreateLogger<SecretsManagerConfigurationProvider>();

        logger.LogInformation("Loading secret manager resources");
        if (!settings.ReadSettingsFromSecretsManager)
        {
            logger.LogInformation("Skipping secret manager resources");
            return;
        }

        logger.LogInformation("Listing secrets");
        var secrets = secretsManager.GetProjectSecrets().GetAwaiter().GetResult();

        foreach (var (key, value) in secrets)
        {
            var dataKey = key.Replace("/", ":");
            if (value.IsNullOrWhiteSpace())
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
                dataKey, jsonData.Keys.JoinAsString(","));

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
