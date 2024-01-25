using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema.OpenApi
{
	[JsonSerializable(typeof(DiscriminatorKeyword))]
	internal partial class OpenApiSerializerContext : JsonSerializerContext
	{
	}
}
