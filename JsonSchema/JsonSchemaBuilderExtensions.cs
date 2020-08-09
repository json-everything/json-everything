using System;

namespace Json.Schema
{
	public static class JsonSchemaBuilderExtensions
	{
		public static JsonSchemaBuilder Id(this JsonSchemaBuilder builder, Uri id)
		{
			builder.Add(new IdKeyword(id));
			return builder;
		}

		public static JsonSchemaBuilder Minimum(this JsonSchemaBuilder builder, decimal value)
		{
			builder.Add(new MinimumKeyword(value));
			return builder;
		}
	}
}