using System;
using System.Text.Json;
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

#if NET481
		Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize(schemaText, TestSerializerContext.Default.JsonSchema));
#else
		Assert.Throws<RegexParseException>(() => JsonSerializer.Deserialize(schemaText, TestSerializerContext.Default.JsonSchema));
#endif
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


#if NET481
		Assert.Throws<ArgumentException>(() => JsonSerializer.Deserialize(schemaText, TestSerializerContext.Default.JsonSchema));
#else
		Assert.Throws<RegexParseException>(() => JsonSerializer.Deserialize(schemaText, TestSerializerContext.Default.JsonSchema));
#endif
	}
}