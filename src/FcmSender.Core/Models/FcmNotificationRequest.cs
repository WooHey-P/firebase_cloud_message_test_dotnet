namespace FcmSender.Core.Models;

/// <summary>
///     Domain-level request describing an outgoing FCM message.
/// </summary>
public sealed record FcmNotificationRequest
{
    /// <summary>
    ///     Notification title shown on the device.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    ///     Notification body shown on the device.
    /// </summary>
    public string Body { get; init; } = string.Empty;

    /// <summary>
    ///     Optional image URL to display with the notification.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    ///     Direct device token (takes precedence over Topic/Condition).
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    ///     Topic name to broadcast the message to.
    /// </summary>
    public string? Topic { get; init; }

    /// <summary>
    ///     Condition string (see FCM docs) to target multiple topics.
    /// </summary>
    public string? Condition { get; init; }

    /// <summary>
    ///     Arbitrary key/value data payload.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Data { get; init; }

    /// <summary>
    ///     When true, FCM validates without delivering (dry-run).
    /// </summary>
    public bool ValidateOnly { get; init; }

    /// <summary>
    ///     Android-specific configuration.
    /// </summary>
    public FcmAndroidConfig? Android { get; init; }

    /// <summary>
    ///     Apple Push Notification Service (APNs) specific configuration.
    /// </summary>
    public FcmApnsConfig? Apns { get; init; }
}

/// <summary>
///     Android-specific message configuration.
/// </summary>
public sealed record FcmAndroidConfig
{
    /// <summary>
    ///     Message priority. Valid values are "normal" and "high".
    /// </summary>
    public string? Priority { get; init; }

    /// <summary>
    ///     How long (in seconds) the message should be kept in FCM storage if the device is offline.
    ///     Format: duration string like "3600s".
    /// </summary>
    public string? Ttl { get; init; }
}

/// <summary>
///     Apple Push Notification Service (APNs) specific configuration.
/// </summary>
public sealed record FcmApnsConfig
{
    /// <summary>
    ///     HTTP request headers for APNs.
    /// </summary>
    public FcmApnsHeaders? Headers { get; init; }

    /// <summary>
    ///     APNs payload including the aps dictionary.
    /// </summary>
    public FcmApnsPayload? Payload { get; init; }
}

/// <summary>
///     APNs HTTP headers.
/// </summary>
public sealed record FcmApnsHeaders
{
    /// <summary>
    ///     The priority of the notification. "10" for immediate, "5" for normal.
    /// </summary>
    public string? ApnsPriority { get; init; }

    /// <summary>
    ///     UNIX epoch timestamp when the notification expires.
    /// </summary>
    public string? ApnsExpiration { get; init; }
}

/// <summary>
///     APNs payload.
/// </summary>
public sealed record FcmApnsPayload
{
    /// <summary>
    ///     The aps dictionary for APNs.
    /// </summary>
    public FcmAps? Aps { get; init; }
}

/// <summary>
///     The aps dictionary contents.
/// </summary>
public sealed record FcmAps
{
    /// <summary>
    ///     Set to 1 to enable background updates (silent push).
    /// </summary>
    public int? ContentAvailable { get; init; }
}
