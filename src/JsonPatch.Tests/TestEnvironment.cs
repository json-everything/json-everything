using System.Text.Encodings.Web;
using System.Text.Json;
//using Json.Schema;
using NUnit.Framework;

namespace Json.Patch.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
			{
				TypeInfoResolverChain = { TestSerializerContext.Default },
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
			};
}