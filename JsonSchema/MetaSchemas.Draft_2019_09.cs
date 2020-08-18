using System;

namespace Json.Schema
{
	public static partial class MetaSchemas
	{
		public static readonly Uri Draft_2019_09_Id = new Uri("https://json-schema.org/draft/2019-09/schema");

		public static readonly JsonSchema Draft_2019_09 =
			new JsonSchemaBuilder()
				.Build();
	}
}