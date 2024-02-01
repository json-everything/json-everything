using System.Text.Json;

namespace Json.Schema.Generation.Tests;

internal static class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptionsWriteIndented =
		new(JsonSchemaSerializerContext.Default.Options)
	{
		WriteIndented = true
	};
}
