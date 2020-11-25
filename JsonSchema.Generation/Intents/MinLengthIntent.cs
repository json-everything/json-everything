namespace Json.Schema.Generation.Intents
{
	internal class MinLengthIntent : ISchemaKeywordIntent
	{
		public uint Value { get; }

		public MinLengthIntent(uint value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.MinLength(Value);
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