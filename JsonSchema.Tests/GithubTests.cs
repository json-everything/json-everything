using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Json.More;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class GithubTests
	{
		[Test]
		public void Issue18_SomethingNotValidatingRight()
		{
			var instance = JsonDocument.Parse(@"{
    ""prop1"": {
        ""name"": ""a"",
        ""version"": 1
    },
    ""prop2"": {},
    ""prop4"": ""a"",
    ""prop5"": {},
    ""prop6"": {
        ""firstId"": ""428de96d-d5b2-4d12-8e88-37827099dd02"",
        ""secondId"": ""428de96d-d5b2-4d12-8e88-37827099dd02"",
        ""version"": ""test-version"",
        ""thirdId"": ""428de96d-d5b2-4d12-8e88-37827099dd02"",
        ""type"": ""test"",
        ""name"": ""testApp"",
        ""receiptTimestamp"": ""2019-02-05T12:36:31.2812022Z"",
        ""timestamp"": ""2012-04-21T12:36:31.2812022Z"",
        ""extra_key"": ""extra_val""
    },
    ""prop3"": {
        ""prop5"": {},
        ""metadata"": {},
        ""deleteAfter"": 3,
        ""allowExport"": true
    }
}");
			var schema = JsonSchema.FromText(@"{
	""$schema"": ""http://json-schema.org/draft-07/schema#"",
	""type"": ""object"",
	""required"": [""prop1"", ""prop2"", ""prop3"", ""prop4"", ""prop5"", ""prop6""],
	""properties"": {
	    ""prop1"": {
	        ""type"": ""object"",
	        ""required"": [""name"", ""version""],
	        ""additionalProperties"": false,
	        ""properties"": {
	            ""name"": {
	                ""type"": ""string"",
	                ""pattern"": ""^[-_]?([a-zA-Z][-_]?)+$""
	            },
	            ""version"": {
	                ""type"": ""integer"",
	                ""minimum"": 1
	            }
	        }
	    },
	    ""prop2"": {
	        ""$ref"": ""http://json-schema.org/draft-07/schema#""
	    },
	    ""prop3"": {
	        ""type"": ""object"",
	        ""required"": [
	        ""prop5"",
	        ""metadata""
	        ],
	        ""additionalProperties"": false,
	        ""properties"": {
	            ""prop5"": {
	                ""type"": ""object""
	            },
	            ""metadata"": {
	                ""type"": ""object""
	            },
	            ""deleteAfter"": {
	                ""type"": ""integer""
	            },
	            ""allowExport"": {
	                ""type"": ""boolean""
	            }
	        }
	    },
	    ""prop4"": {
	        ""type"": ""string"",
	        ""pattern"": ""^[-_]?([a-zA-Z][-_]?)+$""
	    },
	    ""prop5"": {
	        ""type"": ""object""
	    },
	    ""prop6"": {
	        ""type"": ""object"",
	        ""required"": [
	        ""firstId"",
	        ""secondId"",
	        ""version"",
	        ""thirdId"",
	        ""type"",
	        ""name"",
	        ""receiptTimestamp"",
	        ""timestamp""
	        ],
	        ""properties"": {
	            ""firstId"": {
	                ""type"": ""string"",
	                ""pattern"": ""[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}""
	            },
	            ""secondId"": {
	                ""type"": ""string"",
	                ""pattern"": ""[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}""
	            },
	            ""type"": {
	                ""type"": ""string"",
	                ""enum"": [""test"", ""lab"", ""stage"", ""prod""]
	            },
	            ""thirdId"": {
	                ""type"": ""string"",
	                ""pattern"": ""[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}""
	            },
	            ""version"": {
	                ""type"": ""string"",
	                ""minLength"": 1
	            },
	            ""name"": {
	                ""type"": ""string"",
	                ""minLength"": 1
	            },
	            ""receiptTimestamp"": {
	                ""type"": ""string"",
	                ""format"": ""date-time""
	            },
	            ""timestamp"": {
	                ""type"": ""string"",
	                ""format"": ""date-time""
	            }
	        },
	        ""additionalProperties"": {
	            ""type"": ""string""
	        }
	    }
	},
	""additionalProperties"": false
}");

			var result = schema.Validate(instance.RootElement, new ValidationOptions{OutputFormat = OutputFormat.Detailed});

			result.AssertValid();
		}

		[Test]
		public void Issue19_Draft4ShouldInvalidateAsUnrecognizedSchema_NoOption()
		{
			var schema = JsonSchema.FromText("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"type\":\"string\"}");
			var instance = JsonDocument.Parse("\"some string\"");

			var result = schema.Validate(instance.RootElement, new ValidationOptions{OutputFormat = OutputFormat.Detailed});

			result.AssertInvalid();
		}

		[Test]
		public void Issue19_Draft4ShouldInvalidateAsUnrecognizedSchema_WithOption()
		{
			var schema = JsonSchema.FromText("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"type\":\"string\"}");
			var instance = JsonDocument.Parse("\"some string\"");

			var result = schema.Validate(instance.RootElement, new ValidationOptions
			{
				OutputFormat = OutputFormat.Detailed,
				ValidateMetaSchema = true
			});

			result.AssertInvalid();
		}

		[TestCase(Draft.Draft7, @"{}", true)]
		[TestCase(Draft.Draft7, @"{""abc"": 1}", false)]
		[TestCase(Draft.Draft7, @"{""abc"": 1, ""d7"": 7}", true)]
		[TestCase(Draft.Draft7, @"{""abc"": 1, ""d9"": 9}", false)]
		[TestCase(Draft.Draft7, @"{""abc"": 1, ""d7"": 7, ""d9"": 9}", true)]
		[TestCase(Draft.Draft201909, @"{}", true)]
		[TestCase(Draft.Draft201909, @"{""abc"": 1}", false)]
		[TestCase(Draft.Draft201909, @"{""abc"": 1, ""d7"": 7}", false)]
		[TestCase(Draft.Draft201909, @"{""abc"": 1, ""d9"": 9}", true)]
		[TestCase(Draft.Draft201909, @"{""abc"": 1, ""d7"": 7, ""d9"": 9}", true)]
		public void Issue19_SchemaShouldOnlyUseSpecifiedDraftKeywords(Draft draft, string instance, bool isValid)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(@"
{
    ""dependencies"": {
        ""abc"": [ ""d7"" ]
    },
    ""dependentRequired"": {
        ""abc"": [ ""d9"" ]
    }
}");
			var opts = new ValidationOptions
			{
				ValidateAs = draft,
				OutputFormat = OutputFormat.Detailed
			};
			var element = JsonDocument.Parse(instance).RootElement;

			var val = schema.Validate(element, opts);
			Console.WriteLine("Elem `{0}` got validation `{1}`", instance, val.IsValid);
			if (isValid) val.AssertValid();
			else val.AssertInvalid();
		}

		[Test]
		public void Issue29_SchemaFromFileWithoutIdShouldInheritUriFromFilePath()
		{
			var schemaFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "issue29-schema-without-id.json")
				.AdjustForPlatform();

			var jsonStr = @"{
  ""abc"": {
    ""abc"": {
        ""abc"": ""abc""
    }
  }
}";
			var schema = JsonSchema.FromFile(schemaFile);
			var json = JsonDocument.Parse(jsonStr).RootElement;
			var validation = schema.Validate(json, new ValidationOptions { OutputFormat = OutputFormat.Detailed });

			validation.AssertValid();
		}

		[Test]
		public void Issue29_SchemaFromFileWithIdShouldKeepUriFromId()
		{
			var schemaFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "issue29-schema-with-id.json")
				.AdjustForPlatform();

			var jsonStr = @"{
  ""abc"": {
    ""abc"": {
        ""abc"": ""abc""
    }
  }
}";
			var schema = JsonSchema.FromFile(schemaFile);
			var json = JsonDocument.Parse(jsonStr).RootElement;
			var validation = schema.Validate(json, new ValidationOptions { OutputFormat = OutputFormat.Detailed });

			validation.AssertValid();
		}

		[Test]
		public void Issue29_SchemaWithOnlyFileNameIdShouldUseDefaultBaseUri()
		{
			var schemaStr = @"{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""$id"": ""mySchema.json"",
  ""properties"": {
      ""abc"": { ""$ref"": ""mySchema.json"" }
  },
  ""additionalProperties"": false
}";
			var jsonStr = @"{
  ""abc"": {
    ""abc"": {
        ""abc"": ""abc""
    }
  }
}";
			var schema = JsonSerializer.Deserialize<JsonSchema>(schemaStr);
			var json = JsonDocument.Parse(jsonStr).RootElement;
			var validation = schema.Validate(json, new ValidationOptions{OutputFormat = OutputFormat.Detailed});

			validation.AssertValid();
		}

		[Test]
		public void Issue76_GetHashCodeIsNotConsistent()
		{
			var schema1Str = @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""required"": [
    ""a"",
    ""b""
  ],
  ""properties"": {
    ""a"": { ""const"": ""a"" },
    ""b"": { ""const"": ""b"" }
  }
}";

			var schema2Str = @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""required"": [
    ""b"",
    ""a""
  ],
  ""properties"": {
    ""a"": { ""const"": ""a"" },
    ""b"": { ""const"": ""b"" }
  }
}";

			var schema3Str = @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""required"": [
    ""a"",
    ""b""
  ],
  ""properties"": {
    ""b"": { ""const"": ""b"" },
    ""a"": { ""const"": ""a"" }
  }
}";

			var schema4Str = @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""properties"": {
    ""a"": { ""const"": ""a"" },
    ""b"": { ""const"": ""b"" }
  },
  ""required"": [
    ""a"",
    ""b""
  ]
}";
			var schema1 = JsonSerializer.Deserialize<JsonSchema>(schema1Str);
			var schema2 = JsonSerializer.Deserialize<JsonSchema>(schema2Str);
			var schema3 = JsonSerializer.Deserialize<JsonSchema>(schema3Str);
			var schema4 = JsonSerializer.Deserialize<JsonSchema>(schema4Str);

			Assert.IsTrue(schema1.Equals(schema2));
			Assert.AreEqual(schema1.GetHashCode(), schema2.GetHashCode());
			Assert.IsTrue(schema1.Equals(schema3));
			Assert.AreEqual(schema1.GetHashCode(), schema3.GetHashCode());
			Assert.IsTrue(schema1.Equals(schema4));
			Assert.AreEqual(schema1.GetHashCode(), schema4.GetHashCode());
		}

		[Test]
		public void Issue76_GetHashCodeIsNotConsistent_WhitespaceInObjectValue()
		{
			var schema1Str = @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""required"": [
    ""a"",
    ""b""
  ],
  ""properties"": {
    ""a"": { ""const"": { ""left"": ""right"" } },
    ""b"": { ""const"": ""b"" }
  }
}";

			var schema2Str = @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""required"": [
    ""b"",
    ""a""
  ],
  ""properties"": {
    ""a"": { ""const"": {""left"":""right""} },
    ""b"": { ""const"": ""b"" }
  }
}";
			var schema1 = JsonSerializer.Deserialize<JsonSchema>(schema1Str);
			var schema2 = JsonSerializer.Deserialize<JsonSchema>(schema2Str);

			Assert.IsTrue(schema1.Equals(schema2));
			Assert.AreEqual(schema1.GetHashCode(), schema2.GetHashCode());
		}

		[Test]
		public void Issue79_RefsTryingToResolveParent()
		{
			var schema1Str = @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""$id"": ""schema1.json"",
  ""definitions"": {
    ""myDef"": {
      ""properties"": {
        ""abc"": { ""type"": ""string"" }
      }
    }
  },
  ""$ref"": ""#/definitions/myDef""
}";
			var schema2Str = @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""$id"": ""schema2.json"",
  ""$ref"": ""schema1.json""
}";
			var jsonStr = @"{ ""abc"": ""s"" }";
			var schema1 = JsonSerializer.Deserialize<JsonSchema>(schema1Str);
			var schema2 = JsonSerializer.Deserialize<JsonSchema>(schema2Str);
			var json = JsonDocument.Parse(jsonStr).RootElement;
			var uri1 = new Uri("http://first.com/schema1.json");
			var uri2 = new Uri("http://first.com/schema2.json");
			var firstBaseUri = new Uri("http://first.com");
			var map = new Dictionary<Uri, JsonSchema>
			{
				{ uri1, schema1 },
				{ uri2, schema2 },
			};
			var options = new ValidationOptions
			{
				OutputFormat = OutputFormat.Verbose,
				DefaultBaseUri = firstBaseUri,
				SchemaRegistry =
				{
					Fetch = uri =>
					{
						Assert.True(map.TryGetValue(uri, out var ret), "Unexpected uri: {0}", uri);
						return ret;
					}
				}
			};
            var result = schema2.Validate(json, options);
            result.AssertValid();
            Assert.AreEqual(result.NestedResults[0].NestedResults[0].AbsoluteSchemaLocation, "http://first.com/schema1.json#");
		}

		[Test]
		public void Issue79_RefsTryingToResolveParent_Explanation()
		{
			var schemaText = @"{
  ""$id"": ""https://mydomain.com/outer"",
  ""properties"": {
    ""foo"": {
      ""$id"": ""https://mydomain.com/foo"",
      ""properties"": {
        ""inner1"": {
          ""$anchor"": ""bar"",
          ""type"": ""string""
        },
        ""inner2"": {
          ""$ref"": ""#bar""
        }
      }
    },
    ""bar"": {
      ""$anchor"": ""bar"",
      ""type"": ""integer""
    }
  }
}";
			var passingText = @"
{
  ""foo"": {
    ""inner2"": ""value""
  }
}";
			var failingText = @"
{
  ""foo"": {
    ""inner2"": 42
  }
}";

			var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText);
			var passing = JsonDocument.Parse(passingText).RootElement;
			var failing = JsonDocument.Parse(failingText).RootElement;

			schema.Validate(passing, new ValidationOptions{OutputFormat = OutputFormat.Detailed}).AssertValid();
			schema.Validate(failing, new ValidationOptions{OutputFormat = OutputFormat.Detailed}).AssertInvalid();
		}

		[Test]
		public void Issue97_IdentifyCircularReferences()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Ref("#/$defs/string")
				.Defs(("string", new JsonSchemaBuilder().Ref("#/$defs/string")));

			var json = JsonDocument.Parse("\"value\"").RootElement;

			schema.Validate(json, new ValidationOptions{OutputFormat = OutputFormat.Detailed}).AssertInvalid();
		}

		[Test]
		public void Issue97_IdentifyComplexCircularReferences()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Ref("#/$defs/a")
				.Defs(
					("a", new JsonSchemaBuilder().Ref("#/$defs/b")),
					("b", new JsonSchemaBuilder().Ref("#/$defs/a"))
				);

			var json = JsonDocument.Parse("\"value\"").RootElement;

			schema.Validate(json, new ValidationOptions{OutputFormat = OutputFormat.Detailed}).AssertInvalid();
		}

		[SchemaKeyword(Name)]
		[SchemaDraft(Draft.Draft201909 | Draft.Draft202012)]
		private class MinDateKeyword : IJsonSchemaKeyword, IEquatable<MinDateKeyword>
		{
			// ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
			private const string Name = "minDate";
#pragma warning restore IDE1006 // Naming Styles

			public bool Equals(MinDateKeyword other)
			{
				throw new NotImplementedException();
			}

			public void Validate(ValidationContext context)
			{
				throw new NotImplementedException();
			}
		}

		private static string GetResource(int issue, string name)
		{
			var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", $"Issue{issue}_{name}.json")
				.AdjustForPlatform();

			return File.ReadAllText(path);
		}

		[Test]
		public void Issue191_SelfReferentialCustomMetaschemaShouldError()
		{
			var metaSchemaId = new Uri("https://myserver.net/meta-schema");

			var vocabId = "https://myserver.net/my-vocab";

			var metaSchema = JsonSchema.FromText(GetResource(191, "MetaSchema"));

			var schema = JsonSchema.FromText(GetResource(191, "Schema"));

			SchemaKeywordRegistry.Register<MinDateKeyword>();

			VocabularyRegistry.Global.Register(new Vocabulary(vocabId, typeof(MinDateKeyword)));
			SchemaRegistry.Global.Register(metaSchemaId, metaSchema);

			var result = JsonSerializer.Deserialize<JsonElement>(GetResource(191, "Data"));

			Assert.Throws<InvalidOperationException>(() => schema.Validate(result));
		}

		[Test]
		public void Issue208_BundlingNotWorking()
		{
			JsonSchema externalSchema = new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://my-external-schema")
				.Type(SchemaValueType.Object)
				.Properties(
					("first", new JsonSchemaBuilder().Type(SchemaValueType.String))
				);

			var options = new ValidationOptions
			{
				OutputFormat = OutputFormat.Detailed
			};
			options.SchemaRegistry.Register(new Uri("https://my-external-schema"), externalSchema);

			JsonSchema mySchema = new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://my-schema")
				.Type(SchemaValueType.Object)
				.Properties(
					("first", new JsonSchemaBuilder().Ref("https://my-external-schema")),
					("second", new JsonSchemaBuilder()
						.Schema(MetaSchemas.Draft202012Id)
						.Id("https://my-inner-schema")
						.Type(SchemaValueType.Object)
						.Properties(
							("second", new JsonSchemaBuilder().Ref("#/$defs/my-inner-ref"))
						)
						.Defs(
							("my-inner-ref", new JsonSchemaBuilder().Type(SchemaValueType.String))
						)
					)
				);

			var instance = JsonDocument.Parse("{\"first\":{\"first\":\"first\"},\"second\":{\"second\":\"second\"}}").RootElement;

			mySchema.Validate(instance, options).AssertValid();

		}

		[Test]
		public void Issue212_CouldNotResolveAnchorReference_FromFile()
		{
			// This validation fails because the file uses `id` instead of `$id`.
			// See https://github.com/gregsdennis/json-everything/issues/212#issuecomment-1033423550
			var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "issue212_schema.json")
				.AdjustForPlatform();
			var schema = JsonSchema.FromFile(path);

			var instance = JsonDocument.Parse("{\"ContentDefinitionId\": \"fa81bc1d-3efe-4192-9e03-31e9898fef90\"}").RootElement;

			var res = schema.Validate(instance, new ValidationOptions
			{
				OutputFormat = OutputFormat.Detailed,
				//ValidateAs = Draft.Draft7,
				ValidateMetaSchema = true
			});

			res.AssertInvalid();
		}

		[Test]
		public void Issue212_CouldNotResolveAnchorReference_Inline()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft7Id)
				.Id("http://messagebroker.fff.pl/rpn/dci/kkt.json")
				.Type(SchemaValueType.Object)
				.Title("JSON Schema ")
				.Definitions(
					("#guid", new JsonSchemaBuilder()
						.Id("#guid")
						.Title("Definicja obligatoryjnego GUID (regex)")
						.Type(SchemaValueType.String)
						.Examples(
							"09C9A8DA-B40F-4E3A-9746-7B10AFEC2C4F",
							"2d9bbb00-878f-49b0-9d48-767ce3e12dee"
						)
						.MinLength(36)
						.MaxLength(36)
						.Pattern("^([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$")
					)
				)
				.Required("ContentDefinitionId")
				.Properties(
					("ContentDefinitionId", new JsonSchemaBuilder()
						.Ref("#guid")
						.Title("Identyfikator definicji ")
					)
				)
				.AdditionalProperties(false);

			var instance = JsonDocument.Parse("{\"ContentDefinitionId\": \"fa81bc1d-3efe-4192-9e03-31e9898fef90\"}").RootElement;

			var res = schema.Validate(instance, new ValidationOptions
			{
				OutputFormat = OutputFormat.Detailed,
				//ValidateAs = Draft.Draft7,
				ValidateMetaSchema = true
			});
			res.AssertValid();
		}

		[Test]
		public void Issue216_AdditionalPropertiesShouldRelyOnDeclarationsForDraft7()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft7Id)
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
					("bar", new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
				.AdditionalProperties(false);

			var instance = JsonDocument.Parse("{\"foo\":1,\"bar\":false}").RootElement;

			var result = schema.Validate(instance, new ValidationOptions{OutputFormat = OutputFormat.Detailed});

			result.AssertInvalid();
			var nodes = new List<ValidationResults>{result};
			while (nodes.Any())
			{
				var node = nodes.First();
				nodes.Remove(node);
				Assert.AreNotEqual("#/additionalProperties", node.SchemaLocation.ToString());
				nodes.AddRange(node.NestedResults);
			}
		}

		[Test]
		public void Issue220_DeeplyNestedReferences()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft201909Id)
				.Ref("#/$defs/test/items/properties/test")
				.Defs(
					("test", new JsonSchemaBuilder()
						.Items(new JsonSchemaBuilder()
							.Properties(
								("test", new JsonSchemaBuilder().Type(SchemaValueType.String))
							)
						)
					)
				);

			var result = schema.Validate(3.AsJsonElement(), new ValidationOptions {OutputFormat = OutputFormat.Detailed});

			result.AssertInvalid();
			Assert.IsFalse(JsonSerializer.Serialize(result).Contains("Could not resolve reference"));
		}

		[Test]
		public void Issue226_MessageInValidResult()
		{
			var schemaText = GetResource(226, "schema");
			var instanceText = GetResource(226, "instance");

			var schema = JsonSchema.FromText(schemaText);
			var instance = JsonDocument.Parse(instanceText).RootElement;

			var result = schema.Validate(instance, new ValidationOptions{OutputFormat = OutputFormat.Basic});

			Assert.AreEqual(0, result.NestedResults.Count);
		}
	}
}
