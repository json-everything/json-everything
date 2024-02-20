using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;
using NotSupportedException = System.NotSupportedException;

namespace Json.Schema.DataGeneration.Tests;

internal class RefTests
{
	[Test]
	public void PointerRef()
	{
		var schema = JsonSchema.FromText(
			"""
			{
			    "type": "array",
			    "items": { "$ref": "#/$defs/positiveInteger" },
			    "$defs": {
			        "positiveInteger": {
			            "type": "integer",
			            "exclusiveMinimum": 0
			        }
			    },
			    "minItems": 2,
			    "additionalProperties" : false
			}
			""");

		Run(schema);
	}

	[Test]
	public void ExternalRef()
	{
		var schema = JsonSchema.FromText(
			"""
			{
			    "type": "array",
			    "items": { "$ref": "https://json-everything.test/foo" },
			    "$defs": {
			        "positiveInteger": {
			            "type": "integer",
			            "exclusiveMinimum": 0
			        }
			    },
			    "minItems": 2,
			    "additionalProperties" : false
			}
			""");

		Assert.Throws<NotSupportedException>(() => schema.GenerateData());
	}
}