using System;

namespace Json.Schema
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public class VocabularyAttribute : Attribute
	{
		public Uri Id { get;}

		public VocabularyAttribute(string id)
		{
			Id = new Uri(id, UriKind.Absolute);
		}
	}
}