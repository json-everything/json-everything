using System.Linq;

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

		public static JsonPathBuilder Index(this JsonPathBuilder builder, IIndexExpression firstRange, params IIndexExpression[] additionalRanges)
		{
			builder.Add(new IndexNode(firstRange, additionalRanges));
			return builder;
		}

		public static JsonPathBuilder AllIndices(this JsonPathBuilder builder)
		{
			builder.Add(new IndexNode(null));
			return builder;
		}

		public static JsonPathBuilder Recursive(this JsonPathBuilder builder)
		{
			builder.Add(new RecursiveNode());
			return builder;
		}
	}
}