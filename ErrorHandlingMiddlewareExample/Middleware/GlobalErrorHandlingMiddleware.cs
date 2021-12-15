using System.Text.Json;
using ErrorHandlingMiddlewareExample.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ErrorHandlingMiddlewareExample.Middleware;

public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext, 
        ILoggerFactory loggerFactory,
        ProblemDetailsFactory problemDetailsFactory)
    {
        try
        {
            await _next(httpContext);
        }
        catch (BadRequestException exception)
        {
            ILogger logger = BuildLogger(loggerFactory, exception);
            logger.LogWarning("Bad request had {ErrorCount} error(s): {Errors}", exception.Errors.Length, exception.Errors);

            ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext, 400);
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions.Add("errors", exception.Errors);

            await BuildResponseAsync(httpContext, problemDetails);
        }
        catch (ResourceNotFoundException exception)
        {
            ILogger logger = BuildLogger(loggerFactory, exception);
            logger.LogWarning("Resource not found: {ResourceType} {ResourceId}", exception.ResourceName, exception.ResourceId);

            ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext, 404);
            problemDetails.Detail = exception.Message;

            await BuildResponseAsync(httpContext, problemDetails);
        }
        catch (Exception exception)
        {
            ILogger logger = BuildLogger(loggerFactory, exception);
            logger.LogError(exception, exception.Message);

            ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext, 500);
            problemDetails.Detail = exception.Message;

            await BuildResponseAsync(httpContext, problemDetails);
        }
    }

    private ILogger BuildLogger(ILoggerFactory loggerFactory, Exception exception)
    {
        Type typeThatThrewException = exception.TargetSite?.DeclaringType ?? GetType();
        ILogger logger = loggerFactory.CreateLogger(typeThatThrewException);
        return logger;
    }
    
    private static async Task BuildResponseAsync(HttpContext httpContext, ProblemDetails problemDetails)
    {
        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        var json = JsonSerializer.Serialize(problemDetails);
        await httpContext.Response.WriteAsync(json);
    }
}

public static class ErrorHandlingBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="GlobalErrorHandlingMiddleware"/> to the specified <see cref="IApplicationBuilder"/>,
    /// which handles exceptions for all routes.
    /// </summary>
    /// <param name="builder">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalErrorHandlingMiddleware>();
    }
}