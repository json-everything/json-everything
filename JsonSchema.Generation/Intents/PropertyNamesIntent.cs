using System.Collections.Generic;

namespace Json.Schema.Generation.Intents
{
	/// <summary>
	/// Provides intent to create a `propertyNames` keyword.
	/// </summary>
	public class PropertyNamesIntent : ISchemaKeywordIntent, IContextContainer
	{
		/// <summary>
		/// The context that represents the inner requirements.
		/// </summary>
		public SchemaGeneratorContext Context { get; private set; }

		/// <summary>
		/// Creates a new <see cref="PropertyNamesIntent"/> instance.
		/// </summary>
		/// <param name="context">The context.</param>
		public PropertyNamesIntent(SchemaGeneratorContext context)
		{
			Context = context;
		}

		/// <summary>
		/// Gets the contexts.
		/// </summary>
		/// <returns>
		///	The <see cref="SchemaGeneratorContext"/>s contained by this object.
		/// </returns>
		/// <remarks>
		/// Only return the contexts contained directly by this object.  Do not fetch
		/// the child contexts of those contexts.
		/// </remarks>
		public IEnumerable<SchemaGeneratorContext> GetContexts()
		{
			return new[] {Context};
		}

		/// <summary>
		/// Replaces one context with another.
		/// </summary>
		/// <param name="hashCode">The hashcode of the context to replace.</param>
		/// <param name="newContext">The new context.</param>
		/// <remarks>
		/// To implement this, call <see cref="object.GetHashCode()"/> on the contained
		/// contexts.  If any match, replace them with <paramref name="newContext"/>.
		/// </remarks>
		public void Replace(int hashCode, SchemaGeneratorContext newContext)
		{
			var hc = Context.GetHashCode();
			if (hc == hashCode)
				Context = newContext;
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public void Apply(JsonSchemaBuilder builder)
		{
			builder.PropertyNames(Context.Apply());
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
				var hashCode = typeof(PropertyNamesIntent).GetHashCode();
				hashCode = (hashCode * 397) ^ Context.GetHashCode();
				return hashCode;
			}
		}
	}
}