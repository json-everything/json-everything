using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class SerializationTests
{
	[TestCase("true")]
	[TestCase("false")]
	[TestCase("{\"$anchor\":\"value\"}")]
	[TestCase("{\"$comment\":\"some text\"}")]
	[TestCase("{\"$defs\":{}}")]
	[TestCase("{\"$dynamicAnchor\":\"meta\"}")]
	[TestCase("{\"$dynamicRef\":\"#meta\"}")]
	[TestCase("{\"$id\":\"http://some.site/schema\"}")]
	[TestCase("{\"$recursiveAnchor\":true}")]
	[TestCase("{\"$recursiveRef\":\"#\"}")]
	[TestCase("{\"$ref\":\"#/json/pointerOld\"}")]
	[TestCase("{\"$schema\":\"http://some.site/schema\"}")]
	[TestCase("{\"$vocabulary\":{\"http://some.site/vocab\":false}}")]
	[TestCase("{\"additionalItems\":true}")]
	[TestCase("{\"additionalItems\":false}")]
	[TestCase("{\"additionalItems\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"additionalProperties\":true}")]
	[TestCase("{\"additionalProperties\":false}")]
	[TestCase("{\"additionalProperties\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"allOf\":[{\"$id\":\"http://some.site/schema\"}]}")]
	[TestCase("{\"anyOf\":[{\"$id\":\"http://some.site/schema\"}]}")]
	[TestCase("{\"const\":\"some text\"}")]
	[TestCase("{\"const\":9}")]
	[TestCase("{\"const\":9.0}")]
	[TestCase("{\"const\":true}")]
	[TestCase("{\"const\":false}")]
	[TestCase("{\"const\":null}")]
	[TestCase("{\"const\":[]}")]
	[TestCase("{\"const\":{}}")]
	[TestCase("{\"contains\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"contentEncoding\":\"some-text\"}")]
	[TestCase("{\"contentMediaType\":\"some-text\"}")]
	[TestCase("{\"contentSchema\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"default\":\"some text\"}")]
	[TestCase("{\"default\":9}")]
	[TestCase("{\"default\":9.0}")]
	[TestCase("{\"default\":true}")]
	[TestCase("{\"default\":false}")]
	[TestCase("{\"default\":null}")]
	[TestCase("{\"default\":[]}")]
	[TestCase("{\"default\":{}}")]
	[TestCase("{\"definitions\":{}}")]
	[TestCase("{\"dependencies\":{\"foo\":[\"bar\"]}}")]
	[TestCase("{\"dependencies\":{\"foo\":{}}}")]
	[TestCase("{\"dependentRequired\":{\"foo\":[\"bar\"]}}")]
	[TestCase("{\"dependentSchemas\":{\"foo\":{}}}")]
	[TestCase("{\"deprecated\":true}")]
	[TestCase("{\"description\":\"some text\"}")]
	[TestCase("{\"else\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"enum\":[1,true,false,null,\"string\",[],{}]}")]
	[TestCase("{\"examples\":[1,true,false,null,\"string\",[],{}]}")]
	[TestCase("{\"exclusiveMaximum\":1}")]
	[TestCase("{\"exclusiveMinimum\":1}")]
	[TestCase("{\"format\":\"regex\"}")]
	[TestCase("{\"if\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"items\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"items\":[{\"$id\":\"http://some.site/schema\"}]}")]
	[TestCase("{\"maximum\":1}")]
	[TestCase("{\"maxItems\":1}")]
	[TestCase("{\"maxLength\":1}")]
	[TestCase("{\"maxProperties\":1}")]
	[TestCase("{\"minimum\":1}")]
	[TestCase("{\"minItems\":1}")]
	[TestCase("{\"minLength\":1}")]
	[TestCase("{\"minProperties\":1}")]
	[TestCase("{\"multipleOf\":1}")]
	[TestCase("{\"not\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"oneOf\":[{\"$id\":\"http://some.site/schema\"}]}")]
	[TestCase("{\"pattern\":\"^yes{1,3}$\"}")]
	[TestCase("{\"patternProperties\":{\"foo\":{}}}")]
	[TestCase("{\"prefixItems\":[{\"$id\":\"http://some.site/schema\"}]}")]
	[TestCase("{\"properties\":{\"foo\":{}}}")]
	[TestCase("{\"propertyNames\":{}}")]
	[TestCase("{\"readOnly\":true}")]
	[TestCase("{\"readOnly\":false}")]
	[TestCase("{\"required\":[\"foo\"]}")]
	[TestCase("{\"then\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"title\":\"some text\"}")]
	[TestCase("{\"type\":\"object\"}")]
	[TestCase("{\"type\":[\"object\",\"string\"]}")]
	[TestCase("{\"unevaluatedItems\":true}")]
	[TestCase("{\"unevaluatedItems\":false}")]
	[TestCase("{\"unevaluatedItems\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"unevaluatedProperties\":true}")]
	[TestCase("{\"unevaluatedProperties\":false}")]
	[TestCase("{\"unevaluatedProperties\":{\"$id\":\"http://some.site/schema\"}}")]
	[TestCase("{\"uniqueItems\":true}")]
	[TestCase("{\"writeOnly\":true}")]
	[TestCase("{\"writeOnly\":false}")]
	public void RoundTrip(string text)
	{
		var schema = JsonSchema.FromText(text);

		var returnToText = JsonSerializer.Serialize(schema, TestEnvironment.SerializerOptions);

		Assert.That(returnToText, Is.EqualTo(text));
	}

	[Test]
	public void DuplicateKeysThrow()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", true),
				("bar", true)
			)
			.Required("foo", "bar");

		var json = JsonNode.Parse("{\"foo\":1, \"bar\":2, \"foo\":false}");

		Assert.Throws(Is.InstanceOf<Exception>(), () => schema.Evaluate(json));
	}
}