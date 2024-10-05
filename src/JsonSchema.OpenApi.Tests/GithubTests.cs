using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema.Tests;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.OpenApi.Tests;

public class GithubTests
{
	[Test]
	public void Issue388_DiscriminatorSerializationException()
	{
		var schemaAsJson =
			"""
			{
			  "type": "object",
			  "additionalProperties": false,
			  "required": [
			    "firstName",
			    "lastName",
			    "mailingAddress"
			  ],
			  "properties": {
			    "lastName": {
			      "type": "string",
			      "minLength": 0,
			      "maxLength": 64
			    },
			    "firstName": {
			      "maxLength": 64,
			      "type": "string"
			    },
			    "mailingAddress": {
			      "$ref": "#/components/schemas/mailingAddress"
			    }
			  },
			  "components": {
			    "schemas": {
			      "streetAddress": {
			        "allOf": [
			          {
			            "$ref": "#/components/schemas/address"
			          },
			          {
			            "type": "object",
			            "additionalProperties": false,
			            "required": [
			              "street",
			              "city",
			              "state",
			              "postalCode",
			              "country"
			            ],
			            "properties": {
			              "street": {
			                "maxLength": 128,
			                "type": "string"
			              },
			              "apartmentNumber": {
			                "type": "string",
			                "nullable": true,
			                "maxLength": 128
			              }
			            }
			          }
			        ]
			      },
			      "postOfficeAddress": {
			        "allOf": [
			          {
			            "$ref": "#/components/schemas/address"
			          },
			          {
			            "type": "object",
			            "additionalProperties": false,
			            "required": [
			              "poBox",
			              "city",
			              "state",
			              "postalCode",
			              "country"
			            ],
			            "properties": {
			              "poBox": {
			                "maxLength": 128,
			                "type": "string"
			              }
			            }
			          }
			        ]
			      },
			      "address": {
			        "type": "object",
			        "additionalProperties": false,
			        "required": [
			          "type",
			          "city",
			          "state",
			          "postalCode",
			          "country"
			        ],
			        "properties": {
			          "type": {
			            "enum": [
			              "street",
			              "postOffice"
			            ],
			            "type": "string"
			          },
			          "city": {
			            "maxLength": 128,
			            "type": "string"
			          },
			          "state": {
			            "maxLength": 128,
			            "type": "string"
			          },
			          "postalCode": {
			            "maxLength": 16,
			            "type": "string"
			          },
			          "country": {
			            "maxLength": 128,
			            "type": "string"
			          }
			        }
			      },
			      "mailingAddress": {
			        "discriminator": {
			          "mapping": {
			            "street": "#/components/schemas/streetAddress",
			            "postOffice": "#/components/schemas/postOfficeAddress"
			          },
			          "propertyName": "type"
			        },
			        "oneOf": [
			          {
			            "$ref": "#/components/schemas/streetAddress"
			          },
			          {
			            "$ref": "#/components/schemas/postOfficeAddress"
			          }
			        ]
			      }
			    }
			  }
			}
			""";

		var exampleAsJson = 
			"""
			{
			  "lastName": "Last",
			  "firstName": "First",
			  "mailingAddress": {
			    "postalCode": "12345",
			    "city": "City",
			    "country": "USA",
			    "street": "Street",
			    "state": "State",
			    "type": "street"
			  }
			}
			""";

		var schema = JsonSchema.FromText(schemaAsJson);
		var json = JsonNode.Parse(exampleAsJson);
		var options = new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical };
		var result = schema.Evaluate(json, options);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, TestEnvironment.SerializerOptions));

		result.AssertInvalid();
	}

	[Test]
	public void Issue720_DiscriminatorCtorFailing()
	{
		_ = new DiscriminatorKeyword("petType", null, null);
	}
}