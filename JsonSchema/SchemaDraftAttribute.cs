using System;

namespace Json.Schema
{
	/// <summary>
	/// Indicates which JSON Schema draft versions are supported by a keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public class SchemaDraftAttribute : Attribute
	{
		/// <summary>
		/// The supported draft.
		/// </summary>
		public Draft Draft { get; }

		/// <summary>
		/// Creates a new <see cref="SchemaDraftAttribute"/>.
		/// </summary>
		/// <param name="draft">The supported draft.</param>
		public SchemaDraftAttribute(Draft draft)
		{
			Draft = draft;
		}
	}
}