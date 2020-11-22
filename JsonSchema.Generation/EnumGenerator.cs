using System;
using System.Collections.Generic;
using System.Linq;
using Json.More;

namespace Json.Schema.Generation
{
	internal class EnumGenerator : ISchemaGenerator
	{
		public bool Handles(Type type)
		{
			return type.IsEnum;
		}

		public void AddConstraints(JsonSchemaBuilder builder, Type type, List<Attribute> attributes)
		{
			var values = Enum.GetNames(type);

			builder.Enum(values.Select(v => v.AsJsonElement()));
			builder.HandleAttributes(attributes, type);
		}
	}
}