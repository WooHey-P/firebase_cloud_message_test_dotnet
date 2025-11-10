namespace FcmSender.Api.Contracts.Requests;

using System.ComponentModel.DataAnnotations;
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

    public FcmNotificationRequest ToDomainRequest()
        => new()
        {
            Title = Title ?? string.Empty,
            Body = Body ?? string.Empty,
            ImageUrl = ImageUrl,
            Token = Token,
            Topic = Topic,
            Condition = Condition,
            Data = Data is null
                ? null
                : new Dictionary<string, string>(Data, StringComparer.Ordinal),
            ValidateOnly = ValidateOnly
        };

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
