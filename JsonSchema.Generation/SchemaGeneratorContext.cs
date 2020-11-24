using System;
using System.Collections.Generic;

namespace Json.Schema.Generation
{
	public class SchemaGeneratorContext
	{
		public Type Type { get; }
		public List<Attribute> Attributes { get; }
		public List<ISchemaKeywordIntent> Intents { get; } = new List<ISchemaKeywordIntent>();

		public SchemaGeneratorContext(Type type, List<Attribute> attributes)
		{
			Type = type;
			Attributes = attributes;
		}

		public JsonSchemaBuilder Apply(JsonSchemaBuilder builder = null)
		{
			builder ??= new JsonSchemaBuilder();

			foreach (var intent in Intents)
			{
				intent.Apply(builder);
			}

			return builder;
		}

		public void GenerateIntents()
		{
			var generator = GeneratorRegistry.Get(Type);
			generator?.AddConstraints(this);

			AttributeHandler.HandleAttributes( this);
		}
	}
}