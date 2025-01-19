using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

[TestFixture, SetCulture("en-US")]
public class OutputTests
{
	private static readonly JsonSchema _schema =
		new JsonSchemaBuilder()
			.Id("https://test.com/schema")
			.Defs(
				("integer", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("minimum", new JsonSchemaBuilder().Minimum(5))
			)
			.Type(SchemaValueType.Object)
			.Properties(
				("passes", true),
				("fails", false),
				("refs", new JsonSchemaBuilder().Ref("#/$defs/integer")),
				("multi", new JsonSchemaBuilder()
					.AllOf(
						new JsonSchemaBuilder().Ref("#/$defs/integer"),
						new JsonSchemaBuilder().Ref("#/$defs/minimum")
					)
				)
			);

	[Test]
	public void Flag_Success()
	{
		var result = Validate("{\"passes\":\"value\"}", OutputFormat.Flag);
		var expected = @"{
  ""valid"": true
}";

		result.AssertValid(expected);
	}

	[Test]
	public void Flag_Failure()
	{
		var result = Validate("{\"fails\":\"value\"}", OutputFormat.Flag);
		var expected = @"{
  ""valid"": false
}";

		result.AssertInvalid(expected);
	}

	[Test]
	public void Basic_Success()
	{
		var result = Validate("{\"passes\":\"value\"}", OutputFormat.List);
		var expected = @"{
  ""valid"": true,
  ""details"": [
    {
      ""valid"": true,
      ""evaluationPath"": """",
      ""schemaLocation"": ""https://test.com/schema#"",
      ""instanceLocation"": """",
      ""annotations"": {
        ""properties"": [ ""passes"" ]
      }
    },
    {
      ""valid"": true,
      ""evaluationPath"": ""/properties/passes"",
	  ""schemaLocation"": ""https://test.com/schema#/properties/passes"",
	  ""instanceLocation"": ""/passes""
    }
  ]
}";

		result.AssertValid(expected);
	}

	[Test]
	public void Basic_Failure()
	{
		var result = Validate("{\"fails\":\"value\"}", OutputFormat.List);
		var expected = @"{
  ""valid"": false,
  ""details"": [
    {
      ""valid"": false,
      ""evaluationPath"": """",
	  ""schemaLocation"": ""https://test.com/schema#"",
	  ""instanceLocation"": """"
	},
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/fails"",
      ""schemaLocation"": ""https://test.com/schema#/properties/fails"",
      ""instanceLocation"": ""/fails"",
      ""errors"": {
        """": ""All values fail against the false schema""
      }
    }
  ]
}";

		result.AssertInvalid(expected);
	}

	[Test]
	public void Hierarchical_Success()
	{
		var result = Validate("{\"passes\":\"value\"}", OutputFormat.Hierarchical);
		var expected = @"{
  ""valid"": true,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""annotations"": {
    ""properties"": [
      ""passes""
    ]
  },
  ""details"": [
    {
      ""valid"": true,
      ""evaluationPath"": ""/properties/passes"",
      ""schemaLocation"": ""https://test.com/schema#/properties/passes"",
      ""instanceLocation"": ""/passes""
    }
  ]
}";

		result.AssertValid(expected);
	}

	[Test]
	public void Hierarchical_Failure()
	{
		var result = Validate("{\"fails\":\"value\"}", OutputFormat.Hierarchical);
		var expected = @"{
  ""valid"": false,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""details"": [
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/fails"",
      ""schemaLocation"": ""https://test.com/schema#/properties/fails"",
      ""instanceLocation"": ""/fails"",
      ""errors"": {
        """": ""All values fail against the false schema""
      }
    }
  ]
}";
		result.AssertInvalid(expected);
	}

	[Test]
	public void Hierarchical_Failure_WithDroppedAnnotations()
	{
		var instance = JsonNode.Parse("{\"fails\":\"value\"}");
		var options = EvaluationOptions.From(EvaluationOptions.Default);
		options.OutputFormat = OutputFormat.Hierarchical;
		options.PreserveDroppedAnnotations = true;

		var result = _schema.Evaluate(instance, options);
		var expected = @"{
  ""valid"": false,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""droppedAnnotations"": {
    ""properties"": [
      ""fails""
    ]
  },
  ""details"": [
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/fails"",
      ""schemaLocation"": ""https://test.com/schema#/properties/fails"",
      ""instanceLocation"": ""/fails"",
      ""errors"": {
        """": ""All values fail against the false schema""
      }
    }
  ]
}";
		result.AssertInvalid(expected);
	}

	[Test]
	public void Hierarchical_Multi_Success()
	{
		var result = Validate("{\"multi\":8}", OutputFormat.Hierarchical);
		var expected = @"{
  ""valid"": true,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""annotations"": {
    ""properties"": [
      ""multi""
    ]
  },
  ""details"": [
    {
      ""valid"": true,
      ""evaluationPath"": ""/properties/multi"",
      ""schemaLocation"": ""https://test.com/schema#/properties/multi"",
      ""instanceLocation"": ""/multi"",
      ""details"": [
        {
          ""valid"": true,
          ""evaluationPath"": ""/properties/multi/allOf/0"",
          ""schemaLocation"": ""https://test.com/schema#/properties/multi/allOf/0"",
          ""instanceLocation"": ""/multi"",
          ""details"": [
            {
              ""valid"": true,
              ""evaluationPath"": ""/properties/multi/allOf/0/$ref"",
              ""schemaLocation"": ""https://test.com/schema#/$defs/integer"",
              ""instanceLocation"": ""/multi""
            }
          ]
        },
        {
          ""valid"": true,
          ""evaluationPath"": ""/properties/multi/allOf/1"",
          ""schemaLocation"": ""https://test.com/schema#/properties/multi/allOf/1"",
          ""instanceLocation"": ""/multi"",
          ""details"": [
            {
              ""valid"": true,
              ""evaluationPath"": ""/properties/multi/allOf/1/$ref"",
              ""schemaLocation"": ""https://test.com/schema#/$defs/minimum"",
              ""instanceLocation"": ""/multi""
            }
          ]
        }
      ]
    }
  ]
}";

		result.AssertValid(expected);
	}

	[Test]
	public void Hierarchical_Multi_Failure_Both()
	{
		var result = Validate("{\"multi\":3.5}", OutputFormat.Hierarchical);
		var expected = @"{
  ""valid"": false,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""details"": [
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/multi"",
      ""schemaLocation"": ""https://test.com/schema#/properties/multi"",
      ""instanceLocation"": ""/multi"",
      ""details"": [
        {
          ""valid"": false,
          ""evaluationPath"": ""/properties/multi/allOf/0"",
          ""schemaLocation"": ""https://test.com/schema#/properties/multi/allOf/0"",
          ""instanceLocation"": ""/multi"",
          ""details"": [
            {
              ""valid"": false,
              ""evaluationPath"": ""/properties/multi/allOf/0/$ref"",
              ""schemaLocation"": ""https://test.com/schema#/$defs/integer"",
              ""instanceLocation"": ""/multi"",
              ""errors"": {
                ""type"": ""Value is \""number\"" but should be \""integer\""""
              }
            }
          ]
        },
        {
          ""valid"": false,
          ""evaluationPath"": ""/properties/multi/allOf/1"",
          ""schemaLocation"": ""https://test.com/schema#/properties/multi/allOf/1"",
          ""instanceLocation"": ""/multi"",
          ""details"": [
            {
              ""valid"": false,
              ""evaluationPath"": ""/properties/multi/allOf/1/$ref"",
              ""schemaLocation"": ""https://test.com/schema#/$defs/minimum"",
              ""instanceLocation"": ""/multi"",
              ""errors"": {
                ""minimum"": ""3.5 should be at least 5""
              }
            }
          ]
        }
      ]
    }
  ]
}";

		result.AssertInvalid(expected);
	}

	[Test]
	public void Hierarchical_Multi_Failure_Integer()
	{
		var result = Validate("{\"fails\":8.5}", OutputFormat.Hierarchical);
		var expected = @"{
  ""valid"": false,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""details"": [
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/fails"",
      ""schemaLocation"": ""https://test.com/schema#/properties/fails"",
      ""instanceLocation"": ""/fails"",
      ""errors"": {
        """": ""All values fail against the false schema""
      }
    }
  ]
}";

		result.AssertInvalid(expected);
	}

	[Test]
	public void Hierarchical_Multi_Failure_Minimum()
	{
		var result = Validate("{\"fails\":3}", OutputFormat.Hierarchical);
		var expected = @"{
  ""valid"": false,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""details"": [
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/fails"",
      ""schemaLocation"": ""https://test.com/schema#/properties/fails"",
      ""instanceLocation"": ""/fails"",
      ""errors"": {
        """": ""All values fail against the false schema""
      }
    }
  ]
}";

		result.AssertInvalid(expected);
	}

	[Test]
	public void RelativeAndAbsoluteLocations()
	{
		var result = Validate("{\"refs\":8.8}", OutputFormat.Hierarchical);
		var expected = @"{
  ""valid"": false,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""details"": [
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/refs"",
      ""schemaLocation"": ""https://test.com/schema#/properties/refs"",
      ""instanceLocation"": ""/refs"",
      ""details"": [
        {
          ""valid"": false,
          ""evaluationPath"": ""/properties/refs/$ref"",
          ""schemaLocation"": ""https://test.com/schema#/$defs/integer"",
          ""instanceLocation"": ""/refs"",
          ""errors"": {
            ""type"": ""Value is \""number\"" but should be \""integer\""""
          }
        }
      ]
    }
  ]
}";

		result.AssertInvalid(expected);
	}

	private static EvaluationResults Validate(string json, OutputFormat format)
	{
		var instance = JsonNode.Parse(json);
		var options = EvaluationOptions.From(EvaluationOptions.Default);
		options.OutputFormat = format;

		var result = _schema.Evaluate(instance, options);
		return result;
	}

	[Test]
	public void AdditionalPropertiesDoesNotGiveExtraErrors()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Properties(
				("foo", false)
			)
			.AdditionalProperties(false);

		var instance = JsonNode.Parse("{\"foo\": null}");

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		var serialized = JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions);
		TestConsole.WriteLine(serialized);

		Assert.That(serialized, Does.Not.Contain("additionalProperties"));
	}

	[Test]
	public void UnevaluatedPropertiesDoesNotGiveExtraErrors()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Properties(
				("foo", false)
			)
			.UnevaluatedProperties(false);

		var instance = JsonNode.Parse("{\"foo\": null}");

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		var serialized = JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions);
		TestConsole.WriteLine(serialized);

		Assert.That(serialized, Does.Not.Contain("unevaluatedProperties"));
	}

	[Test]
	public void UnevaluatedPropertiesStillGivesExtraErrorsForReffedSchemas()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Defs(
				("reffed", new JsonSchemaBuilder()
					.Properties(
						("foo", false)
					)
				)
			)
			.Ref("#/$defs/reffed")
			.UnevaluatedProperties(false);

		var instance = JsonNode.Parse("{\"foo\": null}");

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		var serialized = JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions);
		TestConsole.WriteLine(serialized);

		Assert.That(serialized, Does.Contain("unevaluatedProperties"));
	}

	[Test]
	public void UnevaluatedPropertiesGivesCorrectInstanceLocation()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Properties(("foo", true))
			.UnevaluatedProperties(false);

		var instance = JsonNode.Parse("{\"foo\": null, \"bar\": null}");

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		var serialized = JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions);
		TestConsole.WriteLine(serialized);

		var unevaluatedPropertiesResult = result.Details.Single(x => x.EvaluationPath.Equals(JsonPointer.Create("unevaluatedProperties")));
		Assert.That(unevaluatedPropertiesResult.InstanceLocation.ToString(), Is.EqualTo("/bar"));
	}


	[Test]
	public void UnevaluatedItemsGivesCorrectInstanceLocation()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.PrefixItems(true)
			.UnevaluatedItems(false);

		var instance = JsonNode.Parse("[1, 2]");

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		var serialized = JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions);
		TestConsole.WriteLine(serialized);

		var unevaluatedPropertiesResult = result.Details.Single(x => x.EvaluationPath.Equals(JsonPointer.Create("unevaluatedItems")));
		Assert.That(unevaluatedPropertiesResult.InstanceLocation.ToString(), Is.EqualTo("/1"));
	}

	[Test]
	public void AdditionalItemsDoesNotGiveExtraErrors()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Items([true, false ])
			.AdditionalItems(false);

		var instance = JsonNode.Parse("[1,2]");

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		var serialized = JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions);
		TestConsole.WriteLine(serialized);

		Assert.That(serialized, Does.Not.Contain("additionalItems"));
	}

	[Test]
	public void UnevaluatedItemsDoesNotGiveExtraErrors()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft201909Id)
			.Items([true, false])
			.UnevaluatedItems(false);

		var instance = JsonNode.Parse("[1,2]");

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		var serialized = JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions);
		TestConsole.WriteLine(serialized);

		Assert.That(serialized, Does.Not.Contain("unevaluatedItems"));
	}

	[Test]
	[Ignore("Not sure about the URI for the false schema")]
	public void FalseSchemaFailsEverything()
	{
		JsonSchema schema = false;

		var instance = JsonDocument.Parse("[1,2]").RootElement;

		var result = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });

		var expected = @"{
  ""valid"": false,
  ""details"": [
    {
      ""valid"": false,
      ""evaluationPath"": """",
      ""schemaLocation"": ""https://json-schema.org/false#"",
      ""instanceLocation"": """",
      ""errors"": {
        """": ""All values fail against the false schema""
      }
    }
  ]
}";

		result.AssertInvalid(expected);
	}

	[Test]
	public void NewOutputFormat()
	{
		var schema = JsonSchema.FromText(@"{
  ""$id"": ""https://json-schema.org/schemas/example"",
  ""type"": ""object"",
  ""title"": ""root"",
  ""properties"": {
    ""foo"": {
      ""allOf"": [
        { ""required"": [ ""unspecified-prop"" ] },
        {
          ""type"": ""object"",
          ""title"": ""foo-title"",
          ""properties"": {
            ""foo-prop"": {
              ""const"": 1,
              ""title"": ""foo-prop-title""
            }
          },
          ""additionalProperties"": { ""type"": ""boolean"" }
        }
      ]
    },
    ""bar"": {
      ""$ref"": ""#/$defs/bar""
    }
  },
  ""$defs"": {
    ""bar"": {
      ""type"": ""object"",
      ""title"": ""bar-title"",
      ""properties"": {
        ""bar-prop"": {
          ""type"": ""integer"",
          ""minimum"": 10,
          ""title"": ""bar-prop-title""
        }
      }
    }
  }
}");
		var failing = JsonNode.Parse(@"{
  ""foo"": {""foo-prop"": ""not 1"", ""other-prop"": false},
  ""bar"": {""bar-prop"": 2}
}");
		var passing = JsonNode.Parse(@"{
  ""foo"": {
    ""foo-prop"": 1,
    ""unspecified-prop"": true
  },
  ""bar"": {""bar-prop"": 20}
}");

		var validationOptions = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		var result = schema.Evaluate(failing, validationOptions);

		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();

		result.ToList();
		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();

		result = schema.Evaluate(passing, validationOptions);

		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();

		result.ToList();
		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));
	}

	[Test]
	public void EtherCondensedOption()
	{
		var instance = JsonNode.Parse(@"{
  ""type"": ""object"",
  ""properties"": {
    ""id"": {
      ""type"": ""string""
    },
    ""product"": {
      ""type"": ""string""
    },
    ""field"": {
      ""type"": ""string""
    },
    ""value"": {
      ""type"": ""string""
    },
    ""created"": {
      ""type"": ""string""
    },
    ""updated"": {
      ""type"": ""string""
    },
    ""deleted"": {
      ""type"": ""string""
    },
    ""allOf"": [
      {
        ""if"": {
          ""required"": [
            ""value""
          ],
          ""properties"": {
            ""value"": {
              ""type"": ""string""
            }
          }
        },
        ""then"": {
          ""properties"": {
            ""value_blob"": {
              ""type"": ""null""
            }
          }
        }
      },
      {
        ""if"": {
          ""required"": [
            ""value_blob""
          ],
          ""properties"": {
            ""value_blob"": {
              ""type"": ""string""
            }
          }
        },
        ""then"": {
          ""properties"": {
            ""value"": {
              ""type"": ""null""
            }
          }
        }
      }
    ]
  }
}");

		var result = MetaSchemas.Draft202012.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		//result.ToBasic();

		TestConsole.WriteLine(JsonSerializer.Serialize(result, TestEnvironment.TestOutputSerializerOptions));
	}

	[Test]
	public void IgnoreTitleAnnotations()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Id("https://test.com/schema")
			.Title("a title")
			.Default("default value")
			.Type(SchemaValueType.String);
		JsonNode? instance = "a string";
		var expected = @"{
  ""valid"": true,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""annotations"": {
    ""default"": ""default value""
  }
}";
		var options = EvaluationOptions.From(EvaluationOptions.Default);
		options.OutputFormat = OutputFormat.Hierarchical;
		options.IgnoreAnnotationsFrom<TitleKeyword>();
		var result = schema.Evaluate(instance, options);

		result.AssertValid(expected);
	}

	[Test]
	public void CollectButDoNotReportPropertiesAnnotations()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Id("https://test.com/schema")
			.Title("a title")
			.Type(SchemaValueType.Object)
			.Properties(("foo", true))
			.AdditionalProperties(false);
		var instance = new JsonObject { ["foo"] = 1 };
		var expected = @"{
  ""valid"": true,
  ""evaluationPath"": """",
  ""schemaLocation"": ""https://test.com/schema#"",
  ""instanceLocation"": """",
  ""annotations"": {
    ""title"": ""a title"",
    ""additionalProperties"": []
  },
  ""details"": [
    {
      ""valid"": true,
      ""evaluationPath"": ""/properties/foo"",
      ""schemaLocation"": ""https://test.com/schema#/properties/foo"",
      ""instanceLocation"": ""/foo""
    }
  ]
}";
		var options = EvaluationOptions.From(EvaluationOptions.Default);
		options.OutputFormat = OutputFormat.Hierarchical;
		options.IgnoreAnnotationsFrom<PropertiesKeyword>();
		var result = schema.Evaluate(instance, options);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(JsonSerializer.Serialize((JsonNode?)instance, TestEnvironment.TestOutputSerializerOptions));
		TestConsole.WriteLine();

		result.AssertValid(expected);
	}
}