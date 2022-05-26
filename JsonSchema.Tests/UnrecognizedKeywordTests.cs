using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class UnrecognizedKeywordTests
{
	[Test]
	public void FooIsNotAKeyword()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText);

		Assert.AreEqual(1, schema!.Keywords!.Count);
		Assert.IsInstanceOf<UnrecognizedKeyword>(schema.Keywords.First());
	}

	[Test]
	public void FooProducesAnAnnotation()
	{
		var schemaText = "{\"foo\": \"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText);

		var result = schema!.Validate("{}", new ValidationOptions { OutputFormat = OutputFormat.Detailed });

		Assert.IsTrue(result.IsValid);
		Assert.AreEqual(1, result.Annotations.Count());
		Assert.IsTrue("bar".AsJsonElement().IsEquivalentTo((JsonElement)result.Annotations.First().Value));
	}

	[Test]
	public void FooIsIncludedInSerialization()
	{
		var schemaText = "{\"foo\":\"bar\"}";

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText);

		var reText = JsonSerializer.Serialize(schema);

		Assert.AreEqual(schemaText, reText);
	}
}