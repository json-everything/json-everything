using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Generation.Tests
{
	internal static class TestEnvironment
	{
		public static readonly JsonSerializerOptions SerializerOptions = new()
		{
			TypeInfoResolverChain = { GenerationTestsJsonSerializerContext.Default, JsonSchema.TypeInfoResolver },
		};

		public static readonly JsonSerializerOptions SerializerOptionsWriteIndented = new(SerializerOptions)
		{
			WriteIndented = true
		};
	}

	[JsonSerializable(typeof(IJsonSchemaKeyword))]
	internal partial class  GenerationTestsJsonSerializerContext : JsonSerializerContext
	{
		
	}
}
