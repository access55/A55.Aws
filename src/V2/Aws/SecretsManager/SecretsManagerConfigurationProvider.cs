using System.Text;
using System.Text.Json.Nodes;
using A55.Extensions.Configuration.Aws.SecretsManager.Abstraction;
using A55.Extensions.Configuration.Aws.SecretsManager.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace A55.Extensions.Configuration.Aws.SecretsManager;

class SecretsManagerConfigurationProvider : ConfigurationProvider
{
    readonly ISecretsManagerService secretsManagerService;
    readonly IKeyPathsRenderer pathsRenderer;
    readonly int maxListSecretsCount;
    readonly ILogger logger;

    internal SecretsManagerConfigurationProvider(
        ISecretsManagerService secretsManagerService,
        IKeyPathsRenderer pathsRenderer,
        int maxListSecretsCount,
        ILogger logger)
    {
        this.secretsManagerService = secretsManagerService;
        this.pathsRenderer = pathsRenderer;
        this.maxListSecretsCount = maxListSecretsCount;
        this.logger = logger;
    }

    public override void Load()
    {
        logger.LogInformation("Listing secrets");
        var keyPaths = pathsRenderer.GetPaths();
        var secrets = secretsManagerService
            .GetProjectSecrets(keyPaths, maxListSecretsCount)
            .GetAwaiter().GetResult();

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

        logger.LogInformation("Finish SecretsManager resource load");
    }
}
