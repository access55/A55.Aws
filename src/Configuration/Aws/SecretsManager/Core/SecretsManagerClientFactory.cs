using Amazon.SecretsManager;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace A55.Extensions.Configuration.Aws.SecretsManager.Core;

static class SecretsManagerClientFactory
{
    internal static AWSCredentials GetAwsCredentials(AwsSharedSettings settings) =>
        settings switch
        {
            {
                AccessKeyId: {Length: > 0} key,
                SecretAccessKey: {Length: > 0} secret,
                SessionToken: {Length: > 0} session,
            } => new SessionAWSCredentials(key, secret, session),

            {
                AccessKeyId: {Length: > 0} key,
                SecretAccessKey: {Length: > 0} secret,
            } => new BasicAWSCredentials(key, secret),

            {Profile: {Length: > 0} profileName} when
                new SharedCredentialsFile(settings.SharedCredentialsFile) is { } sharedFile
                && sharedFile.TryGetProfile(profileName, out var profile)
                && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials)
                => credentials,

            _ => throw new InvalidOperationException("No AWS profile found"),
        };

    internal static IAmazonSecretsManager Create(AwsSharedSettings config) =>
        new AmazonSecretsManagerClient(GetAwsCredentials(config), RegionEndpoint.GetBySystemName(config.RegionShared));
}
