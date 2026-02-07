using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Json.Schema.Api;

public class ValidatingJsonModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

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
                    if (errors.Any())
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