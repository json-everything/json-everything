namespace Json.Schema.Generation.Intents
{
	public class UniqueItemsIntent : ISchemaKeywordIntent
	{
		public bool Value { get; }

		public UniqueItemsIntent(bool value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.UniqueItems(Value);
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