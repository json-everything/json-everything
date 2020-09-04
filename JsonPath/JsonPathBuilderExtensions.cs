namespace JsonPath
{
	public static class JsonPathBuilderExtensions
	{
		public static JsonPathBuilder Property(this JsonPathBuilder builder, string prop)
		{
			builder.Add(new PropertyNode(prop));
			return builder;
		}

		public static JsonPathBuilder AllProperties(this JsonPathBuilder builder)
		{
			builder.Add(new PropertyNode(null));
			return builder;
		}

		public static JsonPathBuilder Index(this JsonPathBuilder builder, params IndexOrRange[] ranges)
		{
			builder.Add(new IndexNode(ranges));
			return builder;
		}

		public static JsonPathBuilder Recursive(this JsonPathBuilder builder)
		{
			builder.Add(new RecursiveNode());
			return builder;
		}
	}
}