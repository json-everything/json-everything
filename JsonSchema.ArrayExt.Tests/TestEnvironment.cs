using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.ArrayExt.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static JsonSerializerOptions SerializerOptions = new()
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	[OneTimeSetUp]
	public void Setup()
	{
		Vocabularies.Register();
		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}
}