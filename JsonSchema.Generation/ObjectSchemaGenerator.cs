using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation
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
				var propContext = SchemaGenerationContextCache.Get(property.PropertyType, propAttributes);

				props.Add(property.Name, propContext);

				if (propAttributes.OfType<ObsoleteAttribute>().Any())
					propContext.Intents.Add(new DeprecatedIntent(true));

				if (propAttributes.OfType<RequiredAttribute>().Any())
					required.Add(property.Name);
			}

			context.Intents.Add(new PropertiesIntent(props));
			if (required.Any())
				context.Intents.Add(new RequiredIntent(required));
		}
	}
}