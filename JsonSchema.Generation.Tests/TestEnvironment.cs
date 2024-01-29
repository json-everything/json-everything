using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Generation.Tests
{
	internal static class TestEnvironment
	{
		public static JsonSerializerOptions SerializerOptions = new()
		{
#if NET8_0_OR_GREATER
			TypeInfoResolverChain = { GenerationTestsJsonSerializerContext.Default, JsonSchema.TypeInfoResolver },
#endif
		};

		public static JsonSerializerOptions SerializerOptionsWriteIndented = new(SerializerOptions)
		{
			WriteIndented = true
		};
	}

	[JsonSerializable(typeof(IJsonSchemaKeyword))]
	internal partial class  GenerationTestsJsonSerializerContext : JsonSerializerContext
	{
		
	}
}
