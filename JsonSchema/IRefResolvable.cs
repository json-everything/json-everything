using System;

namespace Json.Schema
{
	public interface IRefResolvable
	{
		IRefResolvable ResolvePointerSegment(string value);
		void RegisterSubschemas(SchemaRegistry registry, Uri currentUri);
	}
}