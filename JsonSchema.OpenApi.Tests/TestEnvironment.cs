using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.OpenApi.Tests;

public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			}
			.WithJsonSchema()
			.WithOpenApiVocab();

	[OneTimeSetUp]
	public void Setup()
	{
		Vocabularies.Register();
		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}
}