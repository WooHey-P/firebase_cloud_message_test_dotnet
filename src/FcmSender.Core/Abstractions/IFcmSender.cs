namespace FcmSender.Core.Abstractions;

using FcmSender.Core.Models;

/// <summary>
///     Sends Firebase Cloud Messaging notifications.
/// </summary>
public interface IFcmSender
{
    Task<FcmSendResult> SendAsync(FcmNotificationRequest request, CancellationToken cancellationToken = default);
}
