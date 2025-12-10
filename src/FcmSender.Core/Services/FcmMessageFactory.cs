namespace FcmSender.Core.Services;

using FcmSender.Core.Abstractions;
using FcmSender.Core.Models;
using Google.Apis.FirebaseCloudMessaging.v1.Data;

/// <summary>
///     Builds <see cref="Message" /> instances from <see cref="FcmNotificationRequest" /> objects.
/// </summary>
public sealed class FcmMessageFactory : IFcmMessageFactory
{
    public Message Create(FcmNotificationRequest request, string? defaultDeviceToken = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        var message = new Message();

        var hasNotificationPayload = !string.IsNullOrWhiteSpace(request.Title)
                                     || !string.IsNullOrWhiteSpace(request.Body)
                                     || !string.IsNullOrWhiteSpace(request.ImageUrl);

        if (hasNotificationPayload)
        {
            message.Notification = new Notification
            {
                Title = request.Title,
                Body = request.Body,
                Image = request.ImageUrl
            };
        }

        if (request.Data is { Count: > 0 })
        {
            message.Data = new Dictionary<string, string>(request.Data, StringComparer.Ordinal);
        }

        // Apply Android-specific configuration
        if (request.Android is not null)
        {
            message.Android = BuildAndroidConfig(request.Android);
        }

        // Apply APNs-specific configuration
        if (request.Apns is not null)
        {
            message.Apns = BuildApnsConfig(request.Apns);
        }

        var (targetType, targetValue) = ResolveTarget(request, defaultDeviceToken);

        switch (targetType)
        {
            case MessageTargetType.Token:
                message.Token = targetValue;
                break;
            case MessageTargetType.Topic:
                message.Topic = targetValue;
                break;
            case MessageTargetType.Condition:
                message.Condition = targetValue;
                break;
            default:
                throw new InvalidOperationException("Unsupported message target type.");
        }

        return message;
    }

    private static AndroidConfig BuildAndroidConfig(FcmAndroidConfig config)
    {
        var androidConfig = new AndroidConfig();

        if (!string.IsNullOrWhiteSpace(config.Priority))
        {
            androidConfig.Priority = config.Priority.ToUpperInvariant();
        }

        if (!string.IsNullOrWhiteSpace(config.Ttl))
        {
            androidConfig.Ttl = config.Ttl;
        }

        return androidConfig;
    }

    private static ApnsConfig BuildApnsConfig(FcmApnsConfig config)
    {
        var apnsConfig = new ApnsConfig();

        if (config.Headers is not null)
        {
            apnsConfig.Headers = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(config.Headers.ApnsPriority))
            {
                apnsConfig.Headers["apns-priority"] = config.Headers.ApnsPriority;
            }

            if (!string.IsNullOrWhiteSpace(config.Headers.ApnsExpiration))
            {
                apnsConfig.Headers["apns-expiration"] = config.Headers.ApnsExpiration;
            }
        }

        if (config.Payload?.Aps is not null)
        {
            var apsDict = new Dictionary<string, object>();

            if (config.Payload.Aps.ContentAvailable.HasValue)
            {
                apsDict["content-available"] = config.Payload.Aps.ContentAvailable.Value;
            }

            if (apsDict.Count > 0)
            {
                apnsConfig.Payload = new Dictionary<string, object>
                {
                    ["aps"] = apsDict
                };
            }
        }

        return apnsConfig;
    }

    private static (MessageTargetType Type, string Value) ResolveTarget(FcmNotificationRequest request, string? defaultDeviceToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Token))
        {
            return (MessageTargetType.Token, request.Token);
        }

        if (!string.IsNullOrWhiteSpace(request.Topic))
        {
            return (MessageTargetType.Topic, request.Topic);
        }

        if (!string.IsNullOrWhiteSpace(request.Condition))
        {
            return (MessageTargetType.Condition, request.Condition);
        }

        if (!string.IsNullOrWhiteSpace(defaultDeviceToken))
        {
            return (MessageTargetType.Token, defaultDeviceToken);
        }

        throw new InvalidOperationException("At least one of Token, Topic, Condition or a configured default device token must be provided.");
    }

    private enum MessageTargetType
    {
        Token,
        Topic,
        Condition
    }
}
