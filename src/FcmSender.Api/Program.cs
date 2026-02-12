using System.Text.Json;
using DotNetEnv;
using FcmSender.Api.Endpoints;
using FcmSender.Core.Abstractions;
using FcmSender.Core.Options;
using FcmSender.Core.Services;
using Microsoft.OpenApi.Models;

TryLoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

// 환경 변수를 Configuration에 추가
builder.Configuration.AddEnvironmentVariables();

// FIREBASE_PROJECTID, FIREBASE_DEFAULTDEVICETOKEN 환경 변수를 Firebase 섹션에 매핑
MapFirebaseEnvironmentVariables(builder.Configuration);

builder.Services.AddOptions<FirebaseOptions>()
    .BindConfiguration(FirebaseOptions.SectionName)
    .ValidateDataAnnotations()
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ProjectId),
        $"{FirebaseOptions.SectionName}:ProjectId 설정은 필수입니다.");

builder.Services.AddSingleton<IFirebaseCredentialProvider, FirebaseCredentialProvider>();
builder.Services.AddSingleton<IFcmMessageFactory, FcmMessageFactory>();
builder.Services.AddScoped<IFcmSender, FirebaseMessagingSender>();

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FCM Sender API",
        Version = "v1",
        Description = "Firebase Cloud Messaging(Firebase) 서버 메시지 전송 API"
    });
    options.SupportNonNullableReferenceTypes();
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FCM Sender API v1");
    options.DocumentTitle = "FCM Sender API";
});

app.MapNotificationEndpoints();

app.Run();

static void TryLoadDotEnv()
{
    try
    {
        Env.TraversePath().Load();
    }
    catch (FileNotFoundException)
    {
        // Ignore when .env is absent.
    }
}

static void MapFirebaseEnvironmentVariables(IConfigurationManager configuration)
{
    var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECTID");
    var defaultDeviceToken = Environment.GetEnvironmentVariable("FIREBASE_DEFAULTDEVICETOKEN");

    // ProjectId가 환경 변수에 없으면 서비스 계정 JSON에서 추출
    if (string.IsNullOrWhiteSpace(projectId))
    {
        projectId = TryExtractProjectIdFromServiceAccount(configuration);
    }

    if (!string.IsNullOrWhiteSpace(projectId))
    {
        configuration[$"{FirebaseOptions.SectionName}:ProjectId"] = projectId;
    }

    if (!string.IsNullOrWhiteSpace(defaultDeviceToken))
    {
        configuration[$"{FirebaseOptions.SectionName}:DefaultDeviceToken"] = defaultDeviceToken;
    }
}

static string? TryExtractProjectIdFromServiceAccount(IConfiguration configuration)
{
    try
    {
        // 서비스 계정 JSON 파일 경로 확인
        var credentialFilePath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")
            ?? configuration["Firebase:Credentials:FilePath"];

        if (string.IsNullOrWhiteSpace(credentialFilePath))
        {
            return null;
        }

        var absolutePath = Path.GetFullPath(credentialFilePath);

        if (!File.Exists(absolutePath))
        {
            return null;
        }

        // JSON 파일에서 project_id 추출
        var jsonContent = File.ReadAllText(absolutePath);
        using var jsonDoc = JsonDocument.Parse(jsonContent);

        if (jsonDoc.RootElement.TryGetProperty("project_id", out var projectIdElement))
        {
            return projectIdElement.GetString();
        }
    }
    catch
    {
        // 에러 발생 시 null 반환 (나중에 검증 단계에서 오류 처리)
    }

    return null;
}
