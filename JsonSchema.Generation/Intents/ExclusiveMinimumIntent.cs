namespace Json.Schema.Generation.Intents
{
	public class ExclusiveMinimumIntent : ISchemaKeywordIntent
	{
		public decimal Value { get; }

		public ExclusiveMinimumIntent(decimal value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.ExclusiveMinimum(Value);
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