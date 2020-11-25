namespace Json.Schema.Generation.Intents
{
	public class TypeIntent : ISchemaKeywordIntent
	{
		public SchemaValueType Type { get; }

		public TypeIntent(SchemaValueType type)
		{
			Type = type;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Type(Type);
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
				hashCode = (hashCode * 397) ^ Type.GetHashCode();
				return hashCode;
			}
		}
	}
}