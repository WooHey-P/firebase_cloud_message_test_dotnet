namespace FcmSender.Api.Endpoints;

using System.ComponentModel.DataAnnotations;
using FcmSender.Api.Contracts.Requests;
using FcmSender.Api.Contracts.Responses;
using FcmSender.Core.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        group.MapPost("/send", SendAsync)
            .WithName("SendNotification")
            .WithSummary("FCM 단일 메시지 전송")
            .WithDescription("Firebase Cloud Messaging HTTP v1 API를 사용해 단일 기기, 토픽 또는 조건에 메시지를 전송합니다.")
            .Produces<SendNotificationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem()
            .WithOpenApi(operation =>
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Content =
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiObject
                            {
                                ["title"] = new OpenApiString("Hello from server"),
                                ["body"] = new OpenApiString("This message was triggered via the FCM sender API."),
                                ["token"] = new OpenApiString("your-device-token"),
                                ["data"] = new OpenApiObject
                                {
                                    ["customKey"] = new OpenApiString("customValue")
                                },
                                ["validateOnly"] = new OpenApiBoolean(false)
                            }
                        }
                    }
                };

                operation.Responses["200"] = new OpenApiResponse
                {
                    Description = "메시지 전송 성공",
                    Content =
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = nameof(SendNotificationResponse)
                                }
                            },
                            Example = new OpenApiObject
                            {
                                ["messageName"] = new OpenApiString("projects/my-project/messages/1234567890"),
                                ["dryRun"] = new OpenApiBoolean(false)
                            }
                        }
                    }
                };

                return operation;
            });

        return group;
    }

    private static async Task<Results<Ok<SendNotificationResponse>, ValidationProblem>> SendAsync(
        SendNotificationRequest request,
        IFcmSender sender,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRequest(request);
        if (validationErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        try
        {
            var result = await sender.SendAsync(request.ToDomainRequest(), cancellationToken).ConfigureAwait(false);
            return TypedResults.Ok(new SendNotificationResponse(result.MessageName, result.IsDryRun));
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["target"] = new[] { ex.Message }
            });
        }
    }

    private static Dictionary<string, string[]> ValidateRequest(SendNotificationRequest request)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);

        Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true);

        if (validationResults.Count == 0)
        {
            return new Dictionary<string, string[]>();
        }

        return validationResults
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                g => string.IsNullOrWhiteSpace(g.Key) ? "request" : g.Key,
                g => g.Select(r => r.ErrorMessage ?? "유효성 검사 실패").ToArray());
    }
}
