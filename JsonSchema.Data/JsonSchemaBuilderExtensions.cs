using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Data
{
	public static class JsonSchemaBuilderExtensions
	{
		public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, Uri> data)
		{
			builder.Add(new DataKeyword(data));
			return builder;
		}
		public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, string> data)
		{
			builder.Add(new DataKeyword(data.ToDictionary(x => x.Key, x => new Uri(x.Value, UriKind.RelativeOrAbsolute))));
			return builder;
		}
		public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, params (string name, Uri reference)[] data)
		{
			builder.Add(new DataKeyword(data.ToDictionary(x => x.name, x => x.reference)));
			return builder;
		}
		public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, params (string name, string reference)[] data)
		{
			builder.Add(new DataKeyword(data.ToDictionary(x => x.name, x => new Uri(x.reference, UriKind.RelativeOrAbsolute))));
			return builder;
		}
	}
}