using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Generation.Tests;

internal static class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
}

[JsonSerializable(typeof(JsonSchema))]
[JsonSerializable(typeof(PropertiesKeyword))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
public partial class TestSerializerContext : JsonSerializerContext;
