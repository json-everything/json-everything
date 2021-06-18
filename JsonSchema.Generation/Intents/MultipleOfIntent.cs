namespace Json.Schema.Generation.Intents
{
	/// <summary>
	/// Provides intent to create a `multipleOf` keyword.
	/// </summary>
	public class MultipleOfIntent : ISchemaKeywordIntent
	{
		/// <summary>
		/// The value.
		/// </summary>
		public decimal Value { get; set; }

		/// <summary>
		/// Creates a new <see cref="MultipleOfIntent"/> instance.
		/// </summary>
		/// <param name="value">The value.</param>
		public MultipleOfIntent(decimal value)
		{
			Value = value;
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public void Apply(JsonSchemaBuilder builder)
		{
			builder.MultipleOf(Value);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object? obj)
		{
			return !ReferenceEquals(null, obj);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
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