using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace A55.Aws.SecretsManager;

public class AwsSettings
{
    [ConfigurationKeyName("ACCESS_KEY_ID_SHARED")]
    public string? AccessKeyIdShared { get; set; }

    [ConfigurationKeyName("SECRET_ACCESS_KEY_SHARED")]
    public string? SecretAccessKeyShared { get; set; }

    [ConfigurationKeyName("SESSION_TOKEN_SHARED")]
    public string? SessionTokenShared { get; set; }

    [ConfigurationKeyName("REGION_SHARED")]
    public string? RegionShared { get; set; }

    public bool ReadSettingsFromSecretsManager { get; set; }
    public string? SecretsManagerProjectKey { get; set; }

    internal AWSCredentials ToCredentials() =>
        string.IsNullOrWhiteSpace(SessionTokenShared)
        ? new BasicAWSCredentials(AccessKeyIdShared, SecretAccessKeyShared)
        : new SessionAWSCredentials(AccessKeyIdShared, SecretAccessKeyShared, SessionTokenShared);
}
