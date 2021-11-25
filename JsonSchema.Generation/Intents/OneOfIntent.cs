using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema.Generation.Intents
{
	/// <summary>
	/// Provides intent to create a `oneOf` keyword.
	/// </summary>
	public class OneOfIntent : ISchemaKeywordIntent
	{
		/// <summary>
		/// Gets the subschemas to include.
		/// </summary>
		public List<IEnumerable<ISchemaKeywordIntent>> Subschemas { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="OneOfIntent"/> class.
		/// </summary>
		/// <param name="subschemas">The subschemas to include.</param>
		public OneOfIntent(IEnumerable<IEnumerable<ISchemaKeywordIntent>> subschemas)
		{
			Subschemas = subschemas.ToList();
		}

		/// <summary>
		/// Creates a new instance of the <see cref="OneOfIntent"/> class.
		/// </summary>
		/// <param name="subschemas">The subschemas to include.</param>
		public OneOfIntent(params IEnumerable<ISchemaKeywordIntent>[] subschemas)
		{
			Subschemas = subschemas.ToList();
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public void Apply(JsonSchemaBuilder builder)
		{
			builder.OneOf(Subschemas.Select(Build));
		}

		private static JsonSchema Build(IEnumerable<ISchemaKeywordIntent> subschema)
		{
			var builder = new JsonSchemaBuilder();

			foreach (var intent in subschema)
			{
				intent.Apply(builder);
			}

			return builder;
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
				var hashCode = typeof(OneOfIntent).GetHashCode();
				hashCode = (hashCode * 397) ^ Subschemas.GetCollectionHashCode();
				return hashCode;
			}
		}
	}
}
