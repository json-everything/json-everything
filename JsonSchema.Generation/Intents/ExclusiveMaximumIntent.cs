namespace Json.Schema.Generation.Intents
{
	internal class ExclusiveMaximumIntent : ISchemaKeywordIntent
	{
		public decimal Value { get; }

		public ExclusiveMaximumIntent(decimal value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.ExclusiveMaximum(Value);
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
				hashCode = (hashCode * 397) ^ Value.GetHashCode();
				return hashCode;
			}
		}
	}
}