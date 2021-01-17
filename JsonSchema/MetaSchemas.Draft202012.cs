using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Schema
{
	public static partial class MetaSchemas
	{
		internal const string Draft202012IdValue = "https://json-schema.org/draft/2020-12/schema";

		/// <summary>
		/// The Draft 2020-12 meta-schema ID.
		/// </summary>
		public static readonly Uri Draft202012Id = new Uri(Draft202012IdValue);
		
		/// <summary>
		/// The Draft 2020-12 Core meta-schema ID.
		/// </summary>
		public static readonly Uri Core202012Id = new Uri("https://json-schema.org/draft/2020-12/meta/core");
		/// <summary>
		/// The Draft 2020-12 Unevaluated meta-schema ID.
		/// </summary>
		public static readonly Uri Unevaluated202012Id = new Uri("https://json-schema.org/draft/2020-12/meta/unevaluated");
		/// <summary>
		/// The Draft 2020-12 Applicator meta-schema ID.
		/// </summary>
		public static readonly Uri Applicator202012Id = new Uri("https://json-schema.org/draft/2020-12/meta/applicator");
		/// <summary>
		/// The Draft 2020-12 Validation meta-schema ID.
		/// </summary>
		public static readonly Uri Validation202012Id = new Uri("https://json-schema.org/draft/2020-12/meta/validation");
		/// <summary>
		/// The Draft 2020-12 Metadata meta-schema ID.
		/// </summary>
		public static readonly Uri Metadata202012Id = new Uri("https://json-schema.org/draft/2020-12/meta/meta-data");
		/// <summary>
		/// The Draft 2020-12 Format-Annotation meta-schema ID.
		/// </summary>
		public static readonly Uri FormatAnnotation202012Id = new Uri("https://json-schema.org/draft/2020-12/meta/format-annotation");
		/// <summary>
		/// The Draft 2020-12 Format-Assertion meta-schema ID.
		/// </summary>
		public static readonly Uri FormatAssertion202012Id = new Uri("https://json-schema.org/draft/2020-12/meta/format-assertion");
		/// <summary>
		/// The Draft 2020-12 Content meta-schema ID.
		/// </summary>
		public static readonly Uri Content202012Id = new Uri("https://json-schema.org/draft/2020-12/meta/content");

		/// <summary>
		/// The Draft 2020-12 meta-schema.
		/// </summary>
		public static readonly JsonSchema Draft202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(Draft202012Id)
				.Vocabulary(
					(Vocabularies.Core202012Id, true),
					(Vocabularies.Applicator202012Id, true),
					(Vocabularies.Unevaluated202012Id, true),
					(Vocabularies.Validation202012Id, true),
					(Vocabularies.Metadata202012Id, true),
					(Vocabularies.FormatAnnotation202012Id, false),
					(Vocabularies.Content202012Id, true)
				)
				.DynamicAnchor("meta")
				.Title("Core and Validation specifications meta-schema")
				.AllOf(
					new JsonSchemaBuilder().Ref("meta/core"),
					new JsonSchemaBuilder().Ref("meta/applicator"),
					new JsonSchemaBuilder().Ref("meta/validation"),
					new JsonSchemaBuilder().Ref("meta/meta-data"),
					new JsonSchemaBuilder().Ref("meta/format-annotation"),
					new JsonSchemaBuilder().Ref("meta/content")
				)
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Comment("This meta-schema also defines keywords that have appeared in previous drafts in order to prevent incompatible extensions as they remain in common use.")
				.Properties(
					(DefinitionsKeyword.Name, new JsonSchemaBuilder()
						.Comment("\"definitions\" has been replaced by \"$defs\".")
						.Type(SchemaValueType.Object)
						.AdditionalProperties(new JsonSchemaBuilder().DynamicRef("#meta"))
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
						.Deprecated(true)
					),
					(DependenciesKeyword.Name, new JsonSchemaBuilder()
						.Comment("\"dependencies\" has been split and replaced by \"dependentSchemas\" and \"dependentRequired\" in order to serve their differing semantics.")
						.Type(SchemaValueType.Object)
						.AdditionalProperties(new JsonSchemaBuilder()
							.AnyOf(
								new JsonSchemaBuilder().DynamicRef("#meta"),
								new JsonSchemaBuilder().Ref("meta/validation#/$defs/stringArray")
							)
						)
						.Deprecated(true)
					),
					(RecursiveAnchorKeyword.Name, new JsonSchemaBuilder()
						.Comment("\"$recursiveAnchor\" has been replaced by \"$dynamicAnchor\".")
						.Ref("meta/core#/$defs/anchorString")
						.Deprecated(true)
					),
					(RecursiveRefKeyword.Name, new JsonSchemaBuilder()
						.Comment("\"$recursiveRef\" has been replaced by \"$dynamicRef\".")
						.Ref("meta/core#/$defs/uriReferenceString")
						.Deprecated(true)
					)
				);

		/// <summary>
		/// The Draft 2020-12 Core meta-schema.
		/// </summary>
		public static readonly JsonSchema Core202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(Core202012Id)
				.Vocabulary((Vocabularies.Core202012Id, true))
				.DynamicAnchor("meta")
				.Title("Core vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(IdKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/uriReferenceString")
						.Comment("Non-empty fragments not allowed.")
						.Pattern("^[^#]*#?$")
					),
					(SchemaKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/uriReferenceString")
					),
					(RefKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/uriReferenceString")
					),
					(AnchorKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/anchorString")
					),
					(DynamicRefKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/uriReferenceString")
					),
					(DynamicAnchorKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/anchorString")
					),
					(VocabularyKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.PropertyNames(new JsonSchemaBuilder()
							.Ref("#/$defs/uriString")
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
						.AdditionalProperties(new JsonSchemaBuilder().DynamicRef("#meta"))
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					)
				)
				.Defs(
					("anchorString", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Pattern("^[A-Za-z_][-A-Za-z0-9._]*$")),
					("uriString", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.Uri)),
					("uriReferenceString", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.UriReference))
				);

		/// <summary>
		/// The Draft 2020-12 Unevaluated meta-schema.
		/// </summary>
		public static readonly JsonSchema Unevaluated202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(Unevaluated202012Id)
				.Vocabulary((Vocabularies.Unevaluated202012Id, true))
				.DynamicAnchor("meta")
				.Title("Unevaluated applicator vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(UnevaluatedItemsKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					),
					(UnevaluatedPropertiesKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					)
				);

		/// <summary>
		/// The Draft 2020-12 Applicator meta-schema.
		/// </summary>
		public static readonly JsonSchema Applicator202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(Applicator202012Id)
				.Vocabulary((Vocabularies.Applicator202012Id, true))
				.DynamicAnchor("meta")
				.Title("Applicator vocabulary meta-schema")
				.Properties(
					(PrefixItemsKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/schemaArray")
					),
					(ItemsKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					),
					(ContainsKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					),
					(AdditionalPropertiesKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					),
					(PropertiesKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(new JsonSchemaBuilder()
							.DynamicRef("#meta")
						)
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					),
					(PatternPropertiesKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(new JsonSchemaBuilder()
							.DynamicRef("#meta")
						)
						.PropertyNames(new JsonSchemaBuilder()
							.Format(Formats.Regex)
						)
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					),
					(DependentSchemasKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(new JsonSchemaBuilder()
							.DynamicRef("#meta")
						)
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					),
					(PropertyNamesKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
						.Default(new Dictionary<string, JsonElement>().AsJsonElement())
					),
					(IfKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					),
					(ThenKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					),
					(ElseKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					),
					(AllOfKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/schemaArray")
					),
					(AnyOfKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/schemaArray")
					),
					(OneOfKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/schemaArray")
					),
					(NotKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					)
				)
				.Defs(
					("schemaArray", new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.MinItems(1)
						.Items(new JsonSchemaBuilder()
							.DynamicRef("#meta")
						)
					)
				);

		/// <summary>
		/// The Draft 2020-12 Validation meta-schema.
		/// </summary>
		public static readonly JsonSchema Validation202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(Validation202012Id)
				.Vocabulary((Vocabularies.Validation202012Id, true))
				.DynamicAnchor("meta")
				.Title("Validation vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(TypeKeyword.Name, new JsonSchemaBuilder()
						.AnyOf(
							new JsonSchemaBuilder().Ref("#/$defs/simpleTypes"),
							new JsonSchemaBuilder()
								.Type(SchemaValueType.Array)
								.Items(new JsonSchemaBuilder().Ref("#/$defs/simpleTypes"))
								.MinItems(1)
								.UniqueItems(true)
						)
					),
					(ConstKeyword.Name, true),
					(EnumKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(true)
					),
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
						.Default(false)
					),
					(MaxContainsKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
					),
					(MinContainsKeyword.Name, new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
						.Default(1)
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
					)
				)
				.Defs(
					("nonNegativeInteger", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Minimum(0)
					),
					("nonNegativeIntegerDefault0", new JsonSchemaBuilder()
						.Ref("#/$defs/nonNegativeInteger")
						.Default(0)
					),
					("simpleTypes", new JsonSchemaBuilder()
						.Enum(
							"array",
							"boolean",
							"integer",
							"null",
							"number",
							"object",
							"string"
						)
					),
					("stringArray", new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
						.UniqueItems(true)
						.Default(new JsonElement[0].AsJsonElement())
					)
				);

		/// <summary>
		/// The Draft 2020-12 Metadata meta-schema.
		/// </summary>
		public static readonly JsonSchema Metadata202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(Metadata202012Id)
				.Vocabulary((Vocabularies.Metadata202012Id, true))
				.DynamicAnchor("meta")
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
						.Default(false)
					),
					(ReadOnlyKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Boolean)
						.Default(false)
					),
					(WriteOnlyKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Boolean)
						.Default(false)
					),
					(ExamplesKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(true)
					)
				);

		/// <summary>
		/// The Draft 2020-12 Format-Annotation meta-schema.
		/// </summary>
		public static readonly JsonSchema FormatAnnotation202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(FormatAnnotation202012Id)
				.Vocabulary((Vocabularies.FormatAnnotation202012Id, true))
				.DynamicAnchor("meta")
				.Title("Format vocabulary meta-schema for annotation results")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(FormatKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					)
				);

		/// <summary>
		/// The Draft 2020-12 Format-Assertion meta-schema.
		/// </summary>
		public static readonly JsonSchema FormatAssertion202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(FormatAssertion202012Id)
				.Vocabulary((Vocabularies.FormatAssertion202012Id, true))
				.DynamicAnchor("meta")
				.Title("Format vocabulary meta-schema for assertion results")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(FormatKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					)
				);

		/// <summary>
		/// The Draft 2020-12 Content meta-schema.
		/// </summary>
		public static readonly JsonSchema Content202012 =
			new JsonSchemaBuilder()
				.Schema(Draft202012Id)
				.Id(Content202012Id)
				.Vocabulary((Vocabularies.Content202012Id, true))
				.DynamicAnchor("meta")
				.Title("Content vocabulary meta-schema")
				.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				.Properties(
					(ContentMediaTypeKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					(ContentMediaEncodingKeyword.Name, new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					(ContentSchemaKeyword.Name, new JsonSchemaBuilder()
						.DynamicRef("#meta")
					)
				);
	}
}