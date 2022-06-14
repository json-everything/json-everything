namespace JsonEverythingNet.Shared
{
	public class NamespaceNode
	{
		public string Name { get; set; }
		public List<Type> Types { get; set; }
		public List<NamespaceNode> Namespaces { get; set; }
	}
}
