using System.Net;
using System.Text.Json;
using FluentValidation;
using codebase.Common.Exceptions;
using CustomValidationException = codebase.Common.Exceptions.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace codebase.Middleware;

/// <summary>
/// Global exception handling middleware with enhanced validation error handling
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
            
            // Handle authentication/authorization failures from status codes
            if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
            {
                await HandleAuthenticationFailureAsync(context);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        object response;
        HttpStatusCode statusCode;

        switch (exception)
        {
            case FluentValidationException fluentValidationException:
                // FluentValidation errors
                _logger.LogWarning("Validation failed: {Errors}", 
                    string.Join(", ", fluentValidationException.Errors.Select(e => e.ErrorMessage)));
                
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    error = "Validation failed",
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    errors = fluentValidationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                };
                break;

            case CustomValidationException customValidationException:
                // Custom validation errors
                _logger.LogWarning("Custom validation failed: {Errors}", 
                    string.Join(", ", customValidationException.Errors.SelectMany(e => e.Value)));
                
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    error = "Validation failed",
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    errors = customValidationException.Errors
                };
                break;

            case MissingCredentialsException missingCredentialsException:
                _logger.LogWarning("Missing credentials: {Message}", missingCredentialsException.Message);
                statusCode = HttpStatusCode.Unauthorized;
                response = new
                {
                    error = missingCredentialsException.Message,
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    hint = "Please provide authentication credentials in the Authorization header"
                };
                break;

            case BadRequestException badRequestException:
                _logger.LogWarning("Bad request: {Message}", badRequestException.Message);
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    error = badRequestException.Message,
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow
                };
                break;

            case NotFoundException notFoundException:
                _logger.LogWarning("Resource not found: {Message}", notFoundException.Message);
                statusCode = HttpStatusCode.NotFound;
                response = new
                {
                    error = notFoundException.Message,
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow
                };
                break;

            case UnauthorizedException unauthorizedException:
                _logger.LogWarning("Unauthorized access: {Message}", unauthorizedException.Message);
                statusCode = HttpStatusCode.Unauthorized;
                response = new
                {
                    error = unauthorizedException.Message,
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    hint = "Please provide valid authentication credentials"
                };
                break;

            case ForbiddenException forbiddenException:
                _logger.LogWarning("Forbidden access: {Message}", forbiddenException.Message);
                statusCode = HttpStatusCode.Forbidden;
                response = new
                {
                    error = forbiddenException.Message,
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    hint = "You do not have permission to access this resource"
                };
                break;

            case ConflictException conflictException:
                _logger.LogWarning("Conflict: {Message}", conflictException.Message);
                statusCode = HttpStatusCode.Conflict;
                response = new
                {
                    error = conflictException.Message,
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow
                };
                break;

            case ArgumentNullException argumentNullException:
                _logger.LogError(argumentNullException, "Missing required argument: {ParamName}", 
                    argumentNullException.ParamName);
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    error = "Missing required field",
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    field = argumentNullException.ParamName,
                    message = $"The field '{argumentNullException.ParamName}' is required"
                };
                break;

            case ArgumentException argumentException:
                _logger.LogError(argumentException, "Invalid argument: {Message}", argumentException.Message);
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    error = "Invalid input",
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    message = argumentException.Message
                };
                break;

            case InvalidOperationException invalidOperationException:
                _logger.LogError(invalidOperationException, "Invalid operation: {Message}", 
                    invalidOperationException.Message);
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    error = "Invalid operation",
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    message = invalidOperationException.Message
                };
                break;

            case KeyNotFoundException keyNotFoundException:
                _logger.LogWarning("Key not found: {Message}", keyNotFoundException.Message);
                statusCode = HttpStatusCode.NotFound;
                response = new
                {
                    error = "Resource not found",
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    message = keyNotFoundException.Message
                };
                break;

            default:
                // Unhandled exceptions
                _logger.LogError(exception, "An unhandled error occurred: {Message}", exception.Message);
                statusCode = HttpStatusCode.InternalServerError;
                response = new
                {
                    error = "An unexpected error occurred",
                    statusCode = (int)statusCode,
                    timestamp = DateTime.UtcNow,
                    message = "Please contact support if the problem persists"
                };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private async Task HandleAuthenticationFailureAsync(HttpContext context)
    {
        var response = new
        {
            error = "Authentication required",
            statusCode = 401,
            timestamp = DateTime.UtcNow,
            message = "Please provide a valid authentication token",
            hint = "Include 'Authorization: Bearer <token>' header in your request"
        };

        context.Response.ContentType = "application/json";

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
