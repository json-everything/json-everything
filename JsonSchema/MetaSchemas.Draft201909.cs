using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Schema
{
	public static partial class MetaSchemas
	{
		public static readonly Uri Draft201909Id = new Uri("https://json-schema.org/draft/2019-09/schema");
		
		public static readonly Uri Core201909Id = new Uri("https://json-schema.org/draft/2019-09/meta/core");
		public static readonly Uri Applicator201909Id = new Uri("https://json-schema.org/draft/2019-09/meta/applicator");
		public static readonly Uri Validation201909Id = new Uri("https://json-schema.org/draft/2019-09/meta/validation");
		public static readonly Uri Metadata201909Id = new Uri("https://json-schema.org/draft/2019-09/meta/meta-data");
		public static readonly Uri Format201909Id = new Uri("https://json-schema.org/draft/2019-09/meta/format");
		public static readonly Uri Content201909Id = new Uri("https://json-schema.org/draft/2019-09/meta/content");

		public static readonly JsonSchema Draft201909 =
			new JsonSchemaBuilder()
				.Schema(Draft201909Id)
				.Id(Draft201909Id)
				.Vocabulary(
					(VocabularyRegistry.Core201909Id, true),
					(VocabularyRegistry.Applicator201909Id, true),
					(VocabularyRegistry.Validation201909Id, true),
					(VocabularyRegistry.Metadata201909Id, true),
					(VocabularyRegistry.Format201909Id, false),
					(VocabularyRegistry.Content201909Id, true)
				)
				.RecursiveAnchor(true)
				.Title("Core and Validation specifications meta-schema")
				.AllOf(
					new JsonSchemaBuilder().Ref("meta/core"),
					new JsonSchemaBuilder().Ref("meta/applicator"),
					new JsonSchemaBuilder().Ref("meta/validation"),
					new JsonSchemaBuilder().Ref("meta/meta-data"),
					new JsonSchemaBuilder().Ref("meta/format"),
					new JsonSchemaBuilder().Ref("meta/content")
				)
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(DefinitionsKeyword.Name, new JsonSchemaBuilder()
						.Comment("While no longer an official keyword as it is replaced by $defs, this keyword is retained in the meta-schema to prevent incompatible extensions as it remains in common use.")
						.Type(SchemaValueType.Object)
						.AdditionalProperties(JsonSchemaBuilder.RecursiveRefRoot())
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					),
					(DependenciesKeyword.Name, new JsonSchemaBuilder()
						.Comment("\"dependencies\" is no longer a keyword, but schema authors should avoid redefining it to facilitate a smooth transition to \"dependentSchemas\" and \"dependentRequired\"")
						.Type(SchemaValueType.Object)
						.AdditionalProperties(new JsonSchemaBuilder()
							.AnyOf(
								JsonSchemaBuilder.RecursiveRefRoot(),
								new JsonSchemaBuilder().Ref("meta/validation#/$defs/stringArray")
							)
						)
					)
				);

		public static readonly JsonSchema Core201909 =
			new JsonSchemaBuilder()
				.Schema(Draft201909Id)
				.Id(Core201909Id)
				.Vocabulary((VocabularyRegistry.Core201909Id, true))
				.RecursiveAnchor(true)
				.Title("Core vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(IdKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.UriReference)
						.Comment("Non-empty fragments not allowed.")
						.Pattern("^[^#]*#?$")
					),
					(SchemaKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.Uri)
					),
					(AnchorKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Pattern(AnchorKeyword.AnchorPattern)
					),
					(RefKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.UriReference)
					),
					(RecursiveRefKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.UriReference)
					),
					(RecursiveAnchorKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Boolean)
						.Default(false.AsJsonElement())
					),
					(VocabularyKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.PropertyNames(new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Formats.Uri)
						)
						.AdditionalProperties(new JsonSchemaBuilder()
							.Type(SchemaValueType.Boolean)
						)
					),
					(CommentKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					(DefsKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(JsonSchemaBuilder.RecursiveRefRoot())
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					)
				);

		public static readonly JsonSchema Applicator201909 =
			new JsonSchemaBuilder()
				.Schema(Draft201909Id)
				.Id(Applicator201909Id)
				.Vocabulary((VocabularyRegistry.Applicator201909Id, true))
				.RecursiveAnchor(true)
				.Title("Applicator vocabulary meta-schema")
				.Properties(
					(AdditionalItemsKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(UnevaluatedItemsKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(ItemsKeyword.Name, new JsonSchemaBuilder()
						.AnyOf(
							JsonSchemaBuilder.RefRoot(),
							new JsonSchemaBuilder().Ref("#/defs/schemaArray")
						)
					),
					(ContainsKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(AdditionalPropertiesKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(UnevaluatedPropertiesKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(PropertiesKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(JsonSchemaBuilder.RecursiveRefRoot())
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					),
					(PatternPropertiesKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(JsonSchemaBuilder.RecursiveRefRoot())
						.PropertyNames(new JsonSchemaBuilder()
							.Format(Formats.Regex)
						)
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					),
					(DependentSchemasKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(JsonSchemaBuilder.RecursiveRefRoot())
					),
					(PropertyNamesKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(IfKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(ThenKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(ElseKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot()),
					(AllOfKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/schemaArray")
					),
					(AnyOfKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/schemaArray")
					),
					(OneOfKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/schemaArray")
					),
					(NotKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot())
				)
				.Defs(
					("schemaArray", new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.MinItems(1)
						.Items(JsonSchemaBuilder.RecursiveRefRoot())
					)
				);

		public static readonly JsonSchema Validation201909 =
			new JsonSchemaBuilder()
				.Schema(Draft201909Id)
				.Id(Validation201909Id)
				.Vocabulary((VocabularyRegistry.Validation201909Id, true))
				.RecursiveAnchor(true)
				.Title("Validation vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(MultipleOfKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)
						.ExclusiveMinimum(0)
					),
					(MaximumKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)
					),
					(ExclusiveMaximumKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)
					),
					(MinimumKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)
					),
					(ExclusiveMinimumKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)
					),
					(MaxLengthKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
					),
					(MinLengthKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeIntegerDefault0")
					),
					(PatternKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.Regex)
					),
					(MaxItemsKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
					),
					(MinItemsKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeIntegerDefault0")
					),
					(UniqueItemsKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Boolean)
						.Default(false.AsJsonElement())
					),
					(MaxContainsKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
					),
					(MinContainsKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
						.Default(1.AsJsonElement())
					),
					(MaxPropertiesKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
					),
					(MinPropertiesKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeIntegerDefault0")
					),
					(RequiredKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/stringArray")
					),
					(DependentRequiredKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(new JsonSchemaBuilder()
							.Ref("#/$defs/stringArray")
						)
					),
					(ConstKeyword.Name, true),
					(EnumKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(true)
					),
					(TypeKeyword.Name, new JsonSchemaBuilder()
						.AnyOf(
							new JsonSchemaBuilder().Ref("#/$defs/simpleTypes"),
							new JsonSchemaBuilder()
								.Type(SchemaValueType.Array)
								.Items(new JsonSchemaBuilder().Ref("#/$defs/simpleTypes"))
								.MinItems(1)
								.UniqueItems(true)
						)
					)
				)
				.Defs(
					("nonNegativeInteger", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Minimum(0)
					),
					("nonNegativeIntegerDefault0", new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
						.Default(0.AsJsonElement())
					),
					("simpleTypes", new JsonSchemaBuilder()
						.Enum(
							"array".AsJsonElement(),
							"boolean".AsJsonElement(),
							"integer".AsJsonElement(),
							"null".AsJsonElement(),
							"number".AsJsonElement(),
							"object".AsJsonElement(),
							"string".AsJsonElement()
						)
					),
					("stringArray", new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
						.UniqueItems(true)
						.Default(new JsonElement[0].AsJsonElement())
					)
				);

		public static readonly JsonSchema Metadata201909 =
			new JsonSchemaBuilder()
				.Schema(Draft201909Id)
				.Id(Metadata201909Id)
				.Vocabulary((VocabularyRegistry.Metadata201909Id, true))
				.RecursiveAnchor(true)
				.Title("Meta-data vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(TitleKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					(DescriptionKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					(DefaultKeyword.Name, true),
					(DeprecatedKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Boolean)
						.Default(false.AsJsonElement())
					),
					(ReadOnlyKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Boolean)
						.Default(false.AsJsonElement())
					),
					(WriteOnlyKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Boolean)
						.Default(false.AsJsonElement())
					),
					(ExamplesKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
					)
				);

		public static readonly JsonSchema Format201909 =
			new JsonSchemaBuilder()
				.Schema(Draft201909Id)
				.Id(Format201909Id)
				.Vocabulary((VocabularyRegistry.Format201909Id, true))
				.RecursiveAnchor(true)
				.Title("Format vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(FormatKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					)
				);

		public static readonly JsonSchema Content201909 =
			new JsonSchemaBuilder()
				.Schema(Draft201909Id)
				.Id(Content201909Id)
				.Vocabulary((VocabularyRegistry.Content201909Id, true))
				.RecursiveAnchor(true)
				.Title("Content vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(ContentMediaTypeKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					(ContentMediaEncodingKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					(ContentSchemaKeyword.Name, JsonSchemaBuilder.RecursiveRefRoot())
				);
	}
}