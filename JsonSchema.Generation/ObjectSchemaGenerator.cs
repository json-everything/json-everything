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

		public void AddConstraints(JsonSchemaBuilder builder, Type type, List<Attribute> attributes)
		{
			builder.Type(SchemaValueType.Object);

			var props = new Dictionary<string, JsonSchema>();
			var required = new List<string>();
			var propertiesToGenerate = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && p.CanWrite);
			foreach (var property in propertiesToGenerate)
			{
				var propAttributes = property.GetCustomAttributes().ToList();
				var propBuilder = new JsonSchemaBuilder().FromType(property.PropertyType, propAttributes);

				if (property.GetCustomAttribute<RequiredAttribute>() != null)
					required.Add(property.Name);

				props.Add(property.Name, propBuilder.Build());
			}

			builder.Properties(props);
			builder.HandleAttributes(attributes, type);
			if (required.Any())
				builder.Required(required);
		}
	}
}