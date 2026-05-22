using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;
using static Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels;

namespace Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration;

public class DataAnnotationsSourceGeneratorTests
{
	[Test]
	public void PersonWithMaxLength_GeneratesMaxLengthConstraint()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithMaxLength",
		  "type": "object",
		  "properties": {
		    "Name": {
		      "type": "string",
		      "maxLength": 50
		    },
		    "Age": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithMaxLength;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithMinLength_GeneratesMinLengthConstraint()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithMinLength",
		  "type": "object",
		  "properties": {
		    "Name": {
		      "type": "string",
		      "minLength": 2
		    },
		    "Age": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithMinLength;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithStringLength_GeneratesBothLengthConstraints()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithStringLength",
		  "type": "object",
		  "properties": {
		    "Name": {
		      "type": "string",
		      "minLength": 2,
		      "maxLength": 50
		    },
		    "Age": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithStringLength;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithRange_GeneratesMinAndMaxConstraints()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithRange",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Age": {
		      "type": "integer",
		      "minimum": 0,
		      "maximum": 120
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithRange;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithRegex_GeneratesPatternConstraint()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithRegex",
		  "type": "object",
		  "properties": {
		    "SSN": {
		      "type": "string",
		      "pattern": "^\\d{3}-\\d{2}-\\d{4}$"
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithRegex;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithEmail_GeneratesEmailFormat()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithEmail",
		  "type": "object",
		  "properties": {
		    "Email": {
		      "type": "string",
		      "format": "email"
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithEmail;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithUrl_GeneratesUriFormat()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithUrl",
		  "type": "object",
		  "properties": {
		    "Website": {
		      "type": "string",
		      "format": "uri"
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithUrl;
		
		AssertEqual(expected, actual);
	}

#if NET8_0_OR_GREATER
	[Test]
	public void PersonWithLength_GeneratesBothLengthConstraints()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithLength",
		  "type": "object",
		  "properties": {
		    "Name": {
		      "type": "string",
		      "minLength": 2,
		      "maxLength": 50
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithLength;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithBase64_GeneratesBase64Format()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithBase64",
		  "type": "object",
		  "properties": {
		    "Image": {
		      "type": "string",
		      "format": "base64"
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithBase64;
		
		AssertEqual(expected, actual);
	}
#endif

	[Test]
	public void PersonWithMultipleConstraints_GeneratesAllConstraints()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "urn:jsonschema:Json.Schema.Generation.DataAnnotations.Tests.SourceGeneration.DataAnnotationsTestModels.PersonWithMultipleConstraints",
		  "type": "object",
		  "properties": {
		    "Name": {
		      "type": "string",
		      "minLength": 2,
		      "maxLength": 50,
		      "pattern": "^[A-Za-z\\s]+$"
		    },
		    "Age": {
		      "type": "integer",
		      "minimum": 18,
		      "maximum": 120
		    },
		    "Email": {
		      "type": "string",
		      "format": "email"
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.DataAnnotationsTestModels_PersonWithMultipleConstraints;
		
		AssertEqual(expected, actual);
	}
}
