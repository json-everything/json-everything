using System.Collections.Generic;
using System.Linq;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Generation.Intents
{
	/// <summary>
	/// Provides intent to create an `enum` keyword.
	/// </summary>
	public class EnumIntent : ISchemaKeywordIntent
	{
		/// <summary>
		/// The names defined by the enumeration.
		/// </summary>
		public List<string> Names { get; set; }

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="names">The names defined by the enumeration.</param>
		public EnumIntent(IEnumerable<string> names)
		{
			Names = names.ToList();
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="names">The names defined by the enumeration.</param>
		public EnumIntent(params string[] names)
		{
			Names = names.ToList();
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Enum(Names.Select(n => n.AsJsonElement()));
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
				hashCode = (hashCode * 397) ^ Names.GetCollectionHashCode();
				return hashCode;
			}
		}
	}
}