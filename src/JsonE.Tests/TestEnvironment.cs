using System.Text.Encodings.Web;
using System.Text.Json;
using Json.JsonE.Tests.Suite;
using NUnit.Framework;

namespace Json.JsonE.Tests;

[SetUpFixture]
public class TestEnvironment
{
	public static readonly JsonSerializerOptions SerializerOptions =
		new()
		{
			TypeInfoResolverChain = { JsonETestSerializerContext.Default },
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
}