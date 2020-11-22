using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Json.Schema.Generation
{
	internal class ObjectSchemaGenerator : ISchemaGenerator
	{
		private static readonly List<IAttributeHandler> _attributeHandlers =
			typeof(IAttributeHandler).Assembly.DefinedTypes
				.Where(t => typeof(IAttributeHandler).IsAssignableFrom(t) &&
				            !t.IsInterface && !t.IsAbstract)
				.Select(Activator.CreateInstance)
				.Cast<IAttributeHandler>()
				.ToList();

		public bool Handles(Type type)
		{
			return true;
		}

		public void AddConstraints(JsonSchemaBuilder builder, Type type)
		{
			builder.Type(SchemaValueType.Object);

			var props = new Dictionary<string, JsonSchema>();
			var required = new List<string>();
			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite))
			{
				var propBuilder = new JsonSchemaBuilder().FromType(property.PropertyType);

				if (property.GetCustomAttribute<RequiredAttribute>() != null)
					required.Add(property.Name);

				foreach (var handler in _attributeHandlers)
				{
					handler.AddConstraints(builder, propBuilder, property);
				}

				props.Add(property.Name, propBuilder.Build());
			}
			if (required.Any())
				builder.Required(required);

			builder.Properties(props);
		}
	}
}