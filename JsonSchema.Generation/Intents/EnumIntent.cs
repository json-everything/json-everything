using System.Collections.Generic;
using System.Linq;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Generation.Intents
{
	public class EnumIntent : ISchemaKeywordIntent
	{
		public List<string> Names { get; }

		public EnumIntent(List<string> names)
		{
			Names = names;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Enum(Names.Select(n => n.AsJsonElement()));
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = GetType().GetHashCode();
				hashCode = (hashCode * 397) ^ Names.GetCollectionHashCode();
				return hashCode;
			}
		}
	}
}