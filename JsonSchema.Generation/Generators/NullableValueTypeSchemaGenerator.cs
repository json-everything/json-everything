using System;

namespace Json.Schema.Generation.Generators
{
	internal class NullableValueTypeSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			var underlyingType = Nullable.GetUnderlyingType(context.Type);

			if (underlyingType == null) return;
			var underlyingContext = SchemaGenerationContextCache.Get(underlyingType, context.Attributes, context.Configuration);

			context.Intents.AddRange(underlyingContext.Intents);
		}
	}
}