using JsonEverythingNet.Services.MarkdownGen.Reflection;

namespace JsonEverythingNet.Services.MarkdownGen;

public class OrderedTypeList
{
	public OrderedTypeList(Type type)
	{
		TypeCollection = TypeCollection.ForReferencedTypes(type);
		TypesToDocument = TypeCollection.ReferencedTypes.Values
			.OrderBy(t => t.Type.Namespace)
			.ThenBy(t => t.Type.Name).ToList();
		TypesToDocumentSet = new HashSet<Type> { type };
	}

	public TypeCollection TypeCollection { get; set; }
	public List<TypeCollection.TypeInformation> TypesToDocument { get; set; }
	public HashSet<Type> TypesToDocumentSet { get; set; }
}