using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#pragma warning disable IL2026
#pragma warning disable IL3050

namespace Json.Schema.Api;

/// <summary>
/// Provides a model binder that deserializes JSON request data and performs validation, adding any validation errors to
/// the model state.
/// </summary>
/// <remarks>The ValidatingJsonModelBinder is designed for use in ASP.NET Core applications to bind and validate
/// models from JSON request bodies or value providers. When binding from the request body, it uses the configured
/// JsonSerializerOptions and supports validation that can add detailed errors to the model state. If validation fails,
/// the model binding result is set to failed, and errors are available in ModelState for use by validation filters or
/// error handlers. This binder is typically used to enable custom or advanced validation scenarios during model
/// binding.</remarks>
public class ValidatingJsonModelBinder : IModelBinder
{
	/// <summary>Attempts to bind a model.</summary>
	/// <param name="bindingContext">The <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext" />.</param>
	/// <returns>
	/// <para>
	/// A <see cref="T:System.Threading.Tasks.Task" /> which will complete when the model binding process completes.
	/// </para>
	/// <para>
	/// If model binding was successful, the <see cref="P:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext.Result" /> should have
	/// <see cref="P:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult.IsModelSet" /> set to <c>true</c>.
	/// </para>
	/// <para>
	/// A model binder that completes successfully should set <see cref="P:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext.Result" /> to
	/// a value returned from <see cref="M:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult.Success(System.Object)" />.
	/// </para>
	/// </returns>
	public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
		ArgumentNullException.ThrowIfNull(bindingContext);

        // For body binding, we need to read the request body
        if (bindingContext.BindingSource == BindingSource.Body)
        {
            bindingContext.HttpContext.Request.EnableBuffering();
            using var reader = new StreamReader(bindingContext.HttpContext.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            bindingContext.HttpContext.Request.Body.Position = 0;

            if (string.IsNullOrEmpty(body)) return;

            try
            {
                var options = bindingContext.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
                var model = JsonSerializer.Deserialize(body, bindingContext.ModelType, options);
                bindingContext.Result = ModelBindingResult.Success(model);
            }
            catch (JsonException jsonException)
            {
                if (jsonException.Data.Contains("validation") && 
                    jsonException.Data["validation"] is EvaluationResults { IsValid: false } validationResults)
                {
                    var errors = ExtractValidationErrors(validationResults);
                    if (errors.Count != 0)
                    {
                        foreach (var error in errors)
                        {
                            bindingContext.ModelState.AddModelError(error.Path, error.Message);
                        }
                    }
                    else
                    {
                        // Not all keywords produce an error.
                        // Still, in order for the model binder to actually fail, it needs an error message.
                        bindingContext.ModelState.AddModelError("", "A validation error occurred");
                    }

                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }

                bindingContext.ModelState.AddModelError(bindingContext.FieldName, jsonException, bindingContext.ModelMetadata);
                bindingContext.Result = ModelBindingResult.Failed();
            }
            return;
        }

        // For other binding sources, use the value provider
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None) return;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        try
        {
            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(value)) return;

            var options = bindingContext.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
            var model = JsonSerializer.Deserialize(value, bindingContext.ModelType, options);
            bindingContext.Result = ModelBindingResult.Success(model);
        }
        catch (JsonException jsonException)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, jsonException, bindingContext.ModelMetadata);
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }

    private static List<(string Path, string Message)> ExtractValidationErrors(EvaluationResults validationResults)
    {
        var errors = new List<(string Path, string Message)>();
        ExtractValidationErrorsRecursive(validationResults, errors);
        return errors;
    }

    private static void ExtractValidationErrorsRecursive(EvaluationResults results, List<(string Path, string Message)> errors)
    {
        if (results.IsValid) return;

        if (results.Errors != null)
        {
            foreach (var error in results.Errors)
            {
                errors.Add((results.InstanceLocation.ToString(), error.Value));
            }
        }

        if (results.Details != null)
        {
            foreach (var detail in results.Details)
            {
                ExtractValidationErrorsRecursive(detail, errors);
            }
        }
    }
}