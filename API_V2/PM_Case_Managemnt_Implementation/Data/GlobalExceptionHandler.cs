using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PM_Case_Managemnt_Implementation.Helpers.Logger;
using PM_Case_Managemnt_Implementation.Helpers.Response;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace PM_Case_Managemnt_Implementation.Data;
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILoggerManagerService _logger;
    private readonly IHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly ConcurrentDictionary<Type, (string ErrorCode, HttpStatusCode StatusCode)> ExceptionMap = new(new Dictionary<Type, (string, HttpStatusCode)>
        {
            { typeof(InvalidModelStateException), ("ERR_INVALID_MODEL", HttpStatusCode.BadRequest) },
            { typeof(ArgumentNullException), ("ERR_NULL_ARGUMENT", HttpStatusCode.BadRequest) },
            { typeof(ArgumentOutOfRangeException), ("ERR_OUT_OF_RANGE", HttpStatusCode.BadRequest) },
            { typeof(ArgumentException), ("ERR_ARGUMENT", HttpStatusCode.BadRequest) },
            { typeof(FormatException), ("ERR_FORMAT", HttpStatusCode.BadRequest) },
            { typeof(JsonException), ("ERR_JSON_PARSE", HttpStatusCode.BadRequest) },
            { typeof(InvalidOperationException), ("ERR_INVALID_OPERATION", HttpStatusCode.BadRequest) },
            { typeof(NotSupportedException), ("ERR_NOT_SUPPORTED", HttpStatusCode.NotImplemented) },
            { typeof(NotImplementedException), ("ERR_NOT_IMPLEMENTED", HttpStatusCode.NotImplemented) },
            { typeof(TimeoutException), ("ERR_TIMEOUT", HttpStatusCode.RequestTimeout) },
            { typeof(SqlException), ("ERR_SQL", HttpStatusCode.InternalServerError) },
            { typeof(DbUpdateException), ("ERR_DB_UPDATE", HttpStatusCode.InternalServerError) },
            { typeof(DbUpdateConcurrencyException), ("ERR_DB_CONCURRENCY", HttpStatusCode.Conflict) },
            { typeof(UnauthorizedAccessException), ("ERR_UNAUTHORIZED", HttpStatusCode.Unauthorized) },
            { typeof(KeyNotFoundException), ("ERR_NOT_FOUND", HttpStatusCode.NotFound) },
            { typeof(HttpRequestException), ("ERR_HTTP_REQUEST", HttpStatusCode.BadGateway) },
            { typeof(Exception), ("ERR_UNKNOWN", HttpStatusCode.InternalServerError) }
        });

    public GlobalExceptionHandler(ILoggerManagerService logger, IHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();
        var (errorCode, statusCode) = ExceptionMap.GetOrAdd(exceptionType, _ => ExceptionMap[typeof(Exception)]);

        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        _logger.LogException("GlobalExceptionHandler", userId, $"An error occurred: {errorCode}. Details: {exception.Message}");

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        var responseMessage = new ResponseMessage<object>
        {
            Success = false,
            Message = GetErrorMessage(exception, errorCode),
            ErrorCode = errorCode,
            Data = GetExceptionData(exception)
        };

        await JsonSerializer.SerializeAsync(httpContext.Response.Body, responseMessage,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            cancellationToken);

        return true;
    }

    private string GetErrorMessage(Exception ex, string errorCode)
    {
        return errorCode switch
        {
            "ERR_INVALID_MODEL" => "The input data failed validation. Please review and correct your input.",
            "ERR_NULL_ARGUMENT" => "A required argument was null. Please check your input.",
            "ERR_OUT_OF_RANGE" => "An argument was out of the acceptable range. Please check your input.",
            "ERR_ARGUMENT" => "An argument error occurred. Please check your input.",
            "ERR_FORMAT" => "The input was in an incorrect format. Please check the required format and try again.",
            "ERR_JSON_PARSE" => "There was an error parsing the JSON input. Please check your JSON format and try again.",
            "ERR_INVALID_OPERATION" => "An invalid operation was attempted.",
            "ERR_NOT_SUPPORTED" => "The requested operation is not supported.",
            "ERR_NOT_IMPLEMENTED" => "The requested feature is not implemented.",
            "ERR_TIMEOUT" => "The operation timed out. Please try again later or contact support if the issue persists.",
            "ERR_SQL" => "A database error occurred. Please try again later or contact support if the issue persists.",
            "ERR_DB_UPDATE" => "There was an error updating the database. Please try again or contact support if the issue persists.",
            "ERR_DB_CONCURRENCY" => "A concurrency error occurred while updating the database. Please refresh your data and try again.",
            "ERR_UNAUTHORIZED" => "You are not authorized to perform this action. Please check your credentials and permissions.",
            "ERR_NOT_FOUND" => "The requested resource was not found. Please check the identifier and try again.",
            "ERR_HTTP_REQUEST" => "There was an error processing the HTTP request. Please try again later.",
            _ => "An unexpected error occurred. Please try again later or contact support if the issue persists."
        };
    }

    private object GetExceptionData(Exception ex)
    {
        var data = new Dictionary<string, object>();

        if (ex is InvalidModelStateException invalidModelStateEx)
        {
            data["validationErrors"] = invalidModelStateEx.Errors;
        }

        // Only include stack trace in development environment
        if (_env.IsDevelopment())
        {
            data["exceptionType"] = ex.GetType().Name;
            data["stackTrace"] = ex.StackTrace;
        }

        return data.Count > 0 ? data : null;
    }
}

public class InvalidModelStateException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public InvalidModelStateException(Dictionary<string, string[]> errors)
        : base("Invalid model state")
    {
        Errors = errors;
    }
}





