namespace FcmSender.Api.Contracts.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FcmSender.Core.Models;

/// <summary>
///     API contract for sending Firebase notifications.
/// </summary>
public sealed class SendNotificationRequest : IValidatableObject
{
    /// <summary>
    ///     Notification title shown on supported clients.
    /// </summary>
    [StringLength(200)]
    public string? Title { get; init; }

    /// <summary>
    ///     Notification body text.
    /// </summary>
    [StringLength(2_000)]
    public string? Body { get; init; }

    /// <summary>
    ///     Optional image URL for rich notifications.
    /// </summary>
    [Url]
    public string? ImageUrl { get; init; }

    /// <summary>
    ///     Direct device registration token. Takes precedence over topic/condition.
    /// </summary>
    [StringLength(4_096)]
    public string? Token { get; init; }

    /// <summary>
    ///     Topic name to broadcast to (without /topics/ prefix).
    /// </summary>
    [StringLength(256)]
    public string? Topic { get; init; }

    /// <summary>
    ///     Condition expression targeting multiple topics.
    /// </summary>
    [StringLength(1_024)]
    public string? Condition { get; init; }

    /// <summary>
    ///     Optional key/value data payload.
    /// </summary>
    public IDictionary<string, string>? Data { get; init; }

    /// <summary>
    ///     When true, Google validates the message without delivering it.
    /// </summary>
    public bool ValidateOnly { get; init; }

    /// <summary>
    ///     Nested notification object (alternative to top-level title/body).
    /// </summary>
    public NotificationPayload? Notification { get; init; }

    /// <summary>
    ///     Android-specific configuration.
    /// </summary>
    public AndroidConfigRequest? Android { get; init; }

    /// <summary>
    ///     Apple Push Notification Service (APNs) specific configuration.
    /// </summary>
    public ApnsConfigRequest? Apns { get; init; }

    public FcmNotificationRequest ToDomainRequest()
    {
        // Prefer nested notification object, fall back to top-level properties
        var title = Notification?.Title ?? Title ?? string.Empty;
        var body = Notification?.Body ?? Body ?? string.Empty;
        var imageUrl = Notification?.Image ?? ImageUrl;

        return new FcmNotificationRequest
        {
            Title = title,
            Body = body,
            ImageUrl = imageUrl,
            Token = Token,
            Topic = Topic,
            Condition = Condition,
            Data = Data is null
                ? null
                : new Dictionary<string, string>(Data, StringComparer.Ordinal),
            ValidateOnly = ValidateOnly,
            Android = Android?.ToDomain(),
            Apns = Apns?.ToDomain()
        };
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Data is { Count: > 0 })
        {
            foreach (var (key, value) in Data)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    yield return new ValidationResult("데이터 페이로드의 키는 비어 있을 수 없습니다.", new[] { nameof(Data) });
                    break;
                }

                if (value is null)
                {
                    yield return new ValidationResult("데이터 페이로드의 값은 null일 수 없습니다.", new[] { nameof(Data) });
                    break;
                }
            }
        }
    }
}

/// <summary>
///     Nested notification payload.
/// </summary>
public sealed class NotificationPayload
{
    public string? Title { get; init; }
    public string? Body { get; init; }
    public string? Image { get; init; }
}

/// <summary>
///     Android-specific configuration for API contract.
/// </summary>
public sealed class AndroidConfigRequest
{
    public string? Priority { get; init; }
    public string? Ttl { get; init; }

    public FcmAndroidConfig ToDomain() => new()
    {
        Priority = Priority,
        Ttl = Ttl
    };
}

/// <summary>
///     APNs-specific configuration for API contract.
/// </summary>
public sealed class ApnsConfigRequest
{
    public ApnsHeadersRequest? Headers { get; init; }
    public ApnsPayloadRequest? Payload { get; init; }

    public FcmApnsConfig ToDomain() => new()
    {
        Headers = Headers?.ToDomain(),
        Payload = Payload?.ToDomain()
    };
}

/// <summary>
///     APNs headers for API contract.
/// </summary>
public sealed class ApnsHeadersRequest
{
    [JsonPropertyName("apns-priority")]
    public string? ApnsPriority { get; init; }

    [JsonPropertyName("apns-expiration")]
    public string? ApnsExpiration { get; init; }

    public FcmApnsHeaders ToDomain() => new()
    {
        ApnsPriority = ApnsPriority,
        ApnsExpiration = ApnsExpiration
    };
}

/// <summary>
///     APNs payload for API contract.
/// </summary>
public sealed class ApnsPayloadRequest
{
    public ApsRequest? Aps { get; init; }

    public FcmApnsPayload ToDomain() => new()
    {
        Aps = Aps?.ToDomain()
    };
}

/// <summary>
///     APS dictionary for API contract.
/// </summary>
public sealed class ApsRequest
{
    [JsonPropertyName("content-available")]
    public int? ContentAvailable { get; init; }

    public FcmAps ToDomain() => new()
    {
        ContentAvailable = ContentAvailable
    };
}
