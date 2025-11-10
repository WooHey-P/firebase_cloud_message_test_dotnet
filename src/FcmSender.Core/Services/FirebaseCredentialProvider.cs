namespace FcmSender.Core.Services;

using System.Text;
using FcmSender.Core.Abstractions;
using FcmSender.Core.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.FirebaseCloudMessaging.v1;
using Microsoft.Extensions.Options;

/// <summary>
///     Loads <see cref="GoogleCredential" /> instances from configuration or well-known environment variables.
/// </summary>
public sealed class FirebaseCredentialProvider : IFirebaseCredentialProvider
{
    private readonly FirebaseOptions _options;
    private readonly object _syncRoot = new();
    private GoogleCredential? _cachedCredential;

    public FirebaseCredentialProvider(IOptions<FirebaseOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));
    }

    public Task<GoogleCredential> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_cachedCredential is not null)
        {
            return Task.FromResult(_cachedCredential);
        }

        lock (_syncRoot)
        {
            if (_cachedCredential is null)
            {
                _cachedCredential = CreateScopedCredential();
            }

            return Task.FromResult(_cachedCredential);
        }
    }

    private GoogleCredential CreateScopedCredential()
    {
        var (json, filePath) = ResolveCredentialSource();

        GoogleCredential credential = !string.IsNullOrWhiteSpace(json)
            ? GoogleCredential.FromJson(json)
            : GoogleCredential.FromFile(filePath!);

        return credential.CreateScoped(FirebaseCloudMessagingService.Scope.FirebaseMessaging);
    }

    private (string? Json, string? FilePath) ResolveCredentialSource()
    {
        string? json = _options.Credentials.Json;
        string? jsonBase64 = _options.Credentials.JsonBase64;
        string? filePath = _options.Credentials.FilePath;

        if (string.IsNullOrWhiteSpace(json))
        {
            json = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON");
        }

        if (string.IsNullOrWhiteSpace(jsonBase64))
        {
            jsonBase64 = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON_BASE64");
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        }

        if (!string.IsNullOrWhiteSpace(jsonBase64) && string.IsNullOrWhiteSpace(json))
        {
            json = Encoding.UTF8.GetString(Convert.FromBase64String(jsonBase64));
        }

        if (!string.IsNullOrWhiteSpace(json))
        {
            return (json, null);
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new InvalidOperationException(
                "Firebase credentials were not found. Configure Firebase:Credentials or set GOOGLE_APPLICATION_CREDENTIALS / GOOGLE_APPLICATION_CREDENTIALS_JSON.");
        }

        var absolutePath = Path.GetFullPath(filePath);

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"Firebase service account file was not found: {absolutePath}", absolutePath);
        }

        return (null, absolutePath);
    }
}
