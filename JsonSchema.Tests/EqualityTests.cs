using System.Collections.Generic;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class EqualityTests
	{
		public static IEnumerable<TestCaseData> EqualCases
		{
			get
			{
				yield return new TestCaseData("{}", "{}");
				yield return new TestCaseData("true", "true");
				yield return new TestCaseData("false", "false");
				yield return new TestCaseData("{\"$id\":\"http://me.com/schema\"}", "{\"$id\":\"http://me.com/schema\"}");
				yield return new TestCaseData("{\"$schema\":\"http://me.com/schema\"}", "{\"$schema\":\"http://me.com/schema\"}");
				yield return new TestCaseData("{\"additionalItems\":true}", "{\"additionalItems\":true}");
				yield return new TestCaseData("{\"additionalProperties\":true}", "{\"additionalProperties\":true}");
				yield return new TestCaseData("{\"$anchor\":\"schema\"}", "{\"$anchor\":\"schema\"}");
				yield return new TestCaseData("{\"allOf\":[false, true]}", "{\"allOf\":[true, false]}");
				yield return new TestCaseData("{\"anyOf\":[false, true]}", "{\"anyOf\":[true, false]}");
				yield return new TestCaseData("{\"comment\":\"schema\"}", "{\"comment\":\"schema\"}");
				yield return new TestCaseData("{\"const\":\"schema\"}", "{\"const\":\"schema\"}");
				yield return new TestCaseData("{\"contains\":true}", "{\"contains\":true}");
				yield return new TestCaseData("{\"contentMediaEncoding\":\"base64\"}", "{\"contentMediaEncoding\":\"base64\"}");
				yield return new TestCaseData("{\"contentMediaType\":\"application/json\"}", "{\"contentMediaType\":\"application/json\"}");
				yield return new TestCaseData("{\"contentSchema\":true}", "{\"contentSchema\":true}");
				yield return new TestCaseData("{\"default\":1}", "{\"default\":1}");
				yield return new TestCaseData("{\"definitions\":{\"def1\":true}}", "{\"definitions\":{\"def1\":true}}");
				yield return new TestCaseData("{\"$defs\":{\"def1\":true}}", "{\"$defs\":{\"def1\":true}}");
				yield return new TestCaseData("{\"dependencies\":{\"def1\":true}}", "{\"dependencies\":{\"def1\":true}}");
				yield return new TestCaseData("{\"dependencies\":{\"def1\":[\"prop1\"]}}", "{\"dependencies\":{\"def1\":[\"prop1\"]}}");
				yield return new TestCaseData("{\"dependentRequired\":{\"def1\":[\"prop1\"]}}", "{\"dependentRequired\":{\"def1\":[\"prop1\"]}}");
				yield return new TestCaseData("{\"dependentSchemas\":{\"def1\":true}}", "{\"dependentSchemas\":{\"def1\":true}}");
				yield return new TestCaseData("{\"deprecated\":true}", "{\"deprecated\":true}");
				yield return new TestCaseData("{\"description\":\"schema\"}", "{\"description\":\"schema\"}");
				yield return new TestCaseData("{\"else\":true}", "{\"else\":true}");
				yield return new TestCaseData("{\"enum\":[true,\"string\",5]}", "{\"enum\":[5,true,\"string\"]}");
				yield return new TestCaseData("{\"examples\":[true,\"string\",5]}", "{\"examples\":[5,true,\"string\"]}");
				yield return new TestCaseData("{\"exclusiveMaximum\":1}", "{\"exclusiveMaximum\":1}");
				yield return new TestCaseData("{\"exclusiveMinimum\":1}", "{\"exclusiveMinimum\":1}");
				yield return new TestCaseData("{\"format\":\"regex\"}", "{\"format\":\"regex\"}");
				yield return new TestCaseData("{\"if\":true}", "{\"if\":true}");
				yield return new TestCaseData("{\"items\":[false, true]}", "{\"items\":[true, false]}");
				yield return new TestCaseData("{\"maxContains\":1}", "{\"maxContains\":1}");
				yield return new TestCaseData("{\"maximum\":1}", "{\"maximum\":1}");
				yield return new TestCaseData("{\"maxItems\":1}", "{\"maxItems\":1}");
				yield return new TestCaseData("{\"maxLength\":1}", "{\"maxLength\":1}");
				yield return new TestCaseData("{\"maxProperties\":1}", "{\"maxProperties\":1}");
				yield return new TestCaseData("{\"minContains\":1}", "{\"minContains\":1}");
				yield return new TestCaseData("{\"minimum\":1}", "{\"minimum\":1}");
				yield return new TestCaseData("{\"minItems\":1}", "{\"minItems\":1}");
				yield return new TestCaseData("{\"minLength\":1}", "{\"minLength\":1}");
				yield return new TestCaseData("{\"minProperties\":1}", "{\"minProperties\":1}");
				yield return new TestCaseData("{\"multipleOf\":1}", "{\"multipleOf\":1}");
				yield return new TestCaseData("{\"not\":true}", "{\"not\":true}");
				yield return new TestCaseData("{\"oneOf\":[false, true]}", "{\"oneOf\":[true, false]}");
				yield return new TestCaseData("{\"pattern\":\"^.*$\"}", "{\"pattern\":\"^.*$\"}");
				yield return new TestCaseData("{\"patternProperties\":{\"^.*$\":true}}", "{\"patternProperties\":{\"^.*$\":true}}");
				yield return new TestCaseData("{\"properties\":{\"prop1\":true}}", "{\"properties\":{\"prop1\":true}}");
				yield return new TestCaseData("{\"propertyNames\":true}", "{\"propertyNames\":true}");
				yield return new TestCaseData("{\"readOnly\":true}", "{\"readOnly\":true}");
				yield return new TestCaseData("{\"$recursiveRef\":\"http://me.com/schema\"}", "{\"$recursiveRef\":\"http://me.com/schema\"}");
				yield return new TestCaseData("{\"$ref\":\"http://me.com/schema\"}", "{\"$ref\":\"http://me.com/schema\"}");
				yield return new TestCaseData("{\"required\":[\"prop1\"]}", "{\"required\":[\"prop1\"]}");
				yield return new TestCaseData("{\"then\":true}", "{\"then\":true}");
				yield return new TestCaseData("{\"title\":\"schema\"}", "{\"title\":\"schema\"}");
				yield return new TestCaseData("{\"type\":\"object\"}", "{\"type\":\"object\"}");
				yield return new TestCaseData("{\"type\":[\"string\",\"object\"]}", "{\"type\":[\"string\",\"object\"]}");
				yield return new TestCaseData("{\"unevaluatedItems\":true}", "{\"unevaluatedItems\":true}");
				yield return new TestCaseData("{\"unevaluatedProperties\":true}", "{\"unevaluatedProperties\":true}");
				yield return new TestCaseData("{\"uniqueItems\":true}", "{\"uniqueItems\":true}");
				yield return new TestCaseData("{\"vocabulary\":{\"http://me.com/vocab\":true}}", "{\"vocabulary\":{\"http://me.com/vocab\":true}}");
				yield return new TestCaseData("{\"writeOnly\":true}", "{\"writeOnly\":true}");
			}
		}

		[TestCaseSource(nameof(EqualCases))]
		public void AreEqual(string a, string b)
		{
			var aSchema = JsonSchema.FromText(a);
			var bSchema = JsonSchema.FromText(b);

			Assert.AreEqual(aSchema, bSchema);
		}

		public static IEnumerable<TestCaseData> NotEqualCases
		{
			get
			{
				yield return new TestCaseData("{}", "true");
				yield return new TestCaseData("{}", "false");
				yield return new TestCaseData("true", "false");
				yield return new TestCaseData("{\"$id\":\"http://me.com/schema\"}", "{\"comment\":\"http://me.com/schema\"}");
				yield return new TestCaseData("{\"$id\":\"http://me.com/schema\"}", "{\"$id\":\"http://me.com/schema\",\"otherData\":5}");
				yield return new TestCaseData("{\"$id\":\"http://me.com/schema\"}", "{\"$id\":\"http://you.com/schema\"}");
				yield return new TestCaseData("{\"$schema\":\"http://me.com/schema\"}", "{\"$schema\":\"http://you.com/schema\"}");
				yield return new TestCaseData("{\"additionalItems\":true}", "{\"additionalItems\":false}");
				yield return new TestCaseData("{\"additionalProperties\":true}", "{\"additionalProperties\":false}");
				yield return new TestCaseData("{\"allOf\":[false, true]}", "{\"allOf\":[true, false, {}]}");
				yield return new TestCaseData("{\"$anchor\":\"schema\"}", "{\"$anchor\":\"other\"}");
				yield return new TestCaseData("{\"anyOf\":[false, true]}", "{\"anyOf\":[true, false, {}]}");
				yield return new TestCaseData("{\"comment\":\"schema\"}", "{\"comment\":\"other\"}");
				yield return new TestCaseData("{\"const\":\"schema\"}", "{\"const\":\"other\"}");
				yield return new TestCaseData("{\"contains\":true}", "{\"contains\":false}");
				yield return new TestCaseData("{\"contentMediaEncoding\":\"base64\"}", "{\"contentMediaEncoding\":\"raw\"}");
				yield return new TestCaseData("{\"contentMediaType\":\"application/json\"}", "{\"contentMediaType\":\"application/txt\"}");
				yield return new TestCaseData("{\"contentSchema\":true}", "{\"contentSchema\":false}");
				yield return new TestCaseData("{\"default\":1}", "{\"default\":null}");
				yield return new TestCaseData("{\"definitions\":{\"def1\":true}}", "{\"definitions\":{\"def1\":false}}");
				yield return new TestCaseData("{\"definitions\":{\"def1\":true}}", "{\"definitions\":{\"def2\":true}}");
				yield return new TestCaseData("{\"definitions\":{\"def1\":true}}", "{\"definitions\":{\"def1\":true,\"def2\":true}}");
				yield return new TestCaseData("{\"$defs\":{\"def1\":true}}", "{\"$defs\":{\"def1\":false}}");
				yield return new TestCaseData("{\"$defs\":{\"def1\":true}}", "{\"$defs\":{\"def2\":true}}");
				yield return new TestCaseData("{\"$defs\":{\"def1\":true}}", "{\"$defs\":{\"def1\":true,\"def2\":true}}");
				yield return new TestCaseData("{\"dependencies\":{\"def1\":true}}", "{\"dependencies\":{\"def1\":false}}");
				yield return new TestCaseData("{\"dependencies\":{\"def1\":true}}", "{\"dependencies\":{\"def2\":false}}");
				yield return new TestCaseData("{\"dependencies\":{\"def1\":true}}", "{\"dependencies\":{\"def1\":[\"prop1\"]}}");
				yield return new TestCaseData("{\"dependencies\":{\"def1\":[\"prop1\"]}}", "{\"dependencies\":{\"def1\":[\"prop2\"]}}");
				yield return new TestCaseData("{\"dependencies\":{\"def1\":[\"prop1\"]}}", "{\"dependencies\":{\"def2\":[\"prop1\"]}}");
				yield return new TestCaseData("{\"dependentRequired\":{\"def1\":[\"prop1\"]}}", "{\"dependentRequired\":{\"def1\":[\"prop2\"]}}");
				yield return new TestCaseData("{\"dependentRequired\":{\"def1\":[\"prop1\"]}}", "{\"dependentRequired\":{\"def2\":[\"prop1\"]}}");
				yield return new TestCaseData("{\"dependentSchemas\":{\"def1\":true}}", "{\"dependentSchemas\":{\"def1\":false}}");
				yield return new TestCaseData("{\"dependentSchemas\":{\"def1\":true}}", "{\"dependentSchemas\":{\"def2\":true}}");
				yield return new TestCaseData("{\"deprecated\":true}", "{\"deprecated\":false}");
				yield return new TestCaseData("{\"description\":\"schema\"}", "{\"description\":\"other\"}");
				yield return new TestCaseData("{\"else\":true}", "{\"else\":false}");
				yield return new TestCaseData("{\"enum\":[true,\"string\",5]}", "{\"enum\":[6,true,\"string\"]}");
				yield return new TestCaseData("{\"enum\":[true,\"string\",5]}", "{\"enum\":[5,true,\"string\",\"5\"]}");
				yield return new TestCaseData("{\"examples\":[true,\"string\",5]}", "{\"examples\":[5,false,\"string\"]}");
				yield return new TestCaseData("{\"examples\":[true,\"string\",5]}", "{\"examples\":[5,true,\"string\",true]}");
				yield return new TestCaseData("{\"exclusiveMaximum\":1}", "{\"exclusiveMaximum\":2}");
				yield return new TestCaseData("{\"exclusiveMinimum\":1}", "{\"exclusiveMinimum\":2}");
				yield return new TestCaseData("{\"format\":\"regex\"}", "{\"format\":\"json-pointer\"}");
				yield return new TestCaseData("{\"if\":true}", "{\"if\":false}");
				yield return new TestCaseData("{\"items\":[false, true]}", "{\"items\":[true, false, {}]}");
				yield return new TestCaseData("{\"maxContains\":1}", "{\"maxContains\":2}");
				yield return new TestCaseData("{\"maximum\":1}", "{\"maximum\":2}");
				yield return new TestCaseData("{\"maxItems\":1}", "{\"maxItems\":2}");
				yield return new TestCaseData("{\"maxLength\":1}", "{\"maxLength\":2}");
				yield return new TestCaseData("{\"maxProperties\":1}", "{\"maxProperties\":2}");
				yield return new TestCaseData("{\"minContains\":1}", "{\"minContains\":2}");
				yield return new TestCaseData("{\"minimum\":1}", "{\"minimum\":2}");
				yield return new TestCaseData("{\"minItems\":1}", "{\"minItems\":2}");
				yield return new TestCaseData("{\"minLength\":1}", "{\"minLength\":2}");
				yield return new TestCaseData("{\"minProperties\":1}", "{\"minProperties\":2}");
				yield return new TestCaseData("{\"multipleOf\":1}", "{\"multipleOf\":2}");
				yield return new TestCaseData("{\"not\":true}", "{\"not\":false}");
				yield return new TestCaseData("{\"oneOf\":[false, true]}", "{\"oneOf\":[true, false, {}]}");
				yield return new TestCaseData("{\"pattern\":\"^.*$\"}", "{\"pattern\":\"^.+$\"}");
				yield return new TestCaseData("{\"patternProperties\":{\"^.*$\":true}}", "{\"patternProperties\":{\"^.+$\":true}}");
				yield return new TestCaseData("{\"patternProperties\":{\"^.*$\":true}}", "{\"patternProperties\":{\"^.*$\":false}}");
				yield return new TestCaseData("{\"properties\":{\"prop1\":true}}", "{\"properties\":{\"prop2\":true}}");
				yield return new TestCaseData("{\"properties\":{\"prop1\":true}}", "{\"properties\":{\"prop1\":false}}");
				yield return new TestCaseData("{\"propertyNames\":true}", "{\"propertyNames\":false}");
				yield return new TestCaseData("{\"readOnly\":true}", "{\"readOnly\":false}");
				yield return new TestCaseData("{\"$recursiveRef\":\"http://me.com/schema\"}", "{\"$recursiveRef\":\"http://you.com/schema\"}");
				yield return new TestCaseData("{\"$ref\":\"http://me.com/schema\"}", "{\"$ref\":\"http://you.com/schema\"}");
				yield return new TestCaseData("{\"required\":[\"prop1\"]}", "{\"required\":[\"prop2\"]}");
				yield return new TestCaseData("{\"then\":true}", "{\"then\":false}");
				yield return new TestCaseData("{\"title\":\"schema\"}", "{\"title\":\"other\"}");
				yield return new TestCaseData("{\"type\":\"object\"}", "{\"type\":\"string\"}");
				yield return new TestCaseData("{\"type\":[\"array\",\"object\"]}", "{\"type\":[\"string\",\"object\"]}");
				yield return new TestCaseData("{\"type\":\"object\"}", "{\"type\":[\"string\",\"object\"]}");
				yield return new TestCaseData("{\"unevaluatedItems\":true}", "{\"unevaluatedItems\":false}");
				yield return new TestCaseData("{\"unevaluatedProperties\":true}", "{\"unevaluatedProperties\":false}");
				yield return new TestCaseData("{\"uniqueItems\":true}", "{\"uniqueItems\":false}");
				yield return new TestCaseData("{\"vocabulary\":{\"http://me.com/vocab\":true}}", "{\"vocabulary\":{\"http://me.com/vocab\":false}}");
				yield return new TestCaseData("{\"vocabulary\":{\"http://me.com/vocab\":true}}", "{\"vocabulary\":{\"http://you.com/vocab\":true}}");
				yield return new TestCaseData("{\"writeOnly\":true}", "{\"writeOnly\":false}");
			}
		}

		[TestCaseSource(nameof(NotEqualCases))]
		public void AreNotEqual(string a, string b)
		{
			var aSchema = JsonSchema.FromText(a);
			var bSchema = JsonSchema.FromText(b);

			Assert.AreNotEqual(aSchema, bSchema);
		}
	}
}