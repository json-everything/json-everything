using System;

namespace Json.Schema
{
	/// <summary>
	/// Indicates the keyword as it appears in a schema.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class SchemaKeywordAttribute : Attribute
	{
		/// <summary>
		/// The keyword name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Creates a new <see cref="SchemaKeywordAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public SchemaKeywordAttribute(string name)
		{
			Name = name;
		}
	}
}