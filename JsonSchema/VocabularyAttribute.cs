using System;

namespace Json.Schema
{
	/// <summary>
	/// Indicates the ID of the vocabulary a keyword belongs to.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public class VocabularyAttribute : Attribute
	{
		/// <summary>
		/// The vocabulary ID.
		/// </summary>
		public Uri Id { get;}

		/// <summary>
		/// Creates a new <see cref="VocabularyAttribute"/>.
		/// </summary>
		/// <param name="id">The vocabulary ID.</param>
		public VocabularyAttribute(string id)
		{
			Id = new Uri(id, UriKind.Absolute);
		}
	}
}