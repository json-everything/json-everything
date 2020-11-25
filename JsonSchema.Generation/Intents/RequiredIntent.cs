using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema.Generation.Intents
{
	internal class RequiredIntent : ISchemaKeywordIntent
	{
		public List<string> RequiredProperties { get; }

		public RequiredIntent(List<string> requiredProperties)
		{
			RequiredProperties = requiredProperties;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Required(RequiredProperties);
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
				hashCode = (hashCode * 397) ^ RequiredProperties.GetCollectionHashCode();
				return hashCode;
			}
		}
	}
}