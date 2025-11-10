namespace FcmSender.Core.Abstractions;

using FcmSender.Core.Models;
using Google.Apis.FirebaseCloudMessaging.v1.Data;

/// <summary>
///     Builds Google FCM messages from domain requests.
/// </summary>
public interface IFcmMessageFactory
{
    Message Create(FcmNotificationRequest request, string? defaultDeviceToken = null);
}
