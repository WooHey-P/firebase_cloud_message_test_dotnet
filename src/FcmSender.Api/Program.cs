using DotNetEnv;
using FcmSender.Api.Endpoints;
using FcmSender.Core.Abstractions;
using FcmSender.Core.Options;
using FcmSender.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

TryLoadDotEnv();

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
