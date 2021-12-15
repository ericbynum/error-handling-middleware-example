using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ErrorHandlingMiddlewareExample.Exceptions;

public class BadRequestException : Exception
{
    private const string message = "Bad request. See list of errors for details.";

    public BadRequestException(ModelStateDictionary modelState) : base(message)
    {
        Errors = modelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .ToArray();
    }

    public BadRequestException(params string[] errors) : base(message)
    {
        Errors = errors;
    }
    
    public string[] Errors { get; }
}