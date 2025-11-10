namespace FcmSender.Core.Models;

using Google.Apis.FirebaseCloudMessaging.v1.Data;

/// <summary>
///     Result describing the outcome of an FCM send request.
/// </summary>
public sealed record FcmSendResult(string MessageName, bool ValidateOnly, Message ResponseMessage)
{
    /// <summary>
    ///     Indicates whether the message was delivered (<c>false</c>) or only validated (<c>true</c>).
    /// </summary>
    public bool IsDryRun => ValidateOnly;
}
