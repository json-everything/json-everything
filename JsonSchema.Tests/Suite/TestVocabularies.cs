using System;

namespace Json.Schema.Tests.Suite
{
	internal static class TestVocabularies
	{
		public static Uri MetaSchemaIdWithoutValidation_201909 =
			new Uri("http://localhost:1234/draft2019-09/metaschema-no-validation.json");

		public static JsonSchema MetaschemaWithoutValidation_201909 =
			new JsonSchemaBuilder()
				.Id(MetaSchemaIdWithoutValidation_201909)
				.Vocabulary(
					("https://json-schema.org/draft/2019-09/vocab/applicator", true),
					("https://json-schema.org/draft/2019-09/vocab/core", true)
				)
				.AllOf(
					new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2019-09/meta/applicator"),
					new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2019-09/meta/ccre")
				);

		public static Vocabulary VocabWithoutValidation_201909 =
			new Vocabulary("http://localhost:1234/draft2019-09/metaschema-no-validation.json",
				MetaschemaWithoutValidation_201909);

		public static Uri MetaSchemaIdWithoutValidation_202012 =
			new Uri("http://localhost:1234/draft2020-12/metaschema-no-validation.json");

		public static JsonSchema MetaschemaWithoutValidation_202012 =
			new JsonSchemaBuilder()
				.Id(MetaSchemaIdWithoutValidation_202012)
				.Vocabulary(
					("https://json-schema.org/draft/2020-12/vocab/applicator", true),
					("https://json-schema.org/draft/2020-12/vocab/core", true)
				)
				.AllOf(
					new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/meta/applicator"),
					new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/meta/ccre")
				);

		public static Vocabulary VocabWithoutValidation_202012 =
			new Vocabulary("http://localhost:1234/draft/2020-12/metaschema-no-validation.json",
				MetaschemaWithoutValidation_202012);

		public static Uri MetaSchemaIdFormatAssertionFalse_202012 =
			new Uri("http://localhost:1234/draft2020-12/format-assertion-false.json");

		public static JsonSchema MetaschemaFormatAssertionFalse_202012 =
			new JsonSchemaBuilder()
				.Id(MetaSchemaIdFormatAssertionFalse_202012)
				.Vocabulary(
					("https://json-schema.org/draft/2020-12/vocab/core", true),
					("https://json-schema.org/draft/2020-12/vocab/format-assertion", false)
				)
				.AllOf(
					new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/meta/applicator"),
					new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/meta/format-assertion")
				);

		public static Vocabulary VocabFormatAssertionFalse_202012 =
			new Vocabulary("http://localhost:1234/draft/2020-12/format-assertion-false.json",
				MetaschemaFormatAssertionFalse_202012);

		public static Uri MetaSchemaIdFormatAssertionTrue_202012 =
			new Uri("http://localhost:1234/draft2020-12/format-assertion-true.json");

		public static JsonSchema MetaschemaFormatAssertionTrue_202012 =
			new JsonSchemaBuilder()
				.Id(MetaSchemaIdFormatAssertionTrue_202012)
				.Vocabulary(
					("https://json-schema.org/draft/2020-12/vocab/core", true),
					("https://json-schema.org/draft/2020-12/vocab/format-assertion", true)
				)
				.AllOf(
					new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/meta/applicator"),
					new JsonSchemaBuilder().Ref("https://json-schema.org/draft/2020-12/meta/format-assertion")
				);

		public static Vocabulary VocabFormatAssertionTrue_202012 =
			new Vocabulary("http://localhost:1234/draft/2020-12/format-assertion-true.json",
				MetaschemaFormatAssertionTrue_202012);

		public static void Register()
		{
			VocabularyRegistry.Global.Register(VocabWithoutValidation_201909);
			SchemaRegistry.Global.Register(MetaSchemaIdWithoutValidation_201909, MetaschemaWithoutValidation_201909);
			VocabularyRegistry.Global.Register(VocabWithoutValidation_202012);
			SchemaRegistry.Global.Register(MetaSchemaIdWithoutValidation_202012, MetaschemaWithoutValidation_202012);
			VocabularyRegistry.Global.Register(VocabFormatAssertionFalse_202012);
			SchemaRegistry.Global.Register(MetaSchemaIdFormatAssertionFalse_202012, MetaschemaFormatAssertionFalse_202012);
			VocabularyRegistry.Global.Register(VocabFormatAssertionTrue_202012);
			SchemaRegistry.Global.Register(MetaSchemaIdFormatAssertionTrue_202012, MetaschemaFormatAssertionTrue_202012);

		}
	}
}
