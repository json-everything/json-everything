using System.Collections.Generic;
using Json.Pointer;

namespace Json.Schema.Generation.Intents
{
	/// <summary>
	/// Provides intent to create a `required` keyword.
	/// </summary>
	public class RequiredIntent : ISchemaKeywordIntent
	{
		/// <summary>
		/// The required property names.
		/// </summary>
		public List<string> RequiredProperties { get; }

		/// <summary>
		/// Creates a new <see cref="RequiredIntent"/> instance.
		/// </summary>
		/// <param name="requiredProperties">The required property names.</param>
		public RequiredIntent(List<string> requiredProperties)
		{
			RequiredProperties = requiredProperties;
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Required(RequiredProperties);
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
				hashCode = (hashCode * 397) ^ RequiredProperties.GetCollectionHashCode();
				return hashCode;
			}
		}
	}
}