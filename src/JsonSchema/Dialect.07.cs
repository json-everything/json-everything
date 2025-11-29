using Json.Schema.Keywords;

namespace Json.Schema;

public partial class Dialect
{
	public static Dialect Draft07 { get; } = new(
		Keywords.Draft06.AdditionalItemsKeyword.Instance,
		AdditionalPropertiesKeyword.Instance,
		AllOfKeyword.Instance,
		AnyOfKeyword.Instance,
		CommentKeyword.Instance,
		ConstKeyword.Instance,
		Keywords.Draft06.ContainsKeyword.Instance,
		ContentEncodingKeyword.Instance,
		ContentMediaTypeKeyword.Instance,
		DefaultKeyword.Instance,
		Keywords.Draft06.DefinitionsKeyword.Instance,
		DependenciesKeyword.Instance,
		DescriptionKeyword.Instance,
		ElseKeyword.Instance,
		EnumKeyword.Instance,
		ExamplesKeyword.Instance,
		ExclusiveMaximumKeyword.Instance,
		ExclusiveMinimumKeyword.Instance,
		Keywords.Draft06.FormatKeyword.Annotate,
		Keywords.Draft06.IdKeyword.Instance,
		IfKeyword.Instance,
		Keywords.Draft06.ItemsKeyword.Instance,
		MaximumKeyword.Instance,
		MaxItemsKeyword.Instance,
		MaxLengthKeyword.Instance,
		MaxPropertiesKeyword.Instance,
		MinimumKeyword.Instance,
		MinItemsKeyword.Instance,
		MinLengthKeyword.Instance,
		MinPropertiesKeyword.Instance,
		MultipleOfKeyword.Instance,
		NotKeyword.Instance,
		OneOfKeyword.Instance,
		PatternKeyword.Instance,
		PatternPropertiesKeyword.Instance,
		PropertiesKeyword.Instance,
		PropertyNamesKeyword.Instance,
		ReadOnlyKeyword.Instance,
		RefKeyword.Instance,
		RequiredKeyword.Instance,
		SchemaKeyword.Instance,
		ThenKeyword.Instance,
		TitleKeyword.Instance,
		TypeKeyword.Instance,
		UniqueItemsKeyword.Instance,
		WriteOnlyKeyword.Instance
	)
	{
		Id = MetaSchemas.Draft7Id,
		RefIgnoresSiblingKeywords = true,
		AllowUnknownKeywords = true,
		_readOnly = true
	};
}