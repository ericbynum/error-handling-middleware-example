using System.Net.Mime;
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

    public async Task InvokeAsync(HttpContext httpContext, ILogger<GlobalErrorHandlingMiddleware> logger)
    {
        try
        {
            await _next(httpContext);
        }
        catch (BadRequestException exception)
        {
            ProblemDetails problemDetails = BuildProblemDetails(httpContext, 400);
            problemDetails.Extensions.Add("errors", exception.Errors);
            await WriteErrorToResponseAsync(httpContext, problemDetails);
        }
        catch (ResourceNotFoundException exception)
        {
            ProblemDetails problemDetails = BuildProblemDetails(httpContext, 404);
            problemDetails.Detail = $"{exception.ResourceName}: {exception.ResourceId} not found.";
            await WriteErrorToResponseAsync(httpContext, problemDetails);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, exception.Message);

            ProblemDetails problemDetails = BuildProblemDetails(httpContext, 500);
            problemDetails.Detail = exception.Message;
            await WriteErrorToResponseAsync(httpContext, problemDetails);
        }
    }

    private static ProblemDetails BuildProblemDetails(HttpContext httpContext, int statusCode)
    {
        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        return problemDetailsFactory.CreateProblemDetails(httpContext, statusCode);
    }

    private static async Task WriteErrorToResponseAsync(HttpContext httpContext, ProblemDetails problemDetails)
    {
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
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