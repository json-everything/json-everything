using System;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators
{
	internal class UriSchemaGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type == typeof(Uri);
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			context.Intents.Add(new TypeIntent(SchemaValueType.String));
			context.Intents.Add(new FormatIntent(Formats.Uri));
		}
	}
}