using System;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Defines a meta-schema for the 
/// </summary>
public static class MetaSchemas
{
	/// <summary>
	/// The data vocabulary meta-schema ID.
	/// </summary>
	public static readonly Uri ArrayExtId = new("https://json-everything.net/meta/vocab/array-ext");
	/// <summary>
	/// The ID for the draft 2020-12 extension vocabulary which includes the array extensions vocabulary.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Uri ArrayExt_202012Id = new("https://json-everything.net/meta/array-ext");

	/// <summary>
	/// The data vocabulary meta-schema.
	/// </summary>
	public static readonly JsonSchema ArrayExt =
		new JsonSchemaBuilder()
			.Schema(Schema.MetaSchemas.Draft202012Id)
			.Id(ArrayExtId)
			.Title("Array extensions meta-schema")
			.Properties(
				(UniqueKeysKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.JsonPointer))
				)
			);

	/// <summary>
	/// The data vocabulary meta-schema.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly JsonSchema ArrayExt_202012 =
		new JsonSchemaBuilder()
			.Schema(Schema.MetaSchemas.Draft202012Id)
			.Id(ArrayExt_202012Id)
			.Vocabulary(
				(Schema.Vocabularies.Core202012Id, true),
				(Schema.Vocabularies.Applicator202012Id, true),
				(Schema.Vocabularies.Validation202012Id, true),
				(Schema.Vocabularies.Metadata202012Id, true),
				(Schema.Vocabularies.FormatAnnotation202012Id, true),
				(Schema.Vocabularies.Content202012Id, true),
				(Schema.Vocabularies.Unevaluated202012Id, true),
				(Vocabularies.ArrayExtId, true)
			)
			.DynamicAnchor("meta")
			.Title("Array extensions 2020-12 meta-schema")
			.AllOf(
				new JsonSchemaBuilder().Ref(Schema.MetaSchemas.Draft202012Id),
				new JsonSchemaBuilder().Ref(ArrayExtId)
			);
}