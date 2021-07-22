using System;

namespace Json.Schema.UniqueKeys
{
	/// <summary>
	/// Defines a meta-schema for the 
	/// </summary>
	public static class MetaSchemas
	{
		/// <summary>
		/// The data vocabulary meta-schema ID.
		/// </summary>
		public static readonly Uri UniqueKeysId = new Uri("https://gregsdennis.github.io/json-everything/meta/unique-keys");

		/// <summary>
		/// The data vocabulary meta-schema.
		/// </summary>
		public static readonly JsonSchema UniqueKeys =
			new JsonSchemaBuilder()
				.Id(UniqueKeysId)
				.Schema(Schema.MetaSchemas.Draft202012Id)
				.Vocabulary(
					(Schema.Vocabularies.Core202012Id, true),
					(Schema.Vocabularies.Applicator202012Id, true),
					(Schema.Vocabularies.Validation202012Id, true),
					(Schema.Vocabularies.Metadata202012Id, true),
					(Schema.Vocabularies.FormatAnnotation202012Id, true),
					(Vocabularies.UniqueKeysId, true)
				)
				.DynamicAnchor("meta")
				.Title("Unique keys meta-schema")
				.AllOf(new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/schema"))
				.Properties(
					(UniqueKeysKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Formats.JsonPointer))
					)
				);
	}
}