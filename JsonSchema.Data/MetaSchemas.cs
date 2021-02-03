using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Schema.Data
{
	/// <summary>
	/// Defines a meta-schema for the 
	/// </summary>
	public static class MetaSchemas
	{
		/// <summary>
		/// The data vocabulary meta-schema ID.
		/// </summary>
		public static readonly Uri DataId = new Uri("https://gregsdennis.github.io/json-everything/meta/data");

		/// <summary>
		/// The data vocabulary meta-schema.
		/// </summary>
		public static readonly JsonSchema Data =
			new JsonSchemaBuilder()
				.Id(DataId)
				.Schema(Schema.MetaSchemas.Draft202012Id)
				.Vocabulary(
					(Schema.Vocabularies.Core202012Id, true),
					(Schema.Vocabularies.Applicator202012Id, true),
					(Schema.Vocabularies.Validation202012Id, true),
					(Schema.Vocabularies.Metadata202012Id, true),
					(Schema.Vocabularies.FormatAnnotation202012Id, true),
					(Vocabularies.DataId, true)
				)
				.DynamicAnchor("meta")
				.Title("Referenced data meta-schema")
				.AllOf(new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/schema"))
				.Properties(
					(DataKeyword.Name, new JsonSchemaBuilder()
						.AdditionalProperties(new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Formats.UriReference))
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					)
				);
	}
}