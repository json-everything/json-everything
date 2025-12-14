using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Json.Schema;
using JsonSchema = Json.Schema.JsonSchema;
using NJsonSchemaImpl = NJsonSchema.JsonSchema;

namespace Json.Benchmarks.SchemaSuite;

[MemoryDiagnoser]
// [SimpleJob(RuntimeMoniker.Net80, baseline:true)]
// [SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class SingleSchemaRunner
{
	private static readonly JsonElement _schema = 
		JsonDocument.Parse("""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "$id": "https://api.example.com/schemas/order",
			  "title": "Order Schema",
			  "description": "Schema for a complete e-commerce order with comprehensive validation",
			  "type": "object",
			  "required": ["orderId", "customer", "items", "status"],
			  "properties": {
			    "orderId": {
			      "type": "string",
			      "pattern": "^ORD-[0-9]{8}$",
			      "description": "Unique order identifier"
			    },
			    "customer": {
			      "type": "object",
			      "required": ["id", "email", "type"],
			      "properties": {
			        "id": {
			          "type": "integer",
			          "minimum": 1,
			          "maximum": 9999999
			        },
			        "email": {
			          "type": "string",
			          "format": "email",
			          "minLength": 5,
			          "maxLength": 100
			        },
			        "name": {
			          "type": "string",
			          "minLength": 1,
			          "maxLength": 200
			        },
			        "type": {
			          "enum": ["individual", "business", "premium"],
			          "default": "individual"
			        },
			        "vipStatus": {
			          "type": "boolean"
			        },
			        "loyaltyPoints": {
			          "type": "number",
			          "minimum": 0,
			          "multipleOf": 0.5
			        }
			      },
			      "additionalProperties": false
			    },
			    "items": {
			      "type": "array",
			      "minItems": 1,
			      "maxItems": 100,
			      "uniqueItems": true,
			      "items": {
			        "type": "object",
			        "required": ["sku", "quantity", "price"],
			        "properties": {
			          "sku": {
			            "type": "string",
			            "pattern": "^[A-Z]{3}-[0-9]{5}$"
			          },
			          "name": {
			            "type": "string",
			            "minLength": 1,
			            "maxLength": 500
			          },
			          "quantity": {
			            "type": "integer",
			            "minimum": 1,
			            "exclusiveMaximum": 1000
			          },
			          "price": {
			            "type": "number",
			            "exclusiveMinimum": 0,
			            "maximum": 99999.99,
			            "multipleOf": 0.01
			          },
			          "category": {
			            "type": "string",
			            "enum": ["electronics", "books", "clothing", "food", "furniture"]
			          },
			          "tags": {
			            "type": "array",
			            "items": { "type": "string" },
			            "maxItems": 10
			          },
			          "inStock": {
			            "type": "boolean"
			          }
			        }
			      },
			      "contains": {
			        "type": "object",
			        "required": ["inStock"],
			        "properties": {
			          "inStock": { "const": true }
			        }
			      },
			      "minContains": 1
			    },
			    "shipping": {
			      "type": "object",
			      "required": ["method", "address"],
			      "properties": {
			        "method": {
			          "type": "string",
			          "enum": ["standard", "express", "overnight", "international"]
			        },
			        "address": {
			          "type": "object",
			          "required": ["street", "city", "country", "postalCode"],
			          "properties": {
			            "street": { "type": "string", "minLength": 1 },
			            "city": { "type": "string", "minLength": 1 },
			            "state": { "type": "string", "pattern": "^[A-Z]{2}$" },
			            "country": { "type": "string", "pattern": "^[A-Z]{2}$" },
			            "postalCode": { "type": "string" }
			          }
			        },
			        "trackingNumber": {
			          "type": ["string", "null"],
			          "pattern": "^[A-Z0-9]{10,20}$"
			        },
			        "estimatedDays": {
			          "type": "integer",
			          "minimum": 1,
			          "maximum": 30
			        }
			      }
			    },
			    "payment": {
			      "type": "object",
			      "oneOf": [
			        {
			          "properties": {
			            "method": { "const": "credit_card" },
			            "cardLast4": { "type": "string", "pattern": "^[0-9]{4}$" },
			            "cardType": { "enum": ["visa", "mastercard", "amex"] }
			          },
			          "required": ["method", "cardLast4", "cardType"]
			        },
			        {
			          "properties": {
			            "method": { "const": "paypal" },
			            "paypalEmail": { "type": "string", "format": "email" }
			          },
			          "required": ["method", "paypalEmail"]
			        },
			        {
			          "properties": {
			            "method": { "const": "bank_transfer" },
			            "bankAccount": { "type": "string", "pattern": "^[0-9]{8,12}$" }
			          },
			          "required": ["method", "bankAccount"]
			        }
			      ]
			    },
			    "totals": {
			      "type": "object",
			      "required": ["subtotal", "tax", "total"],
			      "properties": {
			        "subtotal": { "type": "number", "minimum": 0 },
			        "tax": { "type": "number", "minimum": 0 },
			        "shipping": { "type": "number", "minimum": 0 },
			        "discount": { "type": "number", "minimum": 0 },
			        "total": { "type": "number", "minimum": 0 }
			      }
			    },
			    "status": {
			      "type": "string",
			      "enum": ["pending", "processing", "shipped", "delivered", "cancelled"]
			    },
			    "notes": {
			      "type": "string",
			      "maxLength": 1000
			    },
			    "metadata": {
			      "type": "object",
			      "properties": {
			        "createdAt": { "type": "string", "format": "date-time" },
			        "updatedAt": { "type": "string", "format": "date-time" },
			        "source": { "type": "string" },
			        "version": { "type": "integer", "minimum": 1 }
			      },
			      "patternProperties": {
			        "^x-": { "type": "string" },
			        "^internal-": {}
			      },
			      "additionalProperties": false
			    }
			  },
			  "allOf": [
			    {
			      "if": {
			        "properties": {
			          "customer": {
			            "properties": {
			              "type": { "const": "business" }
			            }
			          }
			        }
			      },
			      "then": {
			        "properties": {
			          "customer": {
			            "required": ["name"],
			            "properties": {
			              "taxId": { "type": "string", "pattern": "^[0-9]{9}$" }
			            }
			          }
			        }
			      }
			    },
			    {
			      "if": {
			        "properties": {
			          "totals": {
			            "properties": {
			              "total": { "minimum": 100 }
			            }
			          }
			        }
			      },
			      "then": {
			        "properties": {
			          "shipping": {
			            "properties": {
			              "method": { "enum": ["express", "overnight", "international"] }
			            }
			          }
			        }
			      },
			      "else": {
			        "properties": {
			          "shipping": {
			            "properties": {
			              "method": { "enum": ["standard", "express"] }
			            }
			          }
			        }
			      }
			    }
			  ],
			  "anyOf": [
			    { "required": ["shipping"] },
			    { "required": ["payment"] }
			  ],
			  "dependentSchemas": {
			    "shipping": {
			      "properties": {
			        "totals": {
			          "required": ["shipping"]
			        }
			      }
			    }
			  },
			  "dependentRequired": {
			    "notes": ["status"]
			  },
			  "unevaluatedProperties": false
			}
			""").RootElement;

	private static readonly JsonElement _instance =
		JsonDocument.Parse("""
			{
			  "orderId": "ORD-20231215",
			  "customer": {
			    "id": 42857,
			    "email": "john.smith@example.com",
			    "name": "John Smith",
			    "type": "business",
			    "vipStatus": true,
			    "loyaltyPoints": 1250.5,
			    "taxId": "123456789"
			  },
			  "items": [
			    {
			      "sku": "ELC-10001",
			      "name": "Wireless Bluetooth Headphones",
			      "quantity": 2,
			      "price": 79.99,
			      "category": "electronics",
			      "tags": ["audio", "wireless", "bestseller"],
			      "inStock": true
			    },
			    {
			      "sku": "BOK-20045",
			      "name": "The Complete Guide to JSON Schema",
			      "quantity": 1,
			      "price": 49.95,
			      "category": "books",
			      "tags": ["technical", "programming"],
			      "inStock": true
			    },
			    {
			      "sku": "CLO-30012",
			      "name": "Premium Cotton T-Shirt",
			      "quantity": 3,
			      "price": 24.99,
			      "category": "clothing",
			      "tags": ["casual", "cotton"],
			      "inStock": false
			    }
			  ],
			  "shipping": {
			    "method": "express",
			    "address": {
			      "street": "123 Main Street",
			      "city": "Portland",
			      "state": "OR",
			      "country": "US",
			      "postalCode": "97201"
			    },
			    "trackingNumber": "1Z999AA10123456784",
			    "estimatedDays": 3
			  },
			  "payment": {
			    "method": "credit_card",
			    "cardLast4": "4242",
			    "cardType": "visa"
			  },
			  "totals": {
			    "subtotal": 234.92,
			    "tax": 21.14,
			    "shipping": 15.00,
			    "discount": 23.49,
			    "total": 247.57
			  },
			  "status": "processing",
			  "notes": "Please include gift wrapping for all items",
			  "metadata": {
			    "createdAt": "2023-12-15T10:30:00Z",
			    "updatedAt": "2023-12-15T14:22:00Z",
			    "source": "web",
			    "version": 2,
			    "x-correlation-id": "abc123xyz",
			    "x-user-agent": "Mozilla/5.0",
			    "internal-processing-queue": "high-priority"
			  }
			}
			""").RootElement;

	private static readonly BuildOptions _buildOptions = new()
	{
		Dialect = Dialect.Draft201909,
	};

	private static readonly EvaluationOptions _evaluationOptions = new()
	{
		OutputFormat = OutputFormat.Flag
	};

	private static readonly JsonSchema _builtJsonSchemaNetSchema = BuildJsonSchemaNetSchema();
	private static readonly NJsonSchemaImpl _builtNJsonSchema = BuildNJsonSchema();

	private static JsonSchema BuildJsonSchemaNetSchema()
	{
		var options = new BuildOptions { Dialect = Dialect.Draft201909, SchemaRegistry = new() };
		return JsonSchema.Build(_schema, options);
	}

	private static NJsonSchemaImpl BuildNJsonSchema()
	{
		var schemaJson = _schema.GetRawText();
		return NJsonSchemaImpl.FromJsonAsync(schemaJson).Result;
	}

	//[Benchmark]
	//[Arguments(10)]
	//[Arguments(100)]
	//[Arguments(1000)]
	public int JsonSchemaNetBuildOnly(int n)
	{
		for (int j = 0; j < n; j++)
		{
			_buildOptions.SchemaRegistry = new();
			_ = JsonSchema.Build(_schema, _buildOptions);
		}

		return n;
	}

	//[Benchmark]
	//[Arguments(10)]
	//[Arguments(100)]
	//[Arguments(1000)]
	public int NJsonSchemaBuildOnly(int n)
	{
		var schemaJson = _schema.GetRawText();
		for (int j = 0; j < n; j++)
		{
			_ = NJsonSchemaImpl.FromJsonAsync(schemaJson).Result;
		}

		return n;
	}

	//[Benchmark]
	//[Arguments(10)]
	//[Arguments(100)]
	//[Arguments(1000)]
	public int JsonSchemaNetEvaluateOnly(int n)
	{
		for (int j = 0; j < n; j++)
		{
			_ = _builtJsonSchemaNetSchema.Evaluate(_instance, _evaluationOptions);
		}

		return n;
	}

	//[Benchmark]
	//[Arguments(10)]
	//[Arguments(100)]
	//[Arguments(1000)]
	public int NJsonSchemaEvaluateOnly(int n)
	{
		var instanceJson = _instance.GetRawText();
		for (int j = 0; j < n; j++)
		{
			_ = _builtNJsonSchema.Validate(instanceJson);
		}

		return n;
	}

	[Benchmark]
	//[Arguments(10)]
	//[Arguments(100)]
	//[Arguments(1000)]
	[Arguments(100000)]
	public int JsonSchemaNet(int n)
	{
		_buildOptions.SchemaRegistry = new();
		var schema = JsonSchema.Build(_schema, _buildOptions);
		for (int j = 0; j < n; j++)
		{
			_ = schema.Evaluate(_instance, _evaluationOptions);
		}

		return n;
	}

	[Benchmark]
	//[Arguments(10)]
	//[Arguments(100)]
	//[Arguments(1000)]
	[Arguments(100000)]
	public int NJsonSchema(int n)
	{
		var instanceJson = _instance.GetRawText();
		var schemaJson = _schema.GetRawText();
		var schema = NJsonSchemaImpl.FromJsonAsync(schemaJson).Result;
		for (int j = 0; j < n; j++)
		{
			_ = schema.Validate(instanceJson);
		}

		return n;
	}
}