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
}
