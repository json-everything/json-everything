using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

	// https://github.com/json-everything/json-everything/issues/18
	[Test]
	public void Issue18_SomethingNotValidatingRight()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var instance = JsonDocument.Parse("""
			{
			    "prop1": {
			        "name": "a",
			        "version": 1
			    },
			    "prop2": {},
			    "prop4": "a",
			    "prop5": {},
			    "prop6": {
			        "firstId": "428de96d-d5b2-4d12-8e88-37827099dd02",
			        "secondId": "428de96d-d5b2-4d12-8e88-37827099dd02",
			        "version": "test-version",
			        "thirdId": "428de96d-d5b2-4d12-8e88-37827099dd02",
			        "type": "test",
			        "name": "testApp",
			        "receiptTimestamp": "2019-02-05T12:36:31.2812022Z",
			        "timestamp": "2012-04-21T12:36:31.2812022Z",
			        "extra_key": "extra_val"
			    },
			    "prop3": {
			        "prop5": {},
			        "metadata": {},
			        "deleteAfter": 3,
			        "allowExport": true
			    }
			}
			""").RootElement;
		var schema = JsonSchema.FromText("""
			{
				"$schema": "http://json-schema.org/draft-07/schema#",
				"type": "object",
				"required": ["prop1", "prop2", "prop3", "prop4", "prop5", "prop6"],
				"properties": {
				    "prop1": {
				        "type": "object",
				        "required": ["name", "version"],
				        "additionalProperties": false,
				        "properties": {
				            "name": {
				                "type": "string",
				                "pattern": "^[-_]?([a-zA-Z][-_]?)+$"
				            },
				            "version": {
				                "type": "integer",
				                "minimum": 1
				            }
				        }
				    },
				    "prop2": {
				        "$ref": "http://json-schema.org/draft-07/schema#"
				    },
				    "prop3": {
				        "type": "object",
				        "required": [
				        "prop5",
				        "metadata"
				        ],
				        "additionalProperties": false,
				        "properties": {
				            "prop5": {
				                "type": "object"
				            },
				            "metadata": {
				                "type": "object"
				            },
				            "deleteAfter": {
				                "type": "integer"
				            },
				            "allowExport": {
				                "type": "boolean"
				            }
				        }
				    },
				    "prop4": {
				        "type": "string",
				        "pattern": "^[-_]?([a-zA-Z][-_]?)+$"
				    },
				    "prop5": {
				        "type": "object"
				    },
				    "prop6": {
				        "type": "object",
				        "required": [
				        "firstId",
				        "secondId",
				        "version",
				        "thirdId",
				        "type",
				        "name",
				        "receiptTimestamp",
				        "timestamp"
				        ],
				        "properties": {
				            "firstId": {
				                "type": "string",
				                "pattern": "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"
				            },
				            "secondId": {
				                "type": "string",
				                "pattern": "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"
				            },
				            "type": {
				                "type": "string",
				                "enum": ["test", "lab", "stage", "prod"]
				            },
				            "thirdId": {
				                "type": "string",
				                "pattern": "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"
				            },
				            "version": {
				                "type": "string",
				                "minLength": 1
				            },
				            "name": {
				                "type": "string",
				                "minLength": 1
				            },
				            "receiptTimestamp": {
				                "type": "string",
				                "format": "date-time"
				            },
				            "timestamp": {
				                "type": "string",
				                "format": "date-time"
				            }
				        },
				        "additionalProperties": {
				            "type": "string"
				        }
				    }
				},
				"additionalProperties": false
			}
			""", buildOptions);

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/19
	[Test]
	public void Issue19_Draft4ShouldInvalidateAsUnrecognizedSchema_NoOption()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };

		Assert.Throws<JsonSchemaException>(() => JsonSchema.FromText("""{"$schema":"http://json-schema.org/draft-04/schema#","type":"string"}""", buildOptions));
	}

	// https://github.com/json-everything/json-everything/issues/19
	[TestCase("http://json-schema.org/draft-07/schema#", @"{}", true)]
	[TestCase("http://json-schema.org/draft-07/schema#", @"{""abc"": 1}", false)]
	[TestCase("http://json-schema.org/draft-07/schema#", @"{""abc"": 1, ""d7"": 7}", true)]
	[TestCase("http://json-schema.org/draft-07/schema#", @"{""abc"": 1, ""d9"": 9}", false)]
	[TestCase("http://json-schema.org/draft-07/schema#", @"{""abc"": 1, ""d7"": 7, ""d9"": 9}", true)]
	[TestCase("https://json-schema.org/draft/2019-09/schema", @"{}", true)]
	[TestCase("https://json-schema.org/draft/2019-09/schema", @"{""abc"": 1}", false)]
	[TestCase("https://json-schema.org/draft/2019-09/schema", @"{""abc"": 1, ""d7"": 7}", false)]
	[TestCase("https://json-schema.org/draft/2019-09/schema", @"{""abc"": 1, ""d9"": 9}", true)]
	[TestCase("https://json-schema.org/draft/2019-09/schema", @"{""abc"": 1, ""d7"": 7, ""d9"": 9}", true)]
	public void Issue19_SchemaShouldOnlyUseSpecifiedDraftKeywords(string metaSchemaId, string instance, bool isValid)
	{
		var dialect = metaSchemaId switch
		{
			"http://json-schema.org/draft-07/schema#" => Dialect.Draft07,
			"https://json-schema.org/draft/2019-09/schema" => Dialect.Draft201909,
			_ => throw new ArgumentException($"Unknown meta-schema: {metaSchemaId}")
		};
		var buildOptions = new BuildOptions { SchemaRegistry = new(), Dialect = dialect };
		var schema = JsonSchema.FromText("""
			{
			    "dependencies": {
			        "abc": [ "d7" ]
			    },
			    "dependentRequired": {
			        "abc": [ "d9" ]
			    }
			}
			""", buildOptions);
		var opts = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		var element = JsonDocument.Parse(instance).RootElement;

		var val = schema.Evaluate(element, opts);
		TestConsole.WriteLine("Elem `{0}` got validation `{1}`", instance, val.IsValid);
		if (isValid) val.AssertValid();
		else val.AssertInvalid();
	}

	// https://github.com/json-everything/json-everything/issues/29
	[Test]
	public void Issue29_SchemaFromFileWithoutIdShouldInheritUriFromFilePath()
	{
		var schemaFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "issue29-schema-without-id.json")
			.AdjustForPlatform();

		var jsonStr = """
			{
			  "abc": {
			    "abc": {
			        "abc": "abc"
			    }
			  }
			}
			""";
		var schema = JsonSchema.FromFile(schemaFile);
		var json = JsonDocument.Parse(jsonStr).RootElement;
		var validation = schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		validation.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/29
	[Test]
	public void Issue29_SchemaFromFileWithIdShouldKeepUriFromId()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schemaFile = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "issue29-schema-with-id.json")
			.AdjustForPlatform();

		var jsonStr = """
			{
			  "abc": {
			    "abc": {
			        "abc": "abc"
			    }
			  }
			}
			""";
		var schema = JsonSchema.FromFile(schemaFile, buildOptions);
		var json = JsonDocument.Parse(jsonStr).RootElement;
		var validation = schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		validation.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/29
	[Test]
	public void Issue29_SchemaWithOnlyFileNameIdShouldUseDefaultBaseUri()
	{
		var schemaStr = """
			{
			  "$schema": "http://json-schema.org/draft-07/schema#",
			  "$id": "mySchema.json",
			  "properties": {
			      "abc": { "$ref": "mySchema.json" }
			  },
			  "additionalProperties": false
			}
			""";
		var jsonStr = """
			{
			  "abc": {
			    "abc": {
			        "abc": "abc"
			    }
			  }
			}
			""";
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText(schemaStr, buildOptions);
		var json = JsonDocument.Parse(jsonStr).RootElement;
		var validation = schema.Evaluate(json, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		validation.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/79
	[Test]
	public void Issue79_RefsTryingToResolveParent()
	{
		var schema1Str = """
			{
			  "$schema": "http://json-schema.org/draft-07/schema#",
			  "$id": "schema1.json",
			  "definitions": {
			    "myDef": {
			      "properties": {
			        "abc": { "type": "string" }
			      }
			    }
			  },
			  "$ref": "#/definitions/myDef"
			}
			""";
		var schema2Str = """
			{
			  "$schema": "http://json-schema.org/draft-07/schema#",
			  "$id": "schema2.json",
			  "$ref": "schema1.json"
			}
			""";
		var jsonStr = """{ "abc": "s" }""";
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema1 = JsonSchema.FromText(schema1Str, buildOptions);
		var schema2 = JsonSchema.FromText(schema2Str, buildOptions);
		var json = JsonDocument.Parse(jsonStr).RootElement;
		var uri1 = new Uri("https://json-everything.net/schema1.json");
		var uri2 = new Uri("https://json-everything.net/schema2.json");
		var map = new Dictionary<Uri, JsonSchema>
		{
			{ uri1, schema1 },
			{ uri2, schema2 },
		};
		buildOptions.SchemaRegistry.Fetch = (uri, _) =>
		{
			Assert.That(map.TryGetValue(uri, out var ret), Is.True, $"Unexpected uri: {uri}");
			return ret;
		};
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		var result = schema2.Evaluate(json, options);
		result.AssertValid();
		Assert.That(result.Details![0].SchemaLocation.OriginalString, Is.EqualTo("https://json-everything.lib/schema1.json#"));
	}

	// https://github.com/json-everything/json-everything/issues/79
	[Test]
	public void Issue79_RefsTryingToResolveParent_Explanation()
	{
		var schemaText = """
			{
			  "$id": "https://mydomain.com/outer",
			  "properties": {
			    "foo": {
			      "$id": "https://mydomain.com/foo",
			      "properties": {
			        "inner1": {
			          "$anchor": "bar",
			          "type": "string"
			        },
			        "inner2": {
			          "$ref": "#bar"
			        }
			      }
			    },
			    "bar": {
			      "$anchor": "bar",
			      "type": "integer"
			    }
			  }
			}
			""";
		var passingText = """
			{
			  "foo": {
			    "inner2": "value"
			  }
			}
			""";
		var failingText = """
			{
			  "foo": {
			    "inner2": 42
			  }
			}
			""";

		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText(schemaText, buildOptions);
		var passing = JsonDocument.Parse(passingText).RootElement;
		var failing = JsonDocument.Parse(failingText).RootElement;

		schema.Evaluate(passing, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical }).AssertValid();
		schema.Evaluate(failing, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical }).AssertInvalid();
	}

	// https://github.com/json-everything/json-everything/issues/97
	[Test]
	public void Issue97_IdentifyCircularReferences()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		Assert.Throws<JsonSchemaException>(() => new JsonSchemaBuilder(buildOptions)
			.Ref("#/$defs/string")
			.Defs(("string", new JsonSchemaBuilder().Ref("#/$defs/string")))
			.Build());
	}

	// https://github.com/json-everything/json-everything/issues/97
	[Test]
	public void Issue97_IdentifyComplexCircularReferences()
	{
		Assert.Throws<JsonSchemaException>(() => new JsonSchemaBuilder()
			.Ref("#/$defs/a")
			.Defs(
				("a", new JsonSchemaBuilder().Ref("#/$defs/b")),
				("b", new JsonSchemaBuilder().Ref("#/$defs/a"))
			)
			.Build());
	}

	// https://github.com/json-everything/json-everything/issues/191
	[Test]
	public void Issue191_SelfReferentialCustomMetaschemaShouldError()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };

		var metaSchemaText = GetResource(191, "MetaSchema");

		Assert.Throws<JsonSchemaException>(() => JsonSchema.FromText(metaSchemaText, buildOptions));
	}

	// https://github.com/json-everything/json-everything/issues/208
	//[Test]
	//public void Issue208_BundlingNotWorking()
	//{
	//	var buildOptions = new BuildOptions { SchemaRegistry = new() };
	//	JsonSchema externalSchema = new JsonSchemaBuilder(buildOptions)
	//		.Schema(MetaSchemas.Draft202012Id)
	//		.Id("https://my-external-schema")
	//		.Type(SchemaValueType.Object)
	//		.Properties(
	//			("first", new JsonSchemaBuilder().Type(SchemaValueType.String))
	//		)
	//		.Build();

	//	buildOptions.SchemaRegistry.Register(new Uri("https://my-external-schema"), externalSchema);

	//	JsonSchema mySchema = new JsonSchemaBuilder(buildOptions)
	//		.Schema(MetaSchemas.Draft202012Id)
	//		.Id("https://my-schema")
	//		.Type(SchemaValueType.Object)
	//		.Properties(
	//			("first", new JsonSchemaBuilder().Ref("https://my-external-schema")),
	//			("second", new JsonSchemaBuilder()
	//				.Schema(MetaSchemas.Draft202012Id)
	//				.Id("https://my-inner-schema")
	//				.Type(SchemaValueType.Object)
	//				.Properties(
	//					("second", new JsonSchemaBuilder().Ref("#/$defs/my-inner-ref"))
	//				)
	//				.Defs(
	//					("my-inner-ref", new JsonSchemaBuilder().Type(SchemaValueType.String))
	//				)
	//			)
	//		)
	//		.Build();

	//	var instance = JsonDocument.Parse("{\"first\":{\"first\":\"first\"},\"second\":{\"second\":\"second\"}}").RootElement;

	//	var options = new EvaluationOptions
	//	{
	//		OutputFormat = OutputFormat.Hierarchical
	//	};
	//	mySchema.Evaluate(instance, options).AssertValid();

	//}

	// https://github.com/json-everything/json-everything/issues/212
	[Test]
	public void Issue212_CouldNotResolveAnchorReference_FromFile()
	{
		// This validation fails because the file uses `id` instead of `$id`.
		// See https://github.com/json-everything/json-everything/issues/212#issuecomment-1033423550
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "issue212_schema.json")
			.AdjustForPlatform();
		var schema = JsonSchema.FromFile(path, buildOptions);

		var instance = JsonDocument.Parse("{\"ContentDefinitionId\": \"fa81bc1d-3efe-4192-9e03-31e9898fef90\"}").RootElement;

		Assert.Throws<RefResolutionException>(() => schema.Evaluate(instance));
	}

	// https://github.com/json-everything/json-everything/issues/212
	[Test]
	public void Issue212_CouldNotResolveAnchorReference_Inline()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
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
			.AdditionalProperties(false)
			.Build();

		var instance = JsonDocument.Parse("{\"ContentDefinitionId\": \"fa81bc1d-3efe-4192-9e03-31e9898fef90\"}").RootElement;

		var res = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		});
		res.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/216
	[Test]
	public void Issue216_AdditionalPropertiesShouldRelyOnDeclarationsForDraft7()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft7Id)
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("bar", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.AdditionalProperties(false)
			.Build();

		var instance = JsonDocument.Parse("{\"foo\":1,\"bar\":false}").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		result.AssertInvalid();
		var nodes = new List<EvaluationResults> { result };
		while (nodes.Count != 0)
		{
			var node = nodes.First();
			nodes.Remove(node);
			Assert.That(node.EvaluationPath.ToString(), Is.Not.EqualTo("#/additionalProperties"));
			if (node.Details != null) nodes.AddRange(node.Details);
		}
	}

	// https://github.com/json-everything/json-everything/issues/226
	[Test]
	public void Issue226_MessageInValidResult()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schemaText = GetResource(226, "schema");
		var instanceText = GetResource(226, "instance");

		var schema = JsonSchema.FromText(schemaText, buildOptions);
		var instance = JsonDocument.Parse(instanceText).RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/352
	[Test]
	public void Issue352_ConcurrentValidationsWithReferences()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText("""
			{
			    "$schema": "http://json-schema.org/draft-07/schema#",
			    "type": "object",
			    "properties": {
			        "id": {
			            "type": "integer"
			        },        
			        "interval1": {
			            "$ref": "#/components/schemas/interval"
			        }
			    },
			    "components": {
			        "schemas": {
			            "interval": {
			                "type": "object",
			                "properties": {
			                    "from": {
			                        "type": "number"
			                    },
			                    "to": {
			                        "type": "number"
			                    }
			                }
			            }
			        }
			    }
			}
			""", buildOptions);

		var instanceJson = """{"id": 123, "interval1": {"to": 3.0}}""";
		var instance = JsonDocument.Parse(instanceJson).RootElement;

		// verify it runs once
		var result = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.List,
			RequireFormatValidation = true
		});
		result.AssertValid();

		// run in parallel
		var numberOfMessages = 100;
		var jsonMessages = new List<JsonElement>();
		for (int j = 0; j < numberOfMessages; j++)
		{
			jsonMessages.Add(JsonDocument.Parse(instanceJson).RootElement);
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

	// https://github.com/json-everything/json-everything/issues/417
	//[Test]
	//public void Issue417_BundleWithItemsFails()
	//{
	//	// items: true
	//	var singleItemSchema = new JsonSchemaBuilder()
	//		.Items(true)
	//		.Build();
	//	singleItemSchema.Bundle(); // throws
	//	// items: [true, true]
	//	var multiItemSchema = new JsonSchemaBuilder()
	//		.Items([true, true])
	//		.Build();
	//	multiItemSchema.Bundle(); // throws
	//}

	// https://github.com/json-everything/json-everything/issues/419
	[Test]
	public void Issue419_SecondLevelReferences()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		_ = JsonSchema.FromFile(GetFile(419, "level2"), buildOptions);

		_ = JsonSchema.FromFile(GetFile(419, "level1"), buildOptions);

		var rootSchema = JsonSchema.FromFile(GetFile(419, "root"), buildOptions);

		var config = JsonDocument.Parse(GetResource(419, "config")).RootElement;

		var result = rootSchema.Evaluate(config);

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/426
	[Test]
	public void Issue426_FileFragmentRefs()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromFile(GetFile(426, "schema"), buildOptions);
		
		var data = JsonDocument.Parse(File.ReadAllText(GetFile(426, "data"))).RootElement;
		
		var result = schema.Evaluate(data);

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/432
	[Test]
	public void Issue432_UnresolvedLocalRef()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText("""
			{
			  "$schema": "http://json-schema.org/draft-06/schema#",
			  "$ref": "#/definitions/Order",
			  "definitions": {
			    "Order": {
			      "type": "object",
			      "additionalProperties": true,
			      "properties": {
			        "orderId": {
			          "type": "string",
			          "format": "uuid"
			        }
			      },
			      "required": [
			        "orderId"
			      ],
			      "title": "Order"
			    }
			  }
			}
			""", buildOptions);
		var instance = JsonDocument.Parse("""{ "orderId": "3cb65f2d-4049-43c1-b185-1943765acd9" }""").RootElement;

		var result = schema.Evaluate(instance);
		
		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/432
	[Test]
	public void Issue432_UnresolvedLocalRef_Again()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = JsonSchema.FromText("""
			{
			  "$schema": "http://json-schema.org/draft-06/schema#",
			  "$ref": "#/definitions/Order",
			  "definitions": {
			    "Order": {
			      "type": "object",
			      "additionalProperties": true,
			      "properties": {
			        "orderId": {
			          "type": "string",
			          "format": "uuid"
			        },
			        "customer": {
			          "$ref": "#/definitions/Customer"
			        }
			      },
			      "required": [
			        "customer",
			        "orderId"
			      ],
			      "title": "Order"
			    },
			    "Customer": {
			      "type": "object",
			      "additionalProperties": false,
			      "properties": {
			        "name": {
			          "type": "string"
			        }
			      },
			      "required": [
			        "name"
			      ],
			      "title": "Customer"
			    }
			  }
			}
			""", buildOptions);
		var instance = JsonDocument.Parse("""
			{
			  "orderId": "3cb65f2d-4049-4c1-b185-194365acdf99",
			  "customer": {
			    "name": "Some Customer"
			  }
			}
			""").RootElement;

		var result = schema.Evaluate(instance);
		
		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/435
	[Test]
	public void Issue435_NonCircularRefThrowing()
	{
		var schema = JsonSchema.FromText("""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",

			  "type": "array",
			  "items": { "$ref": "#/$defs/DerivedType" },

			  "$defs": {

			    "BaseType": {
			      "type": "object",
			      "properties": {
			        "field1": { "type": "string" }
			      }
			    },

			    "DerivedType": {
			      "allOf": [
			        { "$ref": "#/$defs/BaseType" },
			        { "properties": { "field2": { "type": "string" } } }
			      ]
			    }
			  }
			}
			""");

		var instance = JsonDocument.Parse("""
			[
			  {
			    "field1": "foo",
			    "field2": "bar"
			  }
			]
			""").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions{OutputFormat = OutputFormat.List});

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/435
	[Test]
	public void Issue435_NonCircularRefThrowing_File()
	{
		var file = GetFile(435, "schema");

		var schema = JsonSchema.FromFile(file);

		var instance = JsonDocument.Parse("""
			[
			  {
			    "field1": "foo",
			    "field2": "bar"
			  }
			]
			""").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions{OutputFormat = OutputFormat.List});

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/506
	[Test]
	public void Issue506_ReffingIntoAdditionalProperties()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var schema = new JsonSchemaBuilder(buildOptions)
			.Id("https://test.example/schema")
			.Type(SchemaValueType.Object)
			.AdditionalProperties(new JsonSchemaBuilder()
				.Title("DictTemplate")
				.Type(SchemaValueType.Object)
				.Properties(
					("Enabled", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
				)
			).Build();

		buildOptions.SchemaRegistry.Register(schema);

		// Create a schema that refs into the additionalProperties
		var refSchema = new JsonSchemaBuilder(buildOptions)
			.Ref("https://test.example/schema#/additionalProperties")
			.Build();

		// Verify the ref resolves correctly
		var instance = JsonDocument.Parse("""{"Enabled": true}""").RootElement;
		var result = refSchema.Evaluate(instance);

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/516
	[Test]
	public void Issue516_UnrecognizedKeywordsViaBuilder()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			Dialect = Dialect.Draft202012
		};
		var builder = new JsonSchemaBuilder(buildOptions);
		builder.Unrecognized("foo", null);
		builder.Unrecognized("bar", null);
		var actual = builder.Build();

		var text = "{\"foo\":null,\"bar\":null}";

		Assert.That(JsonSerializer.Serialize(actual, TestEnvironment.SerializerOptions), Is.EqualTo(text));
	}

	// https://github.com/json-everything/json-everything/issues/517
	[TestCase("""{"additionalItems":"not-a-schema"}""", 0, 33)]
	[TestCase("""{"additionalProperties":"not-a-schema"}""", 0, 38)]
	[TestCase("""{"allOf":["not-a-schema"]}""", 0, 24)]
	[TestCase("""{"anyOf":["not-a-schema"]}""", 0, 24)]
	[TestCase("""{"contains":"not-a-schema"}""", 0, 26)]
	[TestCase("""{"contentSchema":"not-a-schema"}""", 0, 31)]
	[TestCase("""{"definitions":{"myDef":"not-a-schema"}}""", 0, 38)]
	[TestCase("""{"$defs":{"myDef":"not-a-schema"}}""", 0, 32)]
	[TestCase("""{"dependencies":{"myDep":"not-a-schema"}}""", 0, 39)]
	[TestCase("""{"not":"not-a-schema"}""", 0, 21)]
	[TestCase("""{"properties":{"abc": "not-a-schema"}}""", 0, 36)]
	[TestCase("""{"additionalItems":{"properties":{"abc": "not-a-schema"}}}""", 0, 55)]
	[TestCase("""
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "required": [ { "abc": null } ]
  "properties": {
      "abc": { "$ref": "mySchema.json" }
  },
  "additionalProperties": false
}
""", 2, 17)]
	[Ignore("These tests are no longer valid since we're not deserializing schemas anymore.")]
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
			TestConsole.WriteLine();
			TestConsole.WriteLine("Actual error");
			TestConsole.WriteLine(schemaStr.Split('\n')[exception!.LineNumber ?? 0]);
			TestConsole.WriteLine(new string('-', (int)(exception.BytePositionInLine ?? 0) - 1) + '^');
			Assert.That(exception.LineNumber, Is.EqualTo(expectedLineNumber));
			Assert.That(exception.BytePositionInLine, Is.EqualTo(expectedBytePositionInLine));
		});
	}

	// https://github.com/json-everything/json-everything/issues/600
	[Test]
	public void Issue600_BaseDocumentOutputSchemaLocation()
	{
		var file = GetFile(600, "schema");
		var text = File.ReadAllText(file);

		var baseDocumentJson = JsonDocument.Parse(text).RootElement;
		var baseDocument = new JsonElementBaseDocument(baseDocumentJson, new Uri("http://localhost/v1"));

		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		buildOptions.SchemaRegistry.Register(baseDocument);

		// Create a schema that refs into the base document at the schema location
		var schema = new JsonSchemaBuilder(buildOptions)
			.Ref("http://localhost/v1#/components/parameters/user/content/application~1json/schema")
			.Build();

		var instance = JsonDocument.Parse("""{"foo-name": "foo", "last-name": "bar"}""").RootElement;

		var options = new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical };
		var result = schema.Evaluate(instance, options);

		result.AssertInvalid();

		var targetSchemaLocation = result.Details![0].Details![0].Details![0].SchemaLocation;

		Assert.That(targetSchemaLocation.OriginalString, Is.EqualTo("http://localhost/v1#/components/schemas/user/properties/last-name"));
	}

	// https://github.com/json-everything/json-everything/issues/881
	[Test]
	public void Issue881_UsingSchema201909DateTimeValidationPassesWithOneOfSchema()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file, buildOptions);

		var instance = JsonDocument.Parse("""{"oneOfDates": "2025-01-02T23:03:22.222Z"}""").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true });

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/881
	[Test]
	public void Issue881_UsingSchema201909DateValidationPassesWithOneOfSchema()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file, buildOptions);

		var instance = JsonDocument.Parse("""{"oneOfDates": "2025-01-02"}""").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true });

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/881
	[Test]
	public void Issue881_UsingSchema201909DateValidationFailsAgainstDateTimeFormat()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file, buildOptions);

		var instance = JsonDocument.Parse("""{"dateWithDateFormat": "2025-01-02T23:03:22.222Z"}""").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true });

		result.AssertInvalid();
	}

	// https://github.com/json-everything/json-everything/issues/881
	[Test]
	public void Issue881_UsingSchema201909DateValidationPassesAgainstDateFormat()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file, buildOptions);

		var instance = JsonDocument.Parse("""{"dateWithDateFormat": "2025-01-02"}""").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true });

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/881
	[Test]
	public void Issue881_UsingSchema201909TimeValidationFailsAgainstDateTimeFormat()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file, buildOptions);

		var instance = JsonDocument.Parse("""{"timeWithTimeFormat": "2025-01-02T23:03:22.222Z"}""").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true });

		result.AssertInvalid();
	}

	// https://github.com/json-everything/json-everything/issues/881
	[Test]
	public void Issue881_UsingSchema201909TimeValidationPassesAgainstTimeFormat()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		var file = GetFile(881, "schema");

		var schema = JsonSchema.FromFile(file, buildOptions);

		var instance = JsonDocument.Parse("""{"timeWithTimeFormat": "23:03:22Z"}""").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true });

		result.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/664
	[Test]
	public void Issue664_UIntConstNotValidating()
	{
		var schema = new JsonSchemaBuilder()
			.Title("Schema with const UInt")
			.Properties(("prop", new JsonSchemaBuilder().Const((uint)1)))
			.AdditionalProperties(false)
			.Build();


		var json = """{"prop":1}""";
		var jsonDocumentInstance = JsonDocument.Parse(json).RootElement;


		var schemaText = JsonSerializer.Serialize(schema, TestSerializerContext.Default.JsonSchema);
		var schemaFromString = JsonSchema.FromText(schemaText);

		var directEvaluationResult = schema.Evaluate(jsonDocumentInstance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.List,
			RequireFormatValidation = true
		});

		var evaluationResultFromSchema = schemaFromString.Evaluate(jsonDocumentInstance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.List,
			RequireFormatValidation = true
		});

		directEvaluationResult.AssertValid();
		evaluationResultFromSchema.AssertValid();
	}

	// https://github.com/json-everything/json-everything/issues/791
	[Test]
	public void Issue791_DecoratedClass()
	{
		Verify791();

		Run791<Model791>();
	}

	// https://github.com/json-everything/json-everything/issues/791
	[Test]
	public void Issue791_UndecoratedClass()
	{
		ValidatingJsonConverter.MapType<Model791Undecorated>(Model791Schema);
		Run791<Model791Undecorated>();
	}

	private static void Verify791()
	{
		var jsonText = """{ "Foo": "foo",  "Bar": -42 }""";
		var node = JsonDocument.Parse(jsonText).RootElement;

		var results = Model791Schema.Evaluate(node, new EvaluationOptions { OutputFormat = OutputFormat.List });
		TestConsole.WriteLine(JsonSerializer.Serialize(results, TestEnvironment.TestOutputSerializerOptions));
	}

	private static void Run791<T>()
	{
		var jsonText = """{ "Foo": "foo",  "Bar": -42 }""";
		var converter = new ValidatingJsonConverter { EvaluationOptions = { OutputFormat = OutputFormat.List } };
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
			.Required(nameof(Model791.Baz))
			.Build();

	[Test]
	public void Issue965_StackOverflow()
	{
		var schemaJson = JsonDocument.Parse(
			"""
			{
			  "$schema": "http://json-schema.org/draft-07/schema#",
			  "$id": "https://json-everything.test/overflow",
			  "$ref": "#/$defs/modelDefinition",
			  "$defs": {
			    "modelDefinition": {
			      "type": "object",
			      "additionalProperties": {
			        "$ref": "#/$defs/modelDefinition"
			      }
			    }
			  }
			}
			""").RootElement;

		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};
		var schema = JsonSchema.Build(schemaJson, buildOptions);
	}

	[Test]
	public void Issue965_StackOverflow_Definitions()
	{
		var schemaJson = JsonDocument.Parse(
			"""
			{
			  "$schema": "http://json-schema.org/draft-07/schema#",
			  "$id": "https://json-everything.test/overflow",
			  "$ref": "#/definitions/modelDefinition",
			  "definitions": {
			    "modelDefinition": {
			      "type": "object",
			      "additionalProperties": {
			        "$ref": "#/definitions/modelDefinition"
			      }
			    }
			  }
			}
			""").RootElement;

		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};
		var schema = JsonSchema.Build(schemaJson, buildOptions);
	}

	[Test]
	public void Issue966_AdditionalPropertiesThroughRef()
	{
		var schemaJson = JsonDocument.Parse(
			"""
			{
			  "$id": "ID",
			  "$schema": "https://json-schema.org/draft/2020-12/schema#",
			  "title": "TITLE",
			  "type": "object",
			  "properties": {
			    "incident": {
			      "$ref": "#/$defs/Incident"
			    }
			  },
			  "$defs": {
			    "Incident": {
			      "type": "object",
			      "properties": {
			        "incidentType": {
			          "$ref": "#/$defs/IncidentType"
			        }
			      },
			      "additionalProperties": false
			    },
			    "IncidentType": {
			      "type": "object",
			      "properties": {
			        "incidentClassification": {
			          "type": "array",
			          "items": {
			            "type": "string"
			          }
			        }
			      }
			    }
			  }
			}
			""").RootElement;
		var instance = JsonDocument.Parse(
			"""
			{
			  "incident": {
			    "incidentType": {
			      "incidentClassification": [
			        "something",
			        "something-else"
			      ]
			    }
			  }
			}
			""").RootElement;

		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};
		var schema = JsonSchema.Build(schemaJson, buildOptions);

		var evaluationOptions = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		var results = schema.Evaluate(instance, evaluationOptions);

		results.AssertValid();
	}
}