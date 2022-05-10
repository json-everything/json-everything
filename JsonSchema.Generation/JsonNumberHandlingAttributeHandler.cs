using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Handler for the <see cref="JsonNumberHandlingAttribute"/>.
/// </summary>
public class JsonNumberHandlingAttributeHandler : IAttributeHandler<JsonNumberHandlingAttribute>
{
	/// <summary>
	/// Processes the type and any attributes (present on the context), and adds
	/// intents to the context.
	/// </summary>
	/// <param name="context">The generation context.</param>
	/// <param name="attribute"></param>
	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var handlingAttribute = attribute as JsonNumberHandlingAttribute;
		if (handlingAttribute == null) return;

		if (!context.Type.IsNumber()) return;

		var typeIntent = context.Intents.OfType<TypeIntent>().FirstOrDefault();

		var existingType = typeIntent?.Type ?? default;

		if (handlingAttribute.Handling.HasFlag(JsonNumberHandling.AllowReadingFromString))
		{
			context.Intents.Remove(typeIntent!);
			context.Intents.Add(new TypeIntent(existingType | SchemaValueType.String));
		}

		if (handlingAttribute.Handling.HasFlag(JsonNumberHandling.AllowNamedFloatingPointLiterals))
		{
			var currentSchema = context.Intents.ToList();
			context.Intents.Clear();
			context.Intents.Add(new AnyOfIntent(currentSchema,
					new ISchemaKeywordIntent[] { new EnumIntent("NaN", "Infinity", "-Infinity") }
				)
			);
		}
	}
}