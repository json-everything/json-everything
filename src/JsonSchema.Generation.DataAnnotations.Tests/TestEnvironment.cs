using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.Schema.Generation.DataAnnotations.Tests;

[SetUpFixture]
internal static class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

	[OneTimeSetUp]
	public static void Setup()
	{
		DataAnnotationsSupport.AddDataAnnotations();
	}
}

[JsonSerializable(typeof(JsonSchema))]
public partial class TestSerializerContext : JsonSerializerContext;
