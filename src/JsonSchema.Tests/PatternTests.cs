#if !NET481

using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class PatternTests
{
	// attempting to use the `.Pattern()` builder method with a bad regex doesn't compile, so... yea!

	[Test]
	public void DeserializingInvalidPatternThrows()
	{
		var schemaText =
			"""
			   {
			  "pattern": "(.{2}"
			}
			""";

		Assert.Throws<RegexParseException>(() => JsonSchema.FromText(schemaText));
	}

	[Test]
	public void DeserializingInvalidPatternPropertyThrows()
	{
		var schemaText =
			"""
			{
			  "patternProperties": {
				"(.{2}": true
			  }
			}
			""";

		Assert.Throws<RegexParseException>(() => JsonSchema.FromText(schemaText));
	}
}

#endif