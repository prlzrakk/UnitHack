using System.Text.Json;
using Api.Application.Common.Exceptions;
using FluentValidation;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var response = CreateErrorResponse(exception);

        if (response.StatusCode >= 500)
        {
            logger.LogError(exception, "Unhandled exception occurred");
        }
        else
        {
            logger.LogWarning(exception, "Handled exception occurred");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }

    private static ErrorResponse CreateErrorResponse(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .ToArray());

            return new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = "validation_error",
                Message = "Validation failed",
                Errors = errors
            };
        }

        if (exception is ApiException apiException)
        {
            return new ErrorResponse
            {
                StatusCode = apiException.StatusCode,
                ErrorCode = apiException.ErrorCode,
                Message = apiException.Message
            };
        }

        if (exception is UnauthorizedAccessException)
        {
            return new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                ErrorCode = "unauthorized",
                Message = "Unauthorized"
            };
        }

        return new ErrorResponse
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            ErrorCode = "internal_server_error",
            Message = "Internal server error"
        };
    }
}