using System;
using System.Text.Json.Nodes;

namespace Json.Schema;

public static partial class MetaSchemas
{
	internal const string DraftNextIdValue = "https://json-schema.org/draft/next/schema";

	/// <summary>
	/// The Draft 2020-12 meta-schema ID.
	/// </summary>
	public static readonly Uri DraftNextId = new Uri(DraftNextIdValue);

	/// <summary>
	/// The Draft 2020-12 Core meta-schema ID.
	/// </summary>
	public static readonly Uri CoreNextId = new Uri("https://json-schema.org/draft/next/meta/core");
	/// <summary>
	/// The Draft 2020-12 Unevaluated meta-schema ID.
	/// </summary>
	public static readonly Uri UnevaluatedNextId = new Uri("https://json-schema.org/draft/next/meta/unevaluated");
	/// <summary>
	/// The Draft 2020-12 Applicator meta-schema ID.
	/// </summary>
	public static readonly Uri ApplicatorNextId = new Uri("https://json-schema.org/draft/next/meta/applicator");
	/// <summary>
	/// The Draft 2020-12 Validation meta-schema ID.
	/// </summary>
	public static readonly Uri ValidationNextId = new Uri("https://json-schema.org/draft/next/meta/validation");
	/// <summary>
	/// The Draft 2020-12 Metadata meta-schema ID.
	/// </summary>
	public static readonly Uri MetadataNextId = new Uri("https://json-schema.org/draft/next/meta/meta-data");
	/// <summary>
	/// The Draft 2020-12 Format-Annotation meta-schema ID.
	/// </summary>
	public static readonly Uri FormatAnnotationNextId = new Uri("https://json-schema.org/draft/next/meta/format-annotation");
	/// <summary>
	/// The Draft 2020-12 Format-Assertion meta-schema ID.
	/// </summary>
	public static readonly Uri FormatAssertionNextId = new Uri("https://json-schema.org/draft/next/meta/format-assertion");
	/// <summary>
	/// The Draft 2020-12 Content meta-schema ID.
	/// </summary>
	public static readonly Uri ContentNextId = new Uri("https://json-schema.org/draft/next/meta/content");

	/// <summary>
	/// The Draft 2020-12 meta-schema.
	/// </summary>
	public static readonly JsonSchema DraftNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(DraftNextId)
			.Vocabulary(
				(Vocabularies.CoreNextId, true),
				(Vocabularies.ApplicatorNextId, true),
				(Vocabularies.UnevaluatedNextId, true),
				(Vocabularies.ValidationNextId, true),
				(Vocabularies.MetadataNextId, true),
				(Vocabularies.FormatAnnotationNextId, true),
				(Vocabularies.ContentNextId, true)
			)
			.DynamicAnchor("meta")
			.Title("Core and Validation specifications meta-schema")
			.AllOf(
				new JsonSchemaBuilder().Ref("meta/core"),
				new JsonSchemaBuilder().Ref("meta/applicator"),
				new JsonSchemaBuilder().Ref("meta/unevaluated"),
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
					.Deprecated(true)
					.Default(new JsonObject())
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
					.Default(new JsonObject())
				),
				(RecursiveAnchorKeyword.Name, new JsonSchemaBuilder()
					.Comment("\"$recursiveAnchor\" has been replaced by \"$dynamicAnchor\".")
					.Ref("meta/core#/$defs/anchorString")
					.Deprecated(true)
				),
				(RecursiveRefKeyword.Name, new JsonSchemaBuilder()
					.Comment("\"$recursiveRef\" has been replaced by \"$dynamicRef\".")
					.Type(SchemaValueType.String)
					.Format(Formats.UriReference)
					.Deprecated(true)
				)
			);

	/// <summary>
	/// The Draft 2020-12 Core meta-schema.
	/// </summary>
	public static readonly JsonSchema CoreNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(CoreNextId)
			.Vocabulary((Vocabularies.CoreNextId, true))
			.DynamicAnchor("meta")
			.Title("Core vocabulary meta-schema")
			.Type(SchemaValueType.Object | SchemaValueType.Boolean)
			.Properties(
				(IdKeyword.Name, new JsonSchemaBuilder()
					.Ref("#/$defs/iriReferenceString")
					.Comment("Non-empty fragments not allowed.")
					.Pattern("^[^#]*#?$")
				),
				(SchemaKeyword.Name, new JsonSchemaBuilder()
					.Ref("#/$defs/iriString")
				),
				(RefKeyword.Name, new JsonSchemaBuilder()
					.Ref("#/$defs/iriReferenceString")
				),
				(AnchorKeyword.Name, new JsonSchemaBuilder()
					.Ref("#/$defs/anchorString")
				),
				(DynamicRefKeyword.Name, new JsonSchemaBuilder()
					.Ref("#/$defs/iriReferenceString")
				),
				(DynamicAnchorKeyword.Name, new JsonSchemaBuilder()
					.Ref("#/$defs/anchorString")
				),
				(VocabularyKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.PropertyNames(new JsonSchemaBuilder()
						.Ref("#/$defs/iriString")
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
				)
			)
			.Defs(
				("anchorString", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Pattern("^[A-Za-z_][-A-Za-z0-9._]*$")),
				("iriString", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.Iri)),
				("iriReferenceString", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.IriReference))
			);

	/// <summary>
	/// The Draft 2020-12 Unevaluated meta-schema.
	/// </summary>
	public static readonly JsonSchema UnevaluatedNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(UnevaluatedNextId)
			.Vocabulary((Vocabularies.UnevaluatedNextId, true))
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
	public static readonly JsonSchema ApplicatorNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(ApplicatorNextId)
			.Vocabulary((Vocabularies.ApplicatorNextId, true))
			.DynamicAnchor("meta")
			.Title("Applicator vocabulary meta-schema")
			.Type(SchemaValueType.Object | SchemaValueType.Boolean)
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
				),
				(PropertiesKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.DynamicRef("#meta")
					)
					.Default(new JsonObject())
				),
				(PatternPropertiesKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.DynamicRef("#meta")
					)
					.PropertyNames(new JsonSchemaBuilder()
						.Format(Formats.Regex)
					)
					.Default(new JsonObject())
				),
				(DependentSchemasKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.DynamicRef("#meta")
					)
					.Default(new JsonObject())
				),
				(PropertyDependenciesKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.AdditionalProperties(new JsonSchemaBuilder()
							.DynamicRef("#meta")
							.Default(new JsonObject())
						)
						.Default(new JsonObject())
					)
				),
				(PropertyNamesKeyword.Name, new JsonSchemaBuilder()
					.DynamicRef("#meta")
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
	public static readonly JsonSchema ValidationNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(ValidationNextId)
			.Vocabulary((Vocabularies.ValidationNextId, true))
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
					.Default(new JsonArray())
				)
			);

	/// <summary>
	/// The Draft 2020-12 Metadata meta-schema.
	/// </summary>
	public static readonly JsonSchema MetadataNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(MetadataNextId)
			.Vocabulary((Vocabularies.MetadataNextId, true))
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
	public static readonly JsonSchema FormatAnnotationNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(FormatAnnotationNextId)
			.Vocabulary((Vocabularies.FormatAnnotationNextId, true))
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
	public static readonly JsonSchema FormatAssertionNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(FormatAssertionNextId)
			.Vocabulary((Vocabularies.FormatAssertionNextId, true))
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
	public static readonly JsonSchema ContentNext =
		new JsonSchemaBuilder()
			.Schema(DraftNextId)
			.Id(ContentNextId)
			.Vocabulary((Vocabularies.ContentNextId, true))
			.DynamicAnchor("meta")
			.Title("Content vocabulary meta-schema")
			.Type(SchemaValueType.Object | SchemaValueType.Boolean)
			.Properties(
				(ContentMediaTypeKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
				),
				(ContentEncodingKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
				),
				(ContentSchemaKeyword.Name, new JsonSchemaBuilder()
					.DynamicRef("#meta")
				)
			);
}