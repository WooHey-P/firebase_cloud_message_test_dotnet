namespace FcmSender.Api.Contracts.Responses;

/// <summary>
///     API response for FCM send operations.
/// </summary>
public sealed record SendNotificationResponse(string MessageName, bool DryRun);
