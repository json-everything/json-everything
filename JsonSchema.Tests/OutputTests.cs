using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

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
		var result = Validate("{\"passes\":\"value\"}", OutputFormat.Basic);
		var expected = @"{
  ""valid"": true,
  ""nested"": [
    {
      ""valid"": true,
      ""evaluationPath"": """",
      ""schemaLocation"": ""https://test.com/schema#"",
      ""instanceLocation"": """",
      ""annotations"": {
        ""properties"": [ ""passes"" ]
      }
    }
  ]
}";

		result.AssertValid(expected);
	}

	[Test]
	public void Basic_Failure()
	{
		var result = Validate("{\"fails\":\"value\"}", OutputFormat.Basic);
		var expected = @"{
  ""valid"": false,
  ""nested"": [
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
  ""nested"": [
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
  ""nested"": [
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
  ""nested"": [
    {
      ""valid"": true,
      ""evaluationPath"": ""/properties/multi"",
      ""schemaLocation"": ""https://test.com/schema#/properties/multi"",
      ""instanceLocation"": ""/multi"",
      ""nested"": [
        {
          ""valid"": true,
          ""evaluationPath"": ""/properties/multi/allOf/0"",
          ""schemaLocation"": ""https://test.com/schema#/properties/multi/allOf/0"",
          ""instanceLocation"": ""/multi"",
          ""nested"": [
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
          ""schemaLocation"": ""https://test.com/schema#/$defs/integer/properties/multi/allOf/1"",
          ""instanceLocation"": ""/multi"",
          ""nested"": [
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
  ""nested"": [
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/multi"",
      ""schemaLocation"": ""https://test.com/schema#/properties/multi"",
      ""instanceLocation"": ""/multi"",
      ""nested"": [
        {
          ""valid"": false,
          ""evaluationPath"": ""/properties/multi/allOf/0"",
          ""schemaLocation"": ""https://test.com/schema#/properties/multi/allOf/0"",
          ""instanceLocation"": ""/multi"",
          ""nested"": [
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
          ""schemaLocation"": ""https://test.com/schema#/$defs/integer/properties/multi/allOf/1"",
          ""instanceLocation"": ""/multi"",
          ""nested"": [
            {
              ""valid"": false,
              ""evaluationPath"": ""/properties/multi/allOf/1/$ref"",
              ""schemaLocation"": ""https://test.com/schema#/$defs/minimum"",
              ""instanceLocation"": ""/multi"",
              ""errors"": {
                ""minimum"": ""3.5 is less than or equal to 5""
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
  ""nested"": [
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
  ""nested"": [
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
  ""nested"": [
    {
      ""valid"": false,
      ""evaluationPath"": ""/properties/refs"",
      ""schemaLocation"": ""https://test.com/schema#/properties/refs"",
      ""instanceLocation"": ""/refs"",
      ""nested"": [
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

		private static ValidationResults Validate(string json, OutputFormat format)
		{
			var instance = JsonNode.Parse(json);
			var options = ValidationOptions.From(ValidationOptions.Default);
			options.OutputFormat = format;

			var result = _schema.Validate(instance, options);
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

		var result = schema.Validate(instance, new ValidationOptions { OutputFormat = OutputFormat.Basic });

		var serialized = JsonSerializer.Serialize(result);
		Console.WriteLine(serialized);

		Assert.False(serialized.Contains("additionalProperties"));
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

		var result = schema.Validate(instance, new ValidationOptions { OutputFormat = OutputFormat.Basic });

		var serialized = JsonSerializer.Serialize(result);
		Console.WriteLine(serialized);

		Assert.False(serialized.Contains("unevaluatedProperties"));
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

		var result = schema.Validate(instance, new ValidationOptions { OutputFormat = OutputFormat.Basic });

		var serialized = JsonSerializer.Serialize(result);
		Console.WriteLine(serialized);

		Assert.True(serialized.Contains("unevaluatedProperties"));
	}

	[Test]
	public void AdditionalItemsDoesNotGiveExtraErrors()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Items(new JsonSchema[] { true, false })
			.AdditionalItems(false);

			var instance = JsonNode.Parse("[1,2]");

		var result = schema.Validate(instance, new ValidationOptions { OutputFormat = OutputFormat.Basic });

		var serialized = JsonSerializer.Serialize(result);
		Console.WriteLine(serialized);

		Assert.False(serialized.Contains("additionalItems"));
	}

	[Test]
	public void UnevaluatedItemsDoesNotGiveExtraErrors()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Items(new JsonSchema[] { true, false })
			.UnevaluatedItems(false);

			var instance = JsonNode.Parse("[1,2]");

		var result = schema.Validate(instance, new ValidationOptions { OutputFormat = OutputFormat.Basic });

		var serialized = JsonSerializer.Serialize(result);
		Console.WriteLine(serialized);

		Assert.False(serialized.Contains("unevaluatedItems"));
	}

	[Test]
	public void FalseSchemaFailsEverything()
	{
		JsonSchema schema = false;

		var instance = JsonDocument.Parse("[1,2]").RootElement;

		var result = schema.Validate(instance, new ValidationOptions { OutputFormat = OutputFormat.Basic });

		var expected = @"{
  ""valid"": false,
  ""nested"": [
    {
      ""valid"": false,
      ""evaluationPath"": """",
      ""schemaLocation"": ""https://json-everything/base#"",
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

		var validationOptions = new ValidationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};
		var result = schema.Validate(failing, validationOptions);

		var serializerOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));
		Console.WriteLine();

		result.ToBasic();
		Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));
		Console.WriteLine();

		result = schema.Validate(passing, validationOptions);

		Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));
		Console.WriteLine();

		result.ToBasic();
		Console.WriteLine(JsonSerializer.Serialize(result, serializerOptions));
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

		ValidationOptions.Default.Log = null!;
		var result = MetaSchemas.Draft202012.Validate(instance, new ValidationOptions { OutputFormat = OutputFormat.Hierarchical });

		//result.ToBasic();

		Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		}));
	}
}