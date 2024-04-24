using System.Text.Json.Nodes;
using Json.Schema;
using Json.Schema.Data;

namespace JsonEverythingNet.Services;

public static class Data2022Support
{
	public static class MetaSchemas
	{
		public static readonly Uri Data2022Id = new("https://json-everything.net/meta/data-2022");

		public static readonly JsonSchema Data2022 =
			new JsonSchemaBuilder()
				.Id(Data2022Id)
				.Schema(Json.Schema.MetaSchemas.Draft202012Id)
				.Vocabulary(
					(Json.Schema.Vocabularies.Core202012Id, true),
					(Json.Schema.Vocabularies.Applicator202012Id, true),
					(Json.Schema.Vocabularies.Validation202012Id, true),
					(Json.Schema.Vocabularies.Metadata202012Id, true),
					(Json.Schema.Vocabularies.FormatAnnotation202012Id, true),
					(Json.Schema.Vocabularies.Content202012Id, true),
					(Json.Schema.Vocabularies.Unevaluated202012Id, true),
					(Vocabularies.Data2022Id, true)
				)
				.DynamicAnchor("meta")
				.Title("Referenced data meta-schema")
				.AllOf(new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/schema"))
				.Properties(
					(DataKeyword.Name, new JsonSchemaBuilder()
						.AdditionalProperties(new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.OneOf(
								new JsonSchemaBuilder().Format(Formats.JsonPointer),
								new JsonSchemaBuilder().Format(Formats.RelativeJsonPointer),
								new JsonSchemaBuilder().Format(Formats.UriReference)
							)
						)
						.Default(new JsonObject())
					)
				);
	}

	public static class Vocabularies
	{
		public const string Data2022Id = "https://json-everything.net/vocabs-data-2022";

		public static readonly Vocabulary Data2022 = new(Data2022Id, typeof(DataKeyword));

		public static void Register(SchemaRegistry? schemaRegistry = null)
		{
			schemaRegistry ??= SchemaRegistry.Global;

			VocabularyRegistry.Register(Data2022);
			// don't need the data keyword registered because it's already registered by the 2023 version.
			schemaRegistry.Register(MetaSchemas.Data2022);
		}
	}
}