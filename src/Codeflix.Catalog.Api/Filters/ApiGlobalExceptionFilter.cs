using Codeflix.Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Codeflix.Catalog.Api.Filters;

public class ApiGlobalExceptionFilter : IExceptionFilter
{
    private readonly IHostEnvironment _environment;

    public ApiGlobalExceptionFilter(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
        var details = new ProblemDetails();
        var exception = context.Exception;

        if (_environment.IsDevelopment())
            details.Extensions.Add("StackTrace", exception.StackTrace);

        if (exception is EntityValidationException)
        {
            var ex = exception as EntityValidationException;
            details.Title = "One or more validation errors occurred";
            details.Status = StatusCodes.Status422UnprocessableEntity;
            details.Type = "UnprocessableEntity";
            details.Detail = ex!.Message;
        }
        else
        {
            details.Title = "An unexpected error occurred";
            details.Status = StatusCodes.Status500InternalServerError;
            details.Type = "UnexpectedError";
            details.Detail = exception.Message;
        }

        context.HttpContext.Response.StatusCode = (int)details.Status;
        context.Result = new ObjectResult(details);
        context.ExceptionHandled = true;
    }
}