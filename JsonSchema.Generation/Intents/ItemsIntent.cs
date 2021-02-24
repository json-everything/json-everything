using System.Collections.Generic;

namespace Json.Schema.Generation.Intents
{
	/// <summary>
	/// Provides intent to create an `items` keyword.
	/// </summary>
	public class ItemsIntent : ISchemaKeywordIntent, IContextContainer
	{
		/// <summary>
		/// The context that represents the inner requirements.
		/// </summary>
		public SchemaGeneratorContext Context { get; private set; }

		/// <summary>
		/// Creates a new <see cref="ItemsIntent"/> instance.
		/// </summary>
		/// <param name="context">The context.</param>
		public ItemsIntent(SchemaGeneratorContext context)
		{
			Context = context;
		}

		/// <summary>
		/// Gets the contexts.
		/// </summary>
		/// <returns>
		///	The <see cref="SchemaGeneratorContext"/>s contained by this object.
		/// </returns>
		public IEnumerable<SchemaGeneratorContext> GetContexts()
		{
			return new[] {Context};
		}

		/// <summary>
		/// Replaces one context with another.
		/// </summary>
		/// <param name="hashCode">The hashcode of the context to replace.</param>
		/// <param name="newContext">The new context.</param>
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
			builder.Items(Context.Apply());
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
				hashCode = (hashCode * 397) ^ Context.GetHashCode();
				return hashCode;
			}
		}
	}
}