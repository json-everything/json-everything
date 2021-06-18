using Json.Schema.Generation.Intents;
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


				var nullable = (context.Configuration.Nullability & Nullability.AllowForNullableValueTypes)
					== Nullability.AllowForNullableValueTypes;

				foreach (var intent in underlyingContext.Intents)
				{
					if (nullable
						&& intent is TypeIntent tIntent
						&& (tIntent.Type & SchemaValueType.Null) == 0)
					{
						context.Intents.Add(new TypeIntent(tIntent.Type | SchemaValueType.Null));
						continue;
					}

					context.Intents.Add(intent);
				}
			}
		}
	}
}