namespace FcmSender.Core.Abstractions;

using Google.Apis.Auth.OAuth2;

/// <summary>
///     Provides access to Google service account credentials configured for Firebase messaging.
/// </summary>
public interface IFirebaseCredentialProvider
{
    Task<GoogleCredential> GetAsync(CancellationToken cancellationToken = default);
}
