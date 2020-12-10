using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators
{
	internal class ObjectSchemaGenerator : ISchemaGenerator
	{

		public bool Handles(Type type)
		{
			return true;
		}

		public void AddConstraints(SchemaGeneratorContext context)
		{
			context.Intents.Add(new TypeIntent(SchemaValueType.Object));

			var props = new Dictionary<string, SchemaGeneratorContext>();
			var required = new List<string>();
			var propertiesToGenerate = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && p.CanWrite);
			foreach (var property in propertiesToGenerate)
			{
				var propAttributes = property.GetCustomAttributes().ToList();
				var ignoreAttribute = context.Attributes.OfType<JsonIgnoreAttribute>().FirstOrDefault();
				if (ignoreAttribute != null) continue;

				var propContext = SchemaGenerationContextCache.Get(property.PropertyType, propAttributes);

				var name = property.Name;
				var nameAttribute = context.Attributes.OfType<JsonPropertyNameAttribute>().FirstOrDefault();
				if (nameAttribute != null)
					name = nameAttribute.Name;

				props.Add(name, propContext);

				if (propAttributes.OfType<RequiredAttribute>().Any())
					required.Add(property.Name);
			}

			context.Intents.Add(new PropertiesIntent(props));
			if (required.Any())
				context.Intents.Add(new RequiredIntent(required));
		}
	}
}