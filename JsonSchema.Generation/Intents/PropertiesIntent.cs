using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation.Intents
{
	/// <summary>
	/// Provides intent to create an `additionalProperties` keyword.
	/// </summary>
	public class PropertiesIntent : ISchemaKeywordIntent, IContextContainer
	{
		/// <summary>
		/// The contexts that represent the properties.
		/// </summary>
		public Dictionary<string, SchemaGeneratorContext> Properties { get; }

		/// <summary>
		/// Creates a new <see cref="PropertiesIntent"/> instance.
		/// </summary>
		/// <param name="properties">The contexts.</param>
		public PropertiesIntent(Dictionary<string, SchemaGeneratorContext> properties)
		{
			Properties = properties;
		}

		/// <summary>
		/// Gets the contexts.
		/// </summary>
		/// <returns>
		///	The <see cref="SchemaGeneratorContext"/>s contained by this object.
		/// </returns>
		public IEnumerable<SchemaGeneratorContext> GetContexts()
		{
			return Properties.Values;
		}

		/// <summary>
		/// Replaces one context with another.
		/// </summary>
		/// <param name="hashCode">The hashcode of the context to replace.</param>
		/// <param name="newContext">The new context.</param>
		public void Replace(int hashCode, SchemaGeneratorContext newContext)
		{
			foreach (var property in Properties.ToList())
			{
				var hc = property.Value.GetHashCode();
				if (hc == hashCode)
					Properties[property.Key] = newContext;
			}
		}

		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Properties(Properties.ToDictionary(p => p.Key, p => p.Value.Apply().Build()));
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
				foreach (var property in Properties)
				{
					hashCode = (hashCode * 397) ^ property.Key.GetHashCode();
					hashCode = (hashCode * 397) ^ property.Value.GetHashCode();
				}
				return hashCode;
			}
		}
	}
}