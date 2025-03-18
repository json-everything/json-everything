using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.More;
using Json.Pointer;
using Json.Schema.Serialization;
using NUnit.Framework;
using TestHelpers;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Json.Schema.Tests;

public class GithubTests
{
	private static string GetFile(int issue, string name)
	{
		return Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", $"Issue{issue}_{name}.json")
			.AdjustForPlatform();
	}

	private static string GetResource(int issue, string name)
	{
		return File.ReadAllText(GetFile(issue, name));
	}

	[Test]
	public void Issue18_SomethingNotValidatingRight()
	{
		var instance = JsonNode.Parse(@"{
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

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
	}

	[Test]
	public void Issue19_Draft4ShouldInvalidateAsUnrecognizedSchema_NoOption()
	{
		var schema = JsonSchema.FromText("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"type\":\"string\"}");
		var instance = JsonNode.Parse("\"some string\"");

		Assert.Throws<RefResolutionException>(() => schema.Evaluate(instance));
	}

	[Test]
	public void Issue19_Draft4ShouldInvalidateAsUnrecognizedSchema_WithOption()
	{
		var schema = JsonSchema.FromText("{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"type\":\"string\"}");
		var instance = JsonNode.Parse("\"some string\"");

		Assert.Throws<RefResolutionException>(() => schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		}));
	}

	[TestCase(SpecVersion.Draft7, @"{}", true)]
	[TestCase(SpecVersion.Draft7, @"{""abc"": 1}", false)]
	[TestCase(SpecVersion.Draft7, @"{""abc"": 1, ""d7"": 7}", true)]
	[TestCase(SpecVersion.Draft7, @"{""abc"": 1, ""d9"": 9}", false)]
	[TestCase(SpecVersion.Draft7, @"{""abc"": 1, ""d7"": 7, ""d9"": 9}", true)]
	[TestCase(SpecVersion.Draft201909, @"{}", true)]
	[TestCase(SpecVersion.Draft201909, @"{""abc"": 1}", false)]
	[TestCase(SpecVersion.Draft201909, @"{""abc"": 1, ""d7"": 7}", false)]
	[TestCase(SpecVersion.Draft201909, @"{""abc"": 1, ""d9"": 9}", true)]
	[TestCase(SpecVersion.Draft201909, @"{""abc"": 1, ""d7"": 7, ""d9"": 9}", true)]
	public void Issue19_SchemaShouldOnlyUseSpecifiedDraftKeywords(SpecVersion version, string instance, bool isValid)
	{
		var schema = JsonSerializer.Deserialize<JsonSchema>(@"
{
    ""dependencies"": {
        ""abc"": [ ""d7"" ]
    },
    ""dependentRequired"": {
        ""abc"": [ ""d9"" ]
    }
}", TestEnvironment.SerializerOptions)!;
		var opts = new EvaluationOptions
		{
			EvaluateAs = version,
			OutputFormat = OutputFormat.Hierarchical
		};
		var element = JsonNode.Parse(instance);

		var val = schema.Evaluate(element, opts);
		TestConsole.WriteLine("Elem `{0}` got validation `{1}`", instance, val.IsValid);
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
		var json = JsonNode.Parse(jsonStr);
		var validation = schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

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
		var json = JsonNode.Parse(jsonStr);
		var validation = schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

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
		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaStr, TestEnvironment.SerializerOptions)!;
		var json = JsonNode.Parse(jsonStr);
		var validation = schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		validation.AssertValid();
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
		var schema1 = JsonSerializer.Deserialize<JsonSchema>(schema1Str, TestEnvironment.SerializerOptions)!;
		var schema2 = JsonSerializer.Deserialize<JsonSchema>(schema2Str, TestEnvironment.SerializerOptions)!;
		var json = JsonNode.Parse(jsonStr);
		var uri1 = new Uri("https://json-everything.net/schema1.json");
		var uri2 = new Uri("https://json-everything.net/schema2.json");
		var map = new Dictionary<Uri, JsonSchema>
		{
			{ uri1, schema1 },
			{ uri2, schema2 },
		};
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			SchemaRegistry =
			{
				Fetch = uri =>
				{
					Assert.That(map.TryGetValue(uri, out var ret), Is.True, $"Unexpected uri: {uri}");
					return ret;
				}
			}
		};
		var result = schema2.Evaluate(json, options);
		result.AssertValid();
		Assert.That(result.Details[0].SchemaLocation.OriginalString, Is.EqualTo("https://json-everything.net/schema1.json"));
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

		var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText, TestEnvironment.SerializerOptions);
		var passing = JsonNode.Parse(passingText);
		var failing = JsonNode.Parse(failingText);

		schema!.Evaluate(passing, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical }).AssertValid();
		schema.Evaluate(failing, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical }).AssertInvalid();
	}

	[Test]
	public void Issue97_IdentifyCircularReferences()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Ref("#/$defs/string")
			.Defs(("string", new JsonSchemaBuilder().Ref("#/$defs/string")));

		var json = JsonNode.Parse("\"value\"");

		Assert.Throws<JsonSchemaException>(() => schema.Evaluate(json));
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

		var json = JsonNode.Parse("\"value\"");

		Assert.Throws<JsonSchemaException>(() => schema.Evaluate(json));
	}

	[SchemaKeyword(Name)]
	[SchemaSpecVersion(SpecVersion.Draft201909 | SpecVersion.Draft202012)]
	private class MinDateKeyword : IJsonSchemaKeyword
	{
		// ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
		private const string Name = "minDate";
#pragma warning restore IDE1006 // Naming Styles

		public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
			ReadOnlySpan<KeywordConstraint> localConstraints,
			EvaluationContext context)
		{
			throw new NotImplementedException();
		}
	}

	[Test]
	public void Issue191_SelfReferentialCustomMetaschemaShouldError()
	{
		var vocabId = "https://myserver.net/my-vocab";
		var vocab = new Vocabulary(vocabId, typeof(MinDateKeyword));
		try
		{
			var metaSchemaId = new Uri("https://myserver.net/meta-schema");
			var metaSchema = JsonSchema.FromText(GetResource(191, "MetaSchema"));

			SchemaKeywordRegistry.Register<MinDateKeyword>();

			VocabularyRegistry.Register(vocab);

			Assert.Throws<JsonSchemaException>(() => SchemaRegistry.Global.Register(metaSchemaId, metaSchema));
		}
		finally
		{
			VocabularyRegistry.Unregister(vocab);
		}
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

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
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

		var instance = JsonNode.Parse("{\"first\":{\"first\":\"first\"},\"second\":{\"second\":\"second\"}}");

		mySchema.Evaluate(instance, options).AssertValid();

	}

	[Test]
	public void Issue212_CouldNotResolveAnchorReference_FromFile()
	{
		// This validation fails because the file uses `id` instead of `$id`.
		// See https://github.com/json-everything/json-everything/issues/212#issuecomment-1033423550
		var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "issue212_schema.json")
			.AdjustForPlatform();
		var schema = JsonSchema.FromFile(path);

		var instance = JsonNode.Parse("{\"ContentDefinitionId\": \"fa81bc1d-3efe-4192-9e03-31e9898fef90\"}");

		Assert.Throws<RefResolutionException>(() => schema.Evaluate(instance, new EvaluationOptions
		{
			ValidateAgainstMetaSchema = true
		}));
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

		var instance = JsonNode.Parse("{\"ContentDefinitionId\": \"fa81bc1d-3efe-4192-9e03-31e9898fef90\"}");

		var res = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			ValidateAgainstMetaSchema = true
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

		var instance = JsonNode.Parse("{\"foo\":1,\"bar\":false}");

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertInvalid();
		var nodes = new List<EvaluationResults> { result };
		while (nodes.Count != 0)
		{
			var node = nodes.First();
			nodes.Remove(node);
			Assert.That(node.EvaluationPath.ToString(), Is.Not.EqualTo("#/additionalProperties"));
			nodes.AddRange(node.Details);
		}
	}

	[Test]
	public void Issue226_MessageInValidResult()
	{
		var schemaText = GetResource(226, "schema");
		var instanceText = GetResource(226, "instance");

		var schema = JsonSchema.FromText(schemaText);
		var instance = JsonNode.Parse(instanceText);

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		result.AssertValid();
	}

	[Test]
	public void Issue352_ConcurrentValidationsWithReferences()
	{
		var schema = JsonSchema.FromText(@"{
            ""$schema"": ""http://json-schema.org/draft-07/schema#"",
            ""type"": ""object"",
            ""properties"": {
                ""id"": {
                    ""type"": ""integer""
                },        
                ""interval1"": {
                    ""$ref"": ""#/components/schemas/interval""
                }
            },
            ""components"": {
                ""schemas"": {
                    ""interval"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""from"": {
                                ""type"": ""number""
                            },
                            ""to"": {
                                ""type"": ""number""
                            }
                        }
                    }
                }
            }
            }");

		var instance = new JsonObject
		{
			["id"] = 123,
			["interval1"] = new JsonObject
			{
				["to"] = 3.0
			}
		};

		// verify it runs once
		var result = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.List,
			RequireFormatValidation = true
		});
		result.AssertValid();

		// run in parallel
		var numberOfMessages = 100;
		var jsonMessages = new List<JsonNode?>();
		for (int j = 0; j < numberOfMessages; j++)
		{
			jsonMessages.Add(instance?.DeepClone());
		}
		Parallel.ForEach(jsonMessages, json =>
		{
			EvaluationResults r;
			try
			{
				r = schema.Evaluate(json, new EvaluationOptions
				{
					OutputFormat = OutputFormat.List,
					RequireFormatValidation = true
				});
			}
			catch (Exception e)
			{
				TestConsole.WriteLine(e);
				throw;
			}

			r.AssertValid();
		});
	}

	[Test]
	public void Issue417_BundleWithItemsFails()
	{
		// items: true
		var singleItemSchema = new JsonSchemaBuilder()
			.Items(true)
			.Build();
		singleItemSchema.Bundle(); // throws
		// items: [true, true]
		var multiItemSchema = new JsonSchemaBuilder()
			.Items([true, true])
			.Build();
		multiItemSchema.Bundle(); // throws
	}

	[Test]
	public void Issue419_SecondLevelReferences()
	{
		var level2 = JsonSchema.FromFile(GetFile(419, "level2"));
		SchemaRegistry.Global.Register(level2);

		var level1 = JsonSchema.FromFile(GetFile(419, "level1"));
		SchemaRegistry.Global.Register(level1);

		var rootSchema = JsonSchema.FromFile(GetFile(419, "root"));

		var config = JsonNode.Parse(GetResource(419, "config"));

		var result = rootSchema.Evaluate(config);

		result.AssertValid();
	}

	[Test]
	public void Issue426_FileFragmentRefs()
	{
		var schema = JsonSchema.FromFile(GetFile(426, "schema"));
		SchemaRegistry.Global.Register(schema);
		
		var data = JsonNode.Parse(File.ReadAllText(GetFile(426, "data")));
		
		var result = schema.Evaluate(data);

		result.AssertValid();
	}

	[Test]
	public void Issue432_UnresolvedLocalRef()
	{
		var schema = JsonSchema.FromText(@"{
  ""$schema"": ""http://json-schema.org/draft-06/schema#"",
  ""$ref"": ""#/definitions/Order"",
  ""definitions"": {
    ""Order"": {
      ""type"": ""object"",
      ""additionalProperties"": true,
      ""properties"": {
        ""orderId"": {
          ""type"": ""string"",
          ""format"": ""uuid""
        }
      },
      ""required"": [
        ""orderId""
      ],
      ""title"": ""Order""
    }
  }
}");
		var instance = JsonNode.Parse("{ \"orderId\": \"3cb65f2d-4049-43c1-b185-1943765acd9\" }");

		var result = schema.Evaluate(instance);
		
		result.AssertValid();
	}

	[Test]
	public void Issue432_UnresolvedLocalRef_Again()
	{
		var schema = JsonSchema.FromText(@"{
  ""$schema"": ""http://json-schema.org/draft-06/schema#"",
  ""$ref"": ""#/definitions/Order"",
  ""definitions"": {
    ""Order"": {
      ""type"": ""object"",
      ""additionalProperties"": true,
      ""properties"": {
        ""orderId"": {
          ""type"": ""string"",
          ""format"": ""uuid""
        },
        ""customer"": {
          ""$ref"": ""#/definitions/Customer""
        }
      },
      ""required"": [
        ""customer"",
        ""orderId""
      ],
      ""title"": ""Order""
    },
    ""Customer"": {
      ""type"": ""object"",
      ""additionalProperties"": false,
      ""properties"": {
        ""name"": {
          ""type"": ""string""
        }
      },
      ""required"": [
        ""name""
      ],
      ""title"": ""Customer""
    }
  }
}");
		var instance = JsonNode.Parse(@"{
  ""orderId"": ""3cb65f2d-4049-4c1-b185-194365acdf99"",
  ""customer"": {
    ""name"": ""Some Customer""
  }
}");

		var result = schema.Evaluate(instance);
		
		result.AssertValid();
	}

	[Test]
	public void Issue435_NonCircularRefThrowing()
	{
		var schema = JsonSchema.FromText(@"{
  ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",

  ""type"": ""array"",
  ""items"": { ""$ref"": ""#/$defs/DerivedType"" },

  ""$defs"": {

    ""BaseType"": {
      ""type"": ""object"",
      ""properties"": {
        ""field1"": { ""type"": ""string"" }
      }
    },

    ""DerivedType"": {
      ""allOf"": [
        { ""$ref"": ""#/$defs/BaseType"" },
        { ""properties"": { ""field2"": { ""type"": ""string"" } } }
      ]
    }
  }
}");

		//var instance = new JsonArray
		//{
		//	new JsonObject
		//	{
		//		["field1"] = "foo",
		//		["field2"] = "bar"
		//	}
		//};

		var instance = JsonNode.Parse(@"[
  {
    ""field1"": ""foo"",
    ""field2"": ""bar""
  }
]");

		var result = schema.Evaluate(instance, new EvaluationOptions{OutputFormat = OutputFormat.List});

		result.AssertValid();
	}

	[Test]
	public void Issue435_NonCircularRefThrowing_File()
	{
		var file = GetFile(435, "schema");

		var schema = JsonSchema.FromFile(file);

		var instance = new JsonArray
		(
			new JsonObject
			{
				["field1"] = "foo",
				["field2"] = "bar"
			}
		);

		var result = schema.Evaluate(instance, new EvaluationOptions{OutputFormat = OutputFormat.List});

		result.AssertValid();
	}

	[Test]
	public void Issue506_ReffingIntoAdditionalProperties()
	{
		IBaseDocument schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.AdditionalProperties(new JsonSchemaBuilder()
				.Title("DictTemplate")
				.Type(SchemaValueType.Object)
				.Properties(
					("Enabled", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
				)
			).Build();

		var pointer = JsonPointer.Parse("/additionalProperties");
		var subSchema = schema.FindSubschema(pointer, EvaluationOptions.Default);

		Assert.That(subSchema, Is.Not.Null);
	}

	[Test]
	public void Issue516_UnrecognizedKeywordsViaBuilder()
	{
		var builder = new JsonSchemaBuilder();
		builder.Add(new UnrecognizedKeyword("foo", null));
		builder.Add(new UnrecognizedKeyword("bar", null));
		var actual = builder.Build();

		var text = "{\"foo\":null,\"bar\":null}";

		Assert.That(JsonSerializer.Serialize(actual, TestEnvironment.SerializerOptions), Is.EqualTo(text));
	}

	[TestCase(@"{""additionalItems"":""not-a-schema""}", 0, 33)]
	[TestCase(@"{""additionalProperties"":""not-a-schema""}", 0, 38)]
	[TestCase(@"{""allOf"":[""not-a-schema""]}", 0, 24)]
	[TestCase(@"{""anyOf"":[""not-a-schema""]}", 0, 24)]
	[TestCase(@"{""contains"":""not-a-schema""}", 0, 26)]
	[TestCase(@"{""contentSchema"":""not-a-schema""}", 0, 31)]
	[TestCase(@"{""definitions"":{""myDef"":""not-a-schema""}}", 0, 38)]
	[TestCase(@"{""$defs"":{""myDef"":""not-a-schema""}}", 0, 32)]
	[TestCase(@"{""dependencies"":{""myDep"":""not-a-schema""}}", 0, 39)]
	[TestCase(@"{""not"":""not-a-schema""}", 0, 21)]
	[TestCase(@"{""properties"":{""abc"": ""not-a-schema""}}", 0, 36)]
	[TestCase(@"{""additionalItems"":{""properties"":{""abc"": ""not-a-schema""}}}", 0, 55)]
	[TestCase(@"{
  ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
  ""required"": [ { ""abc"": null } ]
  ""properties"": {
      ""abc"": { ""$ref"": ""mySchema.json"" }
  },
  ""additionalProperties"": false
}", 2, 17)]
	public void Issue517_IncorrectJsonExceptionLineAndBytePosition(string schemaStr, int expectedLineNumber, int expectedBytePositionInLine)
	{
		// Reminder: per the JsonException documentation, expectedLineNumber & expectedBytePositionInLine are 0-based
		var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<JsonSchema>(schemaStr, TestEnvironment.SerializerOptions));
		Assert.Multiple(() =>
		{
			Assert.That(exception, Is.Not.Null);
			TestConsole.WriteLine("Expected error");
			TestConsole.WriteLine(schemaStr.Split('\n')[expectedLineNumber]);
			TestConsole.WriteLine(new string('-', expectedBytePositionInLine - 1) + '^');
			Assert.That(exception?.LineNumber, Is.EqualTo(expectedLineNumber));
			Assert.That(exception?.BytePositionInLine, Is.EqualTo(expectedBytePositionInLine));
		});
	}

	[Test]
	public void Issue600_BaseDocumentOutputSchemaLocation()
	{
		var file = GetFile(600, "schema");
		var text = File.ReadAllText(file);

		var baseDocumentJson = JsonNode.Parse(text)!;
		var baseDocument = new JsonNodeBaseDocument(baseDocumentJson, new Uri("http://localhost/v1"));

		var options = new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical };
		options.SchemaRegistry.Register(baseDocument);

		var schemaLocation = JsonPointer.Parse("/components/parameters/user/content/application~1json/schema");
		var schema = baseDocument.FindSubschema(schemaLocation, options)!;

		var instance = new JsonObject
		{
			["foo-name"] = "foo",
			["last-name"] = "bar"
		};

		var result = schema.Evaluate(instance, options);

		result.AssertInvalid();

		var targetSchemaLocation = result.Details[0].Details[0].SchemaLocation;

		Assert.That(targetSchemaLocation.OriginalString, Is.EqualTo("http://localhost/v1#/components/schemas/user/properties/last-name"));
	}

	[Test]
	public void Issue881_DateTimeValidationPassesWithOneOfSchema()
	{
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file);

		var instance = new JsonObject()
		{
			["oneOfDates"] = "2025-01-02T23:03:22.222Z"
		};

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		result.AssertValid();
	}

	[Test]
	public void Issue881_DateValidationPassesWithOneOfSchema()
	{
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file);

		var instance = new JsonObject()
		{
			["oneOfDates"] = "2025-01-02"
		};

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		result.AssertValid();
	}

	[Test]
	public void Issue881_DateValidationFailsAgainstDateTime()
	{
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file);

		var instance = new JsonObject()
		{
			["dateWithDateFormat"] = "2025-01-02T23:03:22.222Z"
		};

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		result.AssertInvalid();
	}

	[Test]
	public void Issue881_DateValidationPassesAgainstDate()
	{
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file);

		var instance = new JsonObject()
		{
			["dateWithDateFormat"] = "2025-01-02"
		};

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		result.AssertValid();
	}

	[Test]
	public void Issue881_TimeValidationFailsAgainstDateTime()
	{
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file);

		var instance = new JsonObject()
		{
			["timeWithTimeFormat"] = "2025-01-02T23:03:22.222Z"
		};

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		result.AssertInvalid();
	}

	[Test]
	public void Issue881_TimeValidationPassesAgainstTime()
	{
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file);

		var instance = new JsonObject()
		{
			["timeWithTimeFormat"] = "23:03:22"
		};

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		result.AssertValid();
	}

	[Test]
	public void Issue664_UIntConstNotValidating()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Schema with const UInt")
			.Properties(("prop", new JsonSchemaBuilder().Const((uint)1)))
			.AdditionalProperties(false)
			.Build();


		var json = """{"prop":1}""";
		var jsonDocumentInstance = JsonDocument.Parse(json);


		var schemaText = JsonSerializer.Serialize(schema, TestSerializerContext.Default.JsonSchema);
		var schemaFromString = JsonSchema.FromText(schemaText);

		var directEvaluationResult = schema.Evaluate(jsonDocumentInstance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.List,
			ValidateAgainstMetaSchema = true,
			RequireFormatValidation = true
		});

		var evaluationResultFromSchema = schemaFromString.Evaluate(jsonDocumentInstance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.List,
			ValidateAgainstMetaSchema = true,
			RequireFormatValidation = true
		});

		directEvaluationResult.AssertValid();
		evaluationResultFromSchema.AssertValid();
	}

#if !NET8_0_OR_GREATER
	// This test requires a non-AOT context, so it only runs in .Net 6

	[SchemaKeyword(Name)]
	[JsonConverter(typeof(UiPlaceholderKeywordJsonConverter))]
	public class UiPlaceholderKeyword : IJsonSchemaKeyword
	{
		public const string Name = "placeholder";
		public string Value { get; }

		public UiPlaceholderKeyword(string value)
		{
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, ReadOnlySpan<KeywordConstraint> localConstraints, EvaluationContext context)
		{
			return KeywordConstraint.SimpleAnnotation(Name, Value);
		}
	}

	public sealed class UiPlaceholderKeywordJsonConverter : WeaklyTypedJsonConverter<UiPlaceholderKeyword>
	{
		public override UiPlaceholderKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString()!;

			return new UiPlaceholderKeyword(str);
		}

		public override void Write(Utf8JsonWriter writer, UiPlaceholderKeyword value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Value);
		}
	}

	[Test]
	public void Issue667_CustomKeywordNonAotSerialization()
	{
		try
		{
			SchemaKeywordRegistry.Register<UiPlaceholderKeyword>();

			var subschema = new JsonSchemaBuilder().Type(SchemaValueType.String);
			subschema.Add(new UiPlaceholderKeyword("hello world"));

			var schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(("bar", subschema))
				.Build();

			TestConsole.WriteLine(JsonSerializer.Serialize(schema));
		}
		finally
		{
			SchemaKeywordRegistry.Unregister<UiPlaceholderKeyword>();
		}
	}
#endif

	[Test]
	public void Issue791_DecoratedClass()
	{
		Verify791();

		Run791<Model791>();
	}

	[Test]
	public void Issue791_UndecoratedClass()
	{
		ValidatingJsonConverter.MapType<Model791Undecorated>(Model791Schema);
		Run791<Model791Undecorated>();
	}

	private static void Verify791()
	{
		var jsonText = @"{ ""Foo"": ""foo"",  ""Bar"": -42 }";
		var node = JsonNode.Parse(jsonText);

		var results = Model791Schema.Evaluate(node, new EvaluationOptions { OutputFormat = OutputFormat.List });
		TestConsole.WriteLine(JsonSerializer.Serialize(results, TestEnvironment.TestOutputSerializerOptions));
	}

	private static void Run791<T>()
	{
		var jsonText = @"{ ""Foo"": ""foo"",  ""Bar"": -42 }";
		var converter = new ValidatingJsonConverter { OutputFormat = OutputFormat.List };
		var options = new JsonSerializerOptions(TestEnvironment.TestOutputSerializerOptions) { Converters = { converter } };
		var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<T>(jsonText, options));
		var result = ex.Data["validation"] as EvaluationResults;
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Details, Has.Count.GreaterThan(0));
	}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	// ReSharper disable ClassNeverInstantiated.Local
	[JsonSchema(typeof(GithubTests), nameof(Model791Schema))]
	public class Model791
	{
		public string Foo { get; set; }
		public int Bar { get; set; }
		public DateTime Baz { get; set; }
	}

	public class Model791Undecorated
	{
		public string Foo { get; set; }
		public int Bar { get; set; }
		public DateTime Baz { get; set; }
	}
	// ReSharper restore ClassNeverInstantiated.Local
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

	public static readonly JsonSchema Model791Schema =
		new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				(nameof(Model791.Foo), new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.MinLength(10)
					.MaxLength(50)
				),
				(nameof(Model791.Bar), new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Minimum(0)
				),
				(nameof(Model791.Baz), new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime)
				)
			)
			.Required(nameof(Model791.Baz));
}