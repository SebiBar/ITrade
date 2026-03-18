using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Authentication;

namespace ITrade.ApiServices.Helpers
{
    public class ExceptionHandlingMiddleware : IExceptionHandler
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            ProblemDetailsFactory problemDetailsFactory,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _problemDetailsFactory = problemDetailsFactory;
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (httpContext.Response.HasStarted)
                return false;

            var statusCode = GetStatusCode(exception);
            var title = GetTitle(exception);

            var problemDetails = _problemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: statusCode,
                title: title,
                detail: exception.Message,
                instance: httpContext.Request.Path
            );

            _logger.Log(
                statusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning,
                exception,
                "Request failed. Method: {Method}; Path: {Path}; StatusCode: {StatusCode}; TraceId: {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                statusCode,
                httpContext.TraceIdentifier);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";
            await Results.Problem(problemDetails).ExecuteAsync(httpContext);

            return true;
        }

        private static int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                AuthenticationException => StatusCodes.Status401Unauthorized,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                InvalidOperationException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static string GetTitle(Exception ex)
        {
            return ex switch
            {
                ArgumentException => "Bad Request",
                KeyNotFoundException => "Not Found",
                AuthenticationException => "Unauthorized",
                UnauthorizedAccessException => "Forbidden",
                InvalidOperationException => "Conflict",
                _ => "Internal Server Error"
            };
        }
    }
}
