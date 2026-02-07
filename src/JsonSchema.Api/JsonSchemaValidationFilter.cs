using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Json.Schema.Api;

public class JsonSchemaValidationFilter : IActionFilter, IAlwaysRunResultFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // this method is required for partial binding success
        var check = HandleJsonSchemaErrors(context);
        if (check is not null) 
	        context.Result = check;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // no-op
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        // this method is required for total binding failure
        var check = HandleJsonSchemaErrors(context);
        if (check is not null) 
	        context.Result = check;
    }

    private static IActionResult? HandleJsonSchemaErrors(FilterContext context)
    {
        if (context.ModelState.IsValid)
        {
            return null;
        }

        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Any() == true)
            .SelectMany(x => x.Value!.Errors.Select(e => new
            {
                Path = x.Key,
                Message = e.ErrorMessage,
            }))
            .Where(e => string.IsNullOrEmpty(e.Path) || e.Path.StartsWith('/')) // empty JSON Pointer is empty string
            .GroupBy(x => x.Path)
            .ToDictionary(x => x.Key, x => x.Select(e => e.Message).ToList());

        if (errors.Count == 0)
        {
            // If we don't have JSON Pointer errors, JSON Schema didn't handle this.
            // Don't change anything.
            return null;
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://json-everything.net/errors/validation",
            Title = "Validation Error",
            Status = 400,
            Detail = "One or more validation errors occurred.",
            Extensions =
            {
                ["errors"] = errors
            }
        };

        return new BadRequestObjectResult(problemDetails);

    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // no-op
    }
} 