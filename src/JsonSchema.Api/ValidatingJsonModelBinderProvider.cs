using System;
using System.Linq;
using Json.Schema.Generation.Serialization;
using Json.Schema.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Json.Schema.Api;

/// <summary>
/// Provides an ASP.NET Core model binder provider that supplies a model binder for types annotated with JSON schema
/// attributes, enabling automatic validation of JSON request bodies against the model's schema.
/// </summary>
/// <remarks>This provider only applies to model types decorated with the <see
/// cref="GenerateJsonSchemaAttribute"/> or <see cref="JsonSchemaAttribute"/> attributes and when the binding source is
/// either unspecified or explicitly set to the request body. It is typically used to ensure that incoming JSON payloads
/// conform to the expected schema before model binding completes. To use this provider, register it in the
/// application's model binder provider collection.</remarks>
public class ValidatingJsonModelBinderProvider : IModelBinderProvider
{
	/// <summary>
	/// Creates a <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder" /> based on <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext" />.
	/// </summary>
	/// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext" />.</param>
	/// <returns>An <see cref="T:Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder" />.</returns>
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		// Only use this binder for types that have the GenerateJsonSchema attribute
		// and only if the binding source is undefined or explicitly the body
		if ((context.Metadata.BindingSource == null || context.Metadata.BindingSource == BindingSource.Body) &&
		    (context.Metadata.ModelType.GetCustomAttributes(typeof(GenerateJsonSchemaAttribute), true).Any() ||
		     context.Metadata.ModelType.GetCustomAttributes(typeof(JsonSchemaAttribute), true).Any()))
			return new ValidatingJsonModelBinder();

		return null;
	}
}