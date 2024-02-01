using System.Text.Encodings.Web;
using System.Text.Json;

namespace Json.Schema.Generation.Tests;

internal static class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}.WithJsonSchema();
}
