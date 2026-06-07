using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

/// <summary>
/// Regression tests for a schema-registration collision that occurred when `$id` appeared
/// after subschema-producing keywords (e.g. `$defs` / `properties`) in document order.
/// This happens whenever a schema is serialized in canonical / alphabetical key order, which
/// sorts `$defs` before `$id`. Two or more subschemas built before `$id` inherited the
/// placeholder base URI and were then mis-detected as embedded resources, throwing
/// "Overwriting registered schemas is not permitted." JSON object key order is not
/// significant, so `$id` must establish the resource base URI regardless of its position.
/// </summary>
public class IdOrderingTests
{
	[Test]
	public void DefsBeforeId_DoesNotThrowAndResolvesInternalRefs()
	{
		const string schemaText = @"{
  ""$defs"": { ""a"": { ""type"": ""string"" }, ""b"": { ""type"": ""integer"" } },
  ""$id"": ""https://example.com/id-after-defs.json"",
  ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
  ""type"": ""object"",
  ""properties"": { ""x"": { ""$ref"": ""#/$defs/a"" }, ""y"": { ""$ref"": ""#/$defs/b"" } }
}";

		var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };
		JsonSchema schema = null!;
		Assert.DoesNotThrow(() => schema = JsonSchema.FromText(schemaText, options));

		// The internal `#/$defs/...` refs resolve against the base URI that `$id` establishes;
		// if that base were wrong (the pre-fix placeholder) they would not resolve correctly.
		var valid = schema.Evaluate(JsonDocument.Parse(@"{ ""x"": ""hello"", ""y"": 5 }").RootElement);
		Assert.That(valid.IsValid, Is.True);

		var invalid = schema.Evaluate(JsonDocument.Parse(@"{ ""x"": 5, ""y"": ""nope"" }").RootElement);
		Assert.That(invalid.IsValid, Is.False);
	}

	[Test]
	public void PropertiesBeforeId_DoesNotThrow()
	{
		// No `$defs` and no `$ref`: two subschemas under `properties` built before a late
		// `$id` are by themselves enough to trigger the registration collision.
		const string schemaText = @"{
  ""properties"": { ""a"": { ""type"": ""string"" }, ""b"": { ""type"": ""number"" } },
  ""$id"": ""https://example.com/props-before-id.json"",
  ""type"": ""object""
}";

		var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };
		Assert.DoesNotThrow(() => JsonSchema.FromText(schemaText, options));
	}

	[Test]
	public void IdPositionDoesNotChangeEvaluation()
	{
		// `$id` before vs after the subschema keywords must build and evaluate equivalently.
		const string idFirst = @"{
  ""$id"": ""https://example.com/order-a.json"",
  ""$defs"": { ""s"": { ""type"": ""string"" } },
  ""type"": ""object"",
  ""properties"": { ""x"": { ""$ref"": ""#/$defs/s"" } }
}";
		const string idLast = @"{
  ""$defs"": { ""s"": { ""type"": ""string"" } },
  ""properties"": { ""x"": { ""$ref"": ""#/$defs/s"" } },
  ""type"": ""object"",
  ""$id"": ""https://example.com/order-b.json""
}";

		var a = JsonSchema.FromText(idFirst, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var b = JsonSchema.FromText(idLast, new BuildOptions { SchemaRegistry = new SchemaRegistry() });

		var good = JsonDocument.Parse(@"{ ""x"": ""ok"" }").RootElement;
		Assert.That(a.Evaluate(good).IsValid, Is.True);
		Assert.That(b.Evaluate(good).IsValid, Is.True);

		var bad = JsonDocument.Parse(@"{ ""x"": 1 }").RootElement;
		Assert.That(a.Evaluate(bad).IsValid, Is.False);
		Assert.That(b.Evaluate(bad).IsValid, Is.False);
	}
}
