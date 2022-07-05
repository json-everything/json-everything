namespace JsonEverythingNet.Shared
{
	public class NamespaceNode
	{
#pragma warning disable CS8618
		public string Name { get; set; }
		public List<Type> Types { get; set; }
		public List<NamespaceNode> Namespaces { get; set; }
#pragma warning restore CS8618
	}
}