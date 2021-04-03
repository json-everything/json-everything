using System;

namespace Json.Schema
{
	internal interface IAnchorProvider
	{
		void RegisterAnchor(SchemaRegistry registry, Uri currentUri, JsonSchema schema);
	}
}