using Amazon.SecretsManager;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace A55.Extensions.Configuration.Aws.SecretsManager.Core;

static class AmazonSecretsManagerClientFactory
{
    static AWSCredentials? TryGetAwsCredentials(AwsSharedSettings settings) =>
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

            _ => null
        };

    static RegionEndpoint? TryEndpoint(AwsSharedSettings config) =>
        config.RegionShared is not null
            ? RegionEndpoint.GetBySystemName(config.RegionShared)
            : null;

    internal static IAmazonSecretsManager Create(AwsSharedSettings config) =>
        (TryGetAwsCredentials(config), TryEndpoint(config)) switch
        {
            ({ } credentials, { } endpoint) =>
                new AmazonSecretsManagerClient(credentials, endpoint),
            (null, { } endpoint) =>
                new AmazonSecretsManagerClient(endpoint),
            _ =>
                new AmazonSecretsManagerClient()
        };
}
