using Json.Schema.Keywords;

namespace Json.Schema;

public partial class Dialect
{
	public static Dialect Draft06 { get; } = new(
		new Keywords.Draft06.AdditionalItemsKeyword(),
		new AdditionalPropertiesKeyword(),
		new AllOfKeyword(),
		new AnyOfKeyword(),
		new CommentKeyword(),
		new ConstKeyword(),
		new Keywords.Draft06.ContainsKeyword(),
		new DefaultKeyword(),
		new Keywords.Draft06.DefinitionsKeyword(),
		new DependenciesKeyword(),
		new DescriptionKeyword(),
		new EnumKeyword(),
		new ExamplesKeyword(),
		new ExclusiveMaximumKeyword(),
		new ExclusiveMinimumKeyword(),
		new Keywords.Draft06.FormatKeyword(),
		new Keywords.Draft06.IdKeyword(),
		new Keywords.Draft06.ItemsKeyword(),
		new MaximumKeyword(),
		new MaxItemsKeyword(),
		new MaxLengthKeyword(),
		new MaxPropertiesKeyword(),
		new MinimumKeyword(),
		new MinItemsKeyword(),
		new MinLengthKeyword(),
		new MinPropertiesKeyword(),
		new MultipleOfKeyword(),
		new NotKeyword(),
		new OneOfKeyword(),
		new PatternKeyword(),
		new PatternPropertiesKeyword(),
		new PropertiesKeyword(),
		new PropertyNamesKeyword(),
		new RefKeyword(),
		new RequiredKeyword(),
		new SchemaKeyword(),
		new TitleKeyword(),
		new TypeKeyword(),
		new UniqueItemsKeyword()
	)
	{
		Id = MetaSchemas.Draft6Id,
		RefIgnoresSiblingKeywords = true,
		AllowUnknownKeywords = true,
		_readOnly = true
	};
}