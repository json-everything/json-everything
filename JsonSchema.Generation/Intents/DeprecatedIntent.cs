namespace Json.Schema.Generation.Intents
{
	internal class DeprecatedIntent : ISchemaKeywordIntent
	{
		public bool Deprecated { get; }

		public DeprecatedIntent(bool deprecated)
		{
			Deprecated = deprecated;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Deprecated(Deprecated);
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
				hashCode = (hashCode * 397) ^ Deprecated.GetHashCode();
				return hashCode;
			}
		}
	}
}