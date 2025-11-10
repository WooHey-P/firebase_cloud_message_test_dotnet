namespace FcmSender.Core.Services;

using FcmSender.Core.Abstractions;
using FcmSender.Core.Models;
using FcmSender.Core.Options;
using Google;
using Google.Apis.FirebaseCloudMessaging.v1;
using Google.Apis.FirebaseCloudMessaging.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
///     Sends messages through the Google Firebase Cloud Messaging API.
/// </summary>
public sealed class FirebaseMessagingSender : IFcmSender
{
    private readonly IFirebaseCredentialProvider _credentialProvider;
    private readonly IFcmMessageFactory _messageFactory;
    private readonly FirebaseOptions _options;
    private readonly ILogger<FirebaseMessagingSender> _logger;

    public FirebaseMessagingSender(
        IFirebaseCredentialProvider credentialProvider,
        IFcmMessageFactory messageFactory,
        IOptions<FirebaseOptions> options,
        ILogger<FirebaseMessagingSender> logger)
    {
        _credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
        _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FcmSendResult> SendAsync(FcmNotificationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var credential = await _credentialProvider.GetAsync(cancellationToken).ConfigureAwait(false);

        using var messagingService = CreateMessagingService(credential);

        Message message;
        try
        {
            message = _messageFactory.Create(request, _options.DefaultDeviceToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create FCM message payload.");
            throw;
        }

        var projectPath = $"projects/{_options.ProjectId}";
        var sendRequest = messagingService.Projects.Messages.Send(
            new SendMessageRequest
            {
                Message = message,
                ValidateOnly = request.ValidateOnly
            },
            projectPath);

        try
        {
            var response = await sendRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            var messageName = response.Name ?? string.Empty;

            _logger.LogInformation(
                "FCM message {MessageName} sent (validateOnly: {ValidateOnly}) targeting project {ProjectId}.",
                messageName,
                request.ValidateOnly,
                _options.ProjectId);

            return new FcmSendResult(messageName, request.ValidateOnly, response);
        }
        catch (GoogleApiException apiEx)
        {
            _logger.LogError(
                apiEx,
                "Firebase returned an error while sending the message to project {ProjectId}. Status: {StatusCode}",
                _options.ProjectId,
                apiEx.HttpStatusCode);
            throw;
        }
    }

    private FirebaseCloudMessagingService CreateMessagingService(Google.Apis.Auth.OAuth2.GoogleCredential credential)
        => new(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = $"FcmSender ({_options.ProjectId})"
        });
}
