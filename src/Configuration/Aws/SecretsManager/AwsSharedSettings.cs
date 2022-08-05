using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;

namespace A55.Extensions.Configuration.Aws.SecretsManager;

public class AwsSharedSettings
{
    internal const string DefaultProfile = "default";

    [ConfigurationKeyName("ACCESS_KEY_ID_SHARED")]
    public string? AccessKeyId { get; set; }

    [ConfigurationKeyName("SECRET_ACCESS_KEY_SHARED")]
    public string? SecretAccessKey { get; set; }

    [ConfigurationKeyName("SESSION_TOKEN_SHARED")]
    public string? SessionToken { get; set; }

    [ConfigurationKeyName("REGION_SHARED")]
    public string? RegionShared { get; set; }

    public string? Profile { get; set; } = DefaultProfile;

    public string? SharedCredentialsFile { get; set; }

    public bool ReadSettingsFromSecretsManager { get; set; }
}
