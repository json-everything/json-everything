namespace Json.Schema.Generation.Intents
{
	/// <summary>
	/// Provides intent to create a `type` keyword.
	/// </summary>
	public class TypeIntent : ISchemaKeywordIntent
	{
		/// <summary>
		/// The type.
		/// </summary>
		public SchemaValueType Type { get; set; }

		/// <summary>
		/// Creates a new <see cref="TypeIntent"/> instance.
		/// </summary>
		/// <param name="type">The type.</param>
		public TypeIntent(SchemaValueType type)
		{
			Type = type;
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Type(Type);
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
				hashCode = (hashCode * 397) ^ Type.GetHashCode();
				return hashCode;
			}
		}
	}
}