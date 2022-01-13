using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Holder for an annotation value.
	/// </summary>
	public class Annotation
	{
		/// <summary>
		/// The keyword that created the annotation (acts as a key for lookup).
		/// </summary>
		public string Owner { get; }
		/// <summary>
		/// The annotation value.
		/// </summary>
		public object Value { get; }
		/// <summary>
		/// The pointer to the keyword that created the annotation.
		/// </summary>
		public JsonPointer Source { get; }
		
		internal bool WasConsolidated { get; set; }

		/// <summary>
		/// Creates a new <see cref="Annotation"/>.
		/// </summary>
		/// <param name="owner">The keyword that created the annotation (acts as a key for lookup).</param>
		/// <param name="value">The annotation value.</param>
		/// <param name="source">The pointer to the keyword that created the annotation.</param>
		public Annotation(string owner, object value, in JsonPointer source)
		{
			Owner = owner;
			Value = value;
			Source = source;
		}

		internal static Annotation CreateConsolidated(Annotation source)
		{
			return new Annotation(source.Owner, source.Value, source.Source) {WasConsolidated = true};
		}
	}
}
