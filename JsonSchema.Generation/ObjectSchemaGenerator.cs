using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema.Generation
{
	internal class ObjectSchemaGenerator : ISchemaGenerator
	{

		public bool Handles(Type type)
		{
			return true;
		}

		public void AddConstraints(JsonSchemaBuilder builder, SchemaGeneratorContext context)
		{
			builder.Type(SchemaValueType.Object);

			var props = new Dictionary<string, JsonSchema>();
			var required = new List<string>();
			var propertiesToGenerate = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && p.CanWrite);
			foreach (var property in propertiesToGenerate)
			{
				var propAttributes = property.GetCustomAttributes().ToList();
				var propContext = new SchemaGeneratorContext(property.PropertyType, propAttributes);
				var propBuilder = new JsonSchemaBuilder().FromType(propContext);

				if (propAttributes.OfType<ObsoleteAttribute>().Any())
					propBuilder.Deprecated(true);

				if (propAttributes.OfType<RequiredAttribute>().Any())
					required.Add(property.Name);

				props.Add(property.Name, propBuilder);
			}

			builder.Properties(props);
			if (required.Any())
				builder.Required(required);
		}
	}
}