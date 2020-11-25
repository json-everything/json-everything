namespace Json.Schema.Generation.Intents
{
	public class FormatIntent : ISchemaKeywordIntent
	{
		public Format Format { get; }

		public FormatIntent(Format format)
		{
			Format = format;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Format(Format);
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
				hashCode = (hashCode * 397) ^ Format.GetHashCode();
				return hashCode;
			}
		}
	}
}