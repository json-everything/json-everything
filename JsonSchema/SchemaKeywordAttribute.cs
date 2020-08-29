using System;

namespace Json.Schema
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class SchemaKeywordAttribute : Attribute
	{
		public string Name { get; }

		public SchemaKeywordAttribute(string name)
		{
			Name = name;
		}
	}
}