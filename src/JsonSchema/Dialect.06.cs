using Json.Schema.Keywords;

namespace Json.Schema;

public partial class Dialect
{
	/// <summary>
	/// Gets the JSON Schema dialect definition for Draft 6.
	/// </summary>
	/// <remarks>Use this property to evaluate schemas that conform to the JSON Schema Draft 6 specification.
	/// The dialect includes all standard Draft 6 keywords and allows unknown keywords. Sibling keywords are
	/// ignored when resolving references using the '$ref' keyword.</remarks>
	public static Dialect Draft06 { get; } = new(
		Keywords.Draft06.AdditionalItemsKeyword.Instance,
		AdditionalPropertiesKeyword.Instance,
		AllOfKeyword.Instance,
		AnyOfKeyword.Instance,
		CommentKeyword.Instance,
		ConstKeyword.Instance,
		Keywords.Draft06.ContainsKeyword.Instance,
		DefaultKeyword.Instance,
		Keywords.Draft06.DefinitionsKeyword.Instance,
		Keywords.Draft06.DependenciesKeyword.Instance,
		DescriptionKeyword.Instance,
		EnumKeyword.Instance,
		ExamplesKeyword.Instance,
		ExclusiveMaximumKeyword.Instance,
		ExclusiveMinimumKeyword.Instance,
		Keywords.Draft06.FormatKeyword.Annotate,
		Keywords.Draft06.IdKeyword.Instance,
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
		RefKeyword.Instance,
		RequiredKeyword.Instance,
		SchemaKeyword.Instance,
		TitleKeyword.Instance,
		TypeKeyword.Instance,
		UniqueItemsKeyword.Instance
	)
	{
		Id = MetaSchemas.Draft6Id,
		RefIgnoresSiblingKeywords = true,
		AllowUnknownKeywords = true,
		_readOnly = true
	};
}