using System;

namespace Json.Schema.Generation.Generators;

internal class NullableValueTypeSchemaGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type.IsNullableValueType();
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		var underlyingType = Nullable.GetUnderlyingType(context.Type);

		if (underlyingType == null) return;
		var underlyingContext = SchemaGenerationContextCache.Get(underlyingType);
		underlyingContext.ReferenceCount--;

		context.Intents.AddRange(underlyingContext.Intents);
	}
}