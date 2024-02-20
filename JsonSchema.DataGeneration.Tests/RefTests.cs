using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

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
}