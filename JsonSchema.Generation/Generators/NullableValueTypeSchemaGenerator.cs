using System;

namespace Json.Schema.Generation.Generators
{
	internal class NullableValueTypeSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			var underlyingType = Nullable.GetUnderlyingType(type);

			if (underlyingType != null)
			{
				return GeneratorRegistry.Get(underlyingType) != null;
			}

			return false;
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			var underlyingType = Nullable.GetUnderlyingType(context.Type);

			if (underlyingType != null)
			{
				var underlyingContext = new SchemaGeneratorContext(underlyingType, context.Attributes, context.Configuration);

				GeneratorRegistry.Get(Nullable.GetUnderlyingType(context.Type))?
					.AddConstraints(underlyingContext);

				context.Intents.AddRange(underlyingContext.Intents);
			}
		}
	}
}