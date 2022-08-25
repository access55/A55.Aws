using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace A55.Aws.SecretsManager;

/// <summary>
/// Settings to load secrets manager data
/// </summary>
public class AwsSettings
{
    /// <summary>
    /// Access Key for the Secrets manager account
    /// </summary>
    [ConfigurationKeyName("ACCESS_KEY_ID_SHARED")]
    public string? AccessKeyIdShared { get; set; }

    /// <summary>
    /// Secret Key for the Secrets manager account
    /// </summary>
    [ConfigurationKeyName("SECRET_ACCESS_KEY_SHARED")]
    public string? SecretAccessKeyShared { get; set; }

    /// <summary>
    /// Session Token for the Secrets manager account
    /// </summary>
    [ConfigurationKeyName("SESSION_TOKEN_SHARED")]
    public string? SessionTokenShared { get; set; }

    /// <summary>
    /// Secrets manager regegion
    /// </summary>
    [ConfigurationKeyName("REGION_SHARED")]
    public string? RegionShared { get; set; }

    /// <summary>
    /// Enable reading Secrets Manager on startup
    /// </summary>
    public bool ReadSettingsFromSecretsManager { get; set; }

    /// <summary>
    /// The application name to set keys on secrets manager
    /// the default paths are
    /// "/settings/shared", "/settings/{SecretsManagerProjectKey}/shared/", "/settings/{SecretsManagerProjectKey}/{envAlias}/"
    /// the {envAlias} is defined using ASPNETCORE_ENVIRONMENT default env name, mapping to dev, stg and prd
    /// </summary>
    public string? SecretsManagerProjectKey { get; set; }

    internal AWSCredentials ToCredentials() =>
        string.IsNullOrWhiteSpace(SessionTokenShared)
        ? new BasicAWSCredentials(AccessKeyIdShared, SecretAccessKeyShared)
        : new SessionAWSCredentials(AccessKeyIdShared, SecretAccessKeyShared, SessionTokenShared);
}
