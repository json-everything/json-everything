using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Json.Schema.Api;

/// <summary>
/// An ASP.NET Core filter that validates incoming JSON request bodies against a JSON Schema and returns detailed
/// validation errors in the response if validation fails.
/// </summary>
/// <remarks>This filter inspects the model state for JSON Schema validation errors during both action and result
/// execution. If validation errors are present and correspond to JSON Pointer paths, the filter short-circuits the
/// request pipeline and returns a 400 Bad Request response with a standardized problem details payload. The filter is
/// intended to be used in scenarios where JSON Schema validation is required for API endpoints that accept JSON input. 
/// The filter implements both IActionFilter and IAlwaysRunResultFilter to ensure that validation errors are handled
/// regardless of whether model binding partially or totally fails. It does not modify the response if no relevant
/// validation errors are found.</remarks>
public class JsonSchemaValidationFilter : IActionFilter, IAlwaysRunResultFilter
{
	/// <summary>
	/// Called before the action executes, after model binding is complete.
	/// </summary>
	/// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext" />.</param>
	public void OnActionExecuting(ActionExecutingContext context)
    {
        // this method is required for partial binding success
        var check = HandleJsonSchemaErrors(context);
        if (check is not null) 
	        context.Result = check;
    }

	/// <summary>
	/// Called after the action executes, before the action result.
	/// </summary>
	/// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext" />.</param>
	public void OnActionExecuted(ActionExecutedContext context)
    {
        // no-op
    }

	/// <summary>Called before the action result executes.</summary>
	/// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ResultExecutingContext" />.</param>
	public void OnResultExecuting(ResultExecutingContext context)
    {
        // this method is required for total binding failure
        var check = HandleJsonSchemaErrors(context);
        if (check is not null) 
	        context.Result = check;
    }

    private static BadRequestObjectResult? HandleJsonSchemaErrors(ActionContext context)
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

    /// <summary>Called after the action result executes.</summary>
    /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ResultExecutedContext" />.</param>
    public void OnResultExecuted(ResultExecutedContext context)
    {
        // no-op
    }
} 