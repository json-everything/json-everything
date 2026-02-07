using System;
using System.Linq;
using Json.Schema.Generation.Serialization;
using Json.Schema.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Json.Schema.Api;

public class ValidatingJsonModelBinderProvider : IModelBinderProvider
{
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