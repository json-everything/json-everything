using System.Text.Encodings.Web;
using System.Text.Json;

namespace Json.Schema.CodeGeneration.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}
			.WithJsonSchema();
}