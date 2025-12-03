using Json.Schema.Keywords;
// ReSharper disable UnusedMember.Global

namespace Json.Schema;

public partial class Dialect
{
	/// <summary>
	/// Gets the JSON Schema dialect definition for v1/2026.
	/// </summary>
	/// <remarks>Use this property to evaluate schemas that conform to the v1/2026 specification.
	/// The dialect includes all standard v1/2026 keywords and disallows unknown keywords. Sibling keywords are
	/// processed when resolving references using the "$ref" keyword.</remarks>
	public static Dialect V1_2026 { get; } = new(
		AdditionalPropertiesKeyword.Instance,
		AllOfKeyword.Instance,
		AnchorKeyword.Instance,
		AnyOfKeyword.Instance,
		CommentKeyword.Instance,
		ConstKeyword.Instance,
		ContainsKeyword.Instance,
		ContentEncodingKeyword.Instance,
		ContentMediaTypeKeyword.Instance,
		ContentSchemaKeyword.Instance,
		DefaultKeyword.Instance,
		DefsKeyword.Instance,
		DependentRequiredKeyword.Instance,
		DependentSchemasKeyword.Instance,
		DeprecatedKeyword.Instance,
		DescriptionKeyword.Instance,
		DynamicAnchorKeyword.Instance,
		DynamicRefKeyword.Instance,
		ElseKeyword.Instance,
		EnumKeyword.Instance,
		ExamplesKeyword.Instance,
		ExclusiveMaximumKeyword.Instance,
		ExclusiveMinimumKeyword.Instance,
		FormatKeyword.Instance,
		IdKeyword.Instance,
		IfKeyword.Instance,
		ItemsKeyword.Instance,
		MaxContainsKeyword.Instance,
		MaximumKeyword.Instance,
		MaxItemsKeyword.Instance,
		MaxLengthKeyword.Instance,
		MaxPropertiesKeyword.Instance,
		MinContainsKeyword.Instance,
		MinimumKeyword.Instance,
		MinItemsKeyword.Instance,
		MinLengthKeyword.Instance,
		MinPropertiesKeyword.Instance,
		MultipleOfKeyword.Instance,
		NotKeyword.Instance,
		OneOfKeyword.Instance,
		PatternKeyword.Instance,
		PatternPropertiesKeyword.Instance,
		PrefixItemsKeyword.Instance,
		PropertiesKeyword.Instance,
		// PropertyDependenciesKeyword.Instance,  // TODO: is proposal
		PropertyNamesKeyword.Instance,
		ReadOnlyKeyword.Instance,
		RefKeyword.Instance,
		RequiredKeyword.Instance,
		SchemaKeyword.Instance,
		ThenKeyword.Instance,
		TitleKeyword.Instance,
		TypeKeyword.Instance,
		UnevaluatedItemsKeyword.Instance,
		UnevaluatedPropertiesKeyword.Instance,
		UniqueItemsKeyword.Instance,
		WriteOnlyKeyword.Instance
	)
	{
		Id = MetaSchemas.V1_2026Id
	};

	public static Dialect V1 { get; } = V1_2026;

}