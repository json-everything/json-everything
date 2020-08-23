using System;

namespace Json.Schema
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public class SchemaDraftAttribute : Attribute
	{
		public Draft Draft { get; }

		public SchemaDraftAttribute(Draft draft)
		{
			Draft = draft;
		}
	}
}