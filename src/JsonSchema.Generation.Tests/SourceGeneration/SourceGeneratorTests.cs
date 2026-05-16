using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Json.Schema.Generation.Tests.AssertionExtensions;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Json.Schema.Generation.Tests.SourceGeneration;

public class SourceGeneratorTests
{
	[Test]
	public void SimplePerson_GeneratesSchema()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.SimplePerson",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Age": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_SimplePerson;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void CamelCasePerson_UsesCamelCase()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.CamelCasePerson",
		  "type": "object",
		  "properties": {
		    "firstName": { "type": "string" },
		    "lastName": { "type": "string" },
		    "age": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_CamelCasePerson;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void NestedTypeOfCamelCaseModel_UsesCamelCase()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.NestedType",
		  "type": "object",
		  "properties": {
		    "externalId": { "type": "string" },
		    "displayName": { "type": "string" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_NestedType;

		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithNullable_AllowsNull()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithNullable",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Email": { "type": ["null", "string"] },
		    "Age": { "type": ["null", "integer"] }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithNullable;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithRequired_MarksRequired()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithRequired",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Age": { "type": "integer" }
		  },
		  "required": ["Name"]
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithRequired;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithEnum_HasEnumValues()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithEnum",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Status": { "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.Status" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithEnum;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithNullableEnumArray_AllowsNull()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithNullableEnumArray",
		  "type": "object",
		  "properties": {
		    "styles": {
		      "type": ["null", "array"],
		      "items": {
		        "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ContentStyle"
		      }
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithNullableEnumArray;

		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithDescription_HasDescriptions()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithDescription",
		  "type": "object",
		  "properties": {
		    "Name": {
		      "type": "string",
		      "description": "The person's full name"
		    },
		    "Age": {
		      "type": "integer",
		      "description": "The person's age in years"
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithDescription;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void ProductWithCustomAttributes_AppliesCustomEmitters()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ProductWithCustomAttributes",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Price": {
		      "type": "number",
		      "minimum": 0,
		      "exclusiveMinimum": 0
		    },
		    "DiscountPercentage": {
		      "type": "integer",
		      "minimum": 0,
		      "maximum": 100
		    },
		    "Description": { "type": ["null", "string"] }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ProductWithCustomAttributes;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithGuidArrayAndMinItems_EmitsArrayOfUuid()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithGuidArrayAndMinItems",
		  "type": "object",
		  "properties": {
		    "recipientIds": {
		      "type": "array",
		      "items": {
		        "type": "string",
		        "format": "uuid"
		      },
		      "minItems": 1
		    }
		  },
		  "required": ["recipientIds"]
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithGuidArrayAndMinItems;

		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithAddresses_UsesDefsAndRefs()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithAddresses",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "HomeAddress": {
		      "description": "Home address",
		      "anyOf": [
		        { "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.Address" },
		        { "type": "null" }
		      ]
		    },
		    "WorkAddress": {
		      "description": "Work address",
		      "anyOf": [
		        { "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.Address" },
		        { "type": "null" }
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithAddresses;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithId_UsesCustomId()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "https://json-everything.test/schemas/person",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Age": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithId;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithIdReference_UsesCustomIdInRef()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithIdReference",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Person": {
		      "anyOf": [
		        { "$ref": "https://json-everything.test/schemas/person" },
		        { "type": "null" }
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithIdReference;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithJsonRequired_MarksRequired()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithJsonRequired",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Age": { "type": "integer" }
		  },
		  "required": ["Age"]
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithJsonRequired;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithDefaults_EmitsDefaultValues()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithDefaults",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string", "default": "anonymous" },
		    "Age": { "type": "integer", "default": 0 },
		    "IsActive": { "type": "boolean", "default": true }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithDefaults;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithNoAdditionalProperties_EmitsAdditionalProperties()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.PersonWithNoAdditionalProperties",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Age": { "type": "integer" }
		  },
		  "additionalProperties": false
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_PersonWithNoAdditionalProperties;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithMultipleClosedGenerics_UsesDistinctRefs()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithMultipleClosedGenerics",
		  "type": "object",
		  "properties": {
		    "IntHolder": {
		      "anyOf": [
		        { "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.GenericHolder<int>" },
		        { "type": "null" }
		      ]
		    },
		    "StringHolder": {
		      "anyOf": [
		        { "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.GenericHolder<string>" },
		        { "type": "null" }
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithMultipleClosedGenerics;

		AssertEqual(expected, actual);
	}

	[Test]
	public void GenericHolderInt_GeneratesSchema()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.GenericHolder<int>",
		  "type": "object",
		  "properties": {
		    "Value": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_GenericHolderOfInt32;

		AssertEqual(expected, actual);
	}

	[Test]
	public void GenericHolderString_GeneratesSchema()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.GenericHolder<string>",
		  "type": "object",
		  "properties": {
		    "Value": { "type": "string" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_GenericHolderOfString;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithOptionalWrapper_UsesSchemaHandlerForWrappedType()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithOptionalWrapper",
		  "type": "object",
		  "properties": {
		    "Age": { 
		      "anyOf":[
		        {"type":"integer"},
		        {"type":"null"}
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithOptionalWrapper;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithOptionalObjectWrapper_UsesRefForGeneratedType()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithOptionalObjectWrapper",
		  "type": "object",
		  "properties": {
		    "Person": {
		      "anyOf":[
		        {"$ref":"global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.SimplePerson"},
		        {"type":"null"}
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithOptionalObjectWrapper;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithOptionalCollections_ContinuesWithGenerated()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithOptionalCollections",
		  "type": "object",
		  "properties": {
		    "ValueA": {
		      "anyOf":[
		        {"$ref":"global::System.Collections.Generic.IEnumerable<int>"},
		        {"type":"null"}
		      ]
		    },
		    "ValueB": {
		      "anyOf":[
		        {"$ref":"global::System.Collections.Generic.IEnumerable<int>"},
		        {"type":"null"}
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithOptionalCollections;
		var enumerableParameter = GeneratedJsonSchemas.IEnumerableOfInt32;
		var arrayParameter = GeneratedJsonSchemas.Int32Array;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithOptionalDictionary_ContinuesWithGenerated()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithOptionalDictionary",
		  "type": "object",
		  "properties": {
		    "Data": {
		      "anyOf":[
		        {"$ref":"global::System.Collections.Generic.Dictionary<string, int>"},
		        {"type":"null"}
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithOptionalDictionary;
		var dictionaryParameter = GeneratedJsonSchemas.DictionaryOfStringAndInt32;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithOptionalAdditionalCollections_ContinuesWithGenerated()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithOptionalAdditionalCollections",
		  "type": "object",
		  "properties": {
		    "ValueA": {
		      "anyOf":[
		        {"$ref":"global::System.Collections.Generic.IEnumerable<int>"},
		        {"type":"null"}
		      ]
		    },
		    "ValueB": {
		      "anyOf":[
		        {"$ref":"global::System.Collections.Generic.IEnumerable<int>"},
		        {"type":"null"}
		      ]
		    },
		    "ValueC": {
		      "anyOf":[
		        {"$ref":"global::System.Collections.Generic.IEnumerable<int>"},
		        {"type":"null"}
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithOptionalAdditionalCollections;
		var hashSetParameter = GeneratedJsonSchemas.HashSetOfInt32;
		var queueParameter = GeneratedJsonSchemas.QueueOfInt32;
		var readOnlyCollectionParameter = GeneratedJsonSchemas.IReadOnlyCollectionOfInt32;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithOptionalUngeneratedType_ContinuesWithGenerated()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithOptionalUngeneratedType",
		  "type": "object",
		  "properties": {
		    "Ungenerated": {
		      "anyOf":[
		        {"$ref":"global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.UngeneratedType"},
		        {"type":"null"}
		      ]
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithOptionalUngeneratedType;
		var parameter = GeneratedJsonSchemas.TestModels_UngeneratedType;

		AssertEqual(expected, actual);
	}

	[Test]
	public void BuildForType_UsesSchemaHandlerForOpenGenericType()
	{
		var expectedJson = """
		{
		  "anyOf":[
		    {"type":"string"},
		    {"type":"null"}
		  ]
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = new JsonSchemaBuilder()
			.BuildForType(typeof(TestModels.Optional<string>))
			.Build();

		AssertEqual(expected, actual);
	}

	[Test]
	public void BuildForType_Array_UsesGeneratedSchema()
	{
		var expectedJson = """
		{
		  "$ref": "global::System.Collections.Generic.IEnumerable<int>"
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = new JsonSchemaBuilder()
			.BuildForType(typeof(int[]))
			.Build();
		var parameter = GeneratedJsonSchemas.IEnumerableOfInt32;

		AssertEqual(expected, actual);
	}

	[Test]
	public void BuildForType_Dictionary_UsesGeneratedSchema()
	{
		var expectedJson = """
		{
		  "$ref": "global::System.Collections.Generic.Dictionary<string, int>"
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = new JsonSchemaBuilder()
			.BuildForType(typeof(Dictionary<string, int>))
			.Build();
		var parameter = GeneratedJsonSchemas.DictionaryOfStringAndInt32;

		AssertEqual(expected, actual);
	}

	[Test]
	public void BuildForType_NullableGuid_EmitsNullableStringUuid()
	{
		var expectedJson = """
		{
		  "type": ["null", "string"],
		  "format": "uuid"
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = new JsonSchemaBuilder()
			.BuildForType(typeof(Guid?))
			.Build();

		AssertEqual(expected, actual);
	}

	[Test]
	public void BuildForType_UsesSchemaForGeneratedTypeInAnotherNamespace()
	{
		var expectedJson = """
		{
		  "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.AlternateNamespace.AlternateModels.CrossNamespacePerson"
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = new JsonSchemaBuilder()
			.BuildForType(typeof(AlternateNamespace.AlternateModels.CrossNamespacePerson))
			.Build();

		AssertEqual(expected, actual);
	}

	[Test]
	public void CentralGeneratedSchemasClass_ContainsSchemasFromAnotherNamespace()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.AlternateNamespace.AlternateModels.CrossNamespacePerson",
		  "type": "object",
		  "properties": {
		    "Name": { "type": "string" },
		    "Age": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.AlternateModels_CrossNamespacePerson;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ConflictingNames_AreDisambiguatedWithNamespacePrefix()
	{
		// Both TestModels.ConflictModel classes produce the same raw SchemaPropertyName
		// ("TestModels_ConflictModel"). The generator must prefix each with its
		// relative namespace so both can coexist in GeneratedJsonSchemas.
		var expectedMain = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ConflictModel",
		  "type": "object",
		  "properties": {
		    "Value": { "type": "string" }
		  }
		}
		""";
		var expectedAlt = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.AlternateNamespace.TestModels.ConflictModel",
		  "type": "object",
		  "properties": {
		    "Count": { "type": "integer" }
		  }
		}
		""";

		var registry = new SchemaRegistry();
		AssertEqual(
			JsonSchema.FromText(expectedMain, new BuildOptions { SchemaRegistry = registry }),
			GeneratedJsonSchemas.SourceGeneration_TestModels_ConflictModel);
		AssertEqual(
			JsonSchema.FromText(expectedAlt, new BuildOptions { SchemaRegistry = registry }),
			GeneratedJsonSchemas.SourceGeneration_AlternateNamespace_TestModels_ConflictModel);
	}

	[Test]
	public void NullableGenericArguments_GenerateDistinctSchemaMembers()
	{
		Assert.That(GeneratedJsonSchemas.TestModels_OptionalOfGuid, Is.Not.Null);
		Assert.That(GeneratedJsonSchemas.TestModels_OptionalOfGuidNullable, Is.Not.Null);
	}

	[Test]
	public void GeneratedSchemaForHandledWrapper_UsesHandlerOutput()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.Optional<int>",
		  "anyOf":[
		    {"type":"integer"},
		    {"type":"null"}
		  ]
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_OptionalOfInt32;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithBuiltInJsonTypes_UsesExpectedSchemas()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithBuiltInJsonTypes",
		  "type": "object",
		  "properties": {
		    "Document": {},
		    "Element": {},
		    "Node": {},
		    "Value": {},
		    "Object": { "type": "object" },
		    "Array": { "type": "array" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithBuiltInJsonTypes;

		AssertEqual(expected, actual);
	}

	[Test]
	public void BuildForType_MapsBuiltInJsonTypes()
	{
		var anySchema = JsonSchema.FromText("{}", new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var objectSchema = JsonSchema.FromText("""{ "type": "object" }""", new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var arraySchema = JsonSchema.FromText("""{ "type": "array" }""", new BuildOptions { SchemaRegistry = new SchemaRegistry() });

		AssertEqual(anySchema, new JsonSchemaBuilder().BuildForType(typeof(System.Text.Json.JsonDocument)).Build());
		AssertEqual(anySchema, new JsonSchemaBuilder().BuildForType(typeof(System.Text.Json.JsonElement)).Build());
		AssertEqual(anySchema, new JsonSchemaBuilder().BuildForType(typeof(System.Text.Json.Nodes.JsonNode)).Build());
		AssertEqual(anySchema, new JsonSchemaBuilder().BuildForType(typeof(System.Text.Json.Nodes.JsonValue)).Build());
		AssertEqual(objectSchema, new JsonSchemaBuilder().BuildForType(typeof(System.Text.Json.Nodes.JsonObject)).Build());
		AssertEqual(arraySchema, new JsonSchemaBuilder().BuildForType(typeof(System.Text.Json.Nodes.JsonArray)).Build());
	}

	[Test]
	public void ModelWithStringKeyDictionary_UsesAdditionalProperties()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithStringKeyDictionary",
		  "type": "object",
		  "properties": {
		    "Data": {
		      "type": "object",
		      "additionalProperties": { "type": "integer" }
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithStringKeyDictionary;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithEnumKeyDictionary_UsesPropertyNamesAndAdditionalProperties()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithEnumKeyDictionary",
		  "type": "object",
		  "properties": {
		    "Flags": {
		      "type": "object",
		      "propertyNames": { "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.Status" },
		      "additionalProperties": { "type": "boolean" }
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithEnumKeyDictionary;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ModelWithGuidKeyDictionary_UsesGuidPropertyNamesAndAdditionalProperties()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithGuidKeyDictionary",
		  "type": "object",
		  "properties": {
		    "Data": {
		      "type": "object",
		      "propertyNames": { "type": "string", "format": "uuid" },
		      "additionalProperties": { "type": "integer" }
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithGuidKeyDictionary;

		AssertEqual(expected, actual);
	}

	[Test]
	public void NullableAttribute_OverridesTypeNullability()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithNullableOverrides",
		  "type": "object",
		  "properties": {
		    "ForcedNullable":    { "type": ["null", "string"], "description": "Force-nullable non-nullable string." },
		    "ForcedNonNullable": { "type": "integer",         "description": "Force-non-nullable nullable int." }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithNullableOverrides;

		AssertEqual(expected, actual);
	}

	[Test]
	public void DuplicateMappedPropertyNames_DoNotBreakSchemaGeneration()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ModelWithDuplicateSchemaPropertyNames",
		  "type": "object",
		  "properties": {
		    "foo": { "type": "integer" }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ModelWithDuplicateSchemaPropertyNames;

		AssertEqual(expected, actual);
	}

	[Test]
	public void SingleCondition_GeneratesIfThen()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.SingleCondition",
		  "type": "object",
		  "properties": {
		    "Toggle": { "type": "boolean" },
		    "Required": { "type": ["null", "string"] }
		  },
		  "required": ["Toggle"],
		  "if": {
		    "properties": {
		      "Toggle": { "const": true }
		    },
		    "required": ["Toggle"]
		  },
		  "then": {
		    "required": ["Required"]
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_SingleCondition;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void SingleConditionCamelCase_HonorsNamingConvention()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.SingleConditionCamelCase",
		  "type": "object",
		  "properties": {
		    "toggle": { "type": "boolean" },
		    "required": { "type": ["null", "string"] }
		  },
		  "required": ["toggle"],
		  "if": {
		    "properties": {
		      "toggle": { "const": true }
		    },
		    "required": ["toggle"]
		  },
		  "then": {
		    "required": ["required"]
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_SingleConditionCamelCase;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void MultipleConditionGroups_GeneratesMultipleIfThen()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.MultipleConditionGroups",
		  "type": "object",
		  "properties": {
		    "Toggle": { "type": "boolean" },
		    "OtherToggle": { "type": "integer" },
		    "RequiredIfToggle": { "type": ["null", "string"] },
		    "RequiredIfOtherToggle": { "type": ["null", "string"] }
		  },
		  "required": ["Toggle", "OtherToggle"],
		  "allOf": [
		    {
		      "if": {
		        "properties": {
		          "Toggle": { "const": true }
		        },
		        "required": ["Toggle"]
		      },
		      "then": {
		        "required": ["RequiredIfToggle"]
		      }
		    },
		    {
		      "if": {
		        "properties": {
		          "OtherToggle": { "const": 42 }
		        },
		        "required": ["OtherToggle"]
		      },
		      "then": {
		        "required": ["RequiredIfOtherToggle"]
		      }
		    }
		  ]
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_MultipleConditionGroups;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void MultipleTriggersInSameGroup_CombinesTriggers()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.MultipleTriggersInSameGroup",
		  "type": "object",
		  "properties": {
		    "Count": { "type": "integer" },
		    "Name": { "type": "string" },
		    "SpecialField": { "type": ["null", "string"] }
		  },
		  "required": ["Count", "Name"],
		  "if": {
		    "properties": {
		      "Count": { "const": 1 },
		      "Name": { "const": "special" }
		    },
		    "required": ["Count", "Name"]
		  },
		  "then": {
		    "required": ["SpecialField"]
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_MultipleTriggersInSameGroup;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void MultipleIfsSamePropertyAndGroup_CombinesToSinglePropertyTrigger()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.MultipleIfsSamePropertyAndGroup",
		  "type": "object",
		  "properties": {
		    "Status": { "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.Status" },
		    "Note": {
		      "type": ["null", "string"]
		    }
		  },
		  "required": ["Status"],
		  "if": {
		    "properties": {
		      "Status": {
		        "enum": ["Active", "Pending"]
		      }
		    },
		    "required": ["Status"]
		  },
		  "then": {
		    "required": ["Note"]
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_MultipleIfsSamePropertyAndGroup;

		AssertEqual(expected, actual);
	}

	[Test]
	public void MultipleIfsSamePropertyAcrossGroups_GeneratesWithoutDuplicateIfProperties()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.MultipleIfsSamePropertyAcrossGroups",
		  "type": "object",
		  "properties": {
		    "Status": { "$ref": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.Status" },
		    "JobListingId": {
		      "type": ["null", "string"],
		      "format": "uuid"
		    },
		    "OrgId": {
		      "type": ["null", "string"],
		      "format": "uuid"
		    }
		  },
		  "required": ["Status"],
		  "allOf": [
		    {
		      "if": {
		        "properties": {
		          "Status": {
		            "enum": ["Active", "Pending"]
		          }
		        },
		        "required": ["Status"]
		      },
		      "then": {
		        "required": ["JobListingId"]
		      }
		    },
		    {
		      "if": {
		        "properties": {
		          "Status": {
		            "const": "Inactive"
		          }
		        },
		        "required": ["Status"]
		      },
		      "then": {
		        "required": ["OrgId"]
		      }
		    }
		  ]
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_MultipleIfsSamePropertyAcrossGroups;

		AssertEqual(expected, actual);
	}

	[Test]
	public void ConditionalWithMinimum_GeneratesMinimumTrigger()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ConditionalWithMinimum",
		  "type": "object",
		  "properties": {
		    "Age": { "type": "integer" },
		    "AdultField": { "type": ["null", "string"] }
		  },
		  "required": ["Age"],
		  "if": {
		    "properties": {
		      "Age": { "minimum": 18 }
		    },
		    "required": ["Age"]
		  },
		  "then": {
		    "required": ["AdultField"]
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ConditionalWithMinimum;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void ConditionalWithMaximum_GeneratesMaximumTrigger()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ConditionalWithMaximum",
		  "type": "object",
		  "properties": {
		    "Score": { "type": "integer" },
		    "BonusEligible": { "type": ["null", "string"] }
		  },
		  "required": ["Score"],
		  "if": {
		    "properties": {
		      "Score": { "maximum": 100 }
		    },
		    "required": ["Score"]
		  },
		  "then": {
		    "required": ["BonusEligible"]
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ConditionalWithMaximum;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void EnumSwitch_GeneratesConditionPerEnumValue()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.EnumSwitch",
		  "type": "object",
		  "properties": {
		    "Day": { "$ref": "global::System.DayOfWeek" },
		    "MondayField": { "type": ["null", "string"] },
		    "TuesdayField": { "type": ["null", "string"] }
		  },
		  "required": ["Day"],
		  "allOf": [
		    {
		      "if": {
		        "properties": {
		          "Day": { "const": "Monday" }
		        },
		        "required": ["Day"]
		      },
		      "then": {
		        "required": ["MondayField"]
		      }
		    },
		    {
		      "if": {
		        "properties": {
		          "Day": { "const": "Tuesday" }
		        },
		        "required": ["Day"]
		      },
		      "then": {
		        "required": ["TuesdayField"]
		      }
		    }
		  ]
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_EnumSwitch;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void ConditionalValidation_EmitsValidationInThenClause()
	{
		var expectedJson = """
		{
		  "$schema": "https://json-schema.org/draft/2020-12/schema",
		  "$id": "global::Json.Schema.Generation.Tests.SourceGeneration.TestModels.ConditionalValidation",
		  "type": "object",
		  "properties": {
		    "IsActive": { "type": "boolean" },
		    "Name": { "type": ["null", "string"] },
		    "Age": { "type": ["null", "integer"] }
		  },
		  "required": ["IsActive"],
		  "if": {
		    "properties": {
		      "IsActive": { "const": true }
		    },
		    "required": ["IsActive"]
		  },
		  "then": {
		    "properties": {
		      "Name": {
		        "minLength": 5,
		        "maxLength": 100
		      },
		      "Age": {
		        "minimum": 0,
		        "maximum": 150
		      }
		    }
		  }
		}
		""";
		var expected = JsonSchema.FromText(expectedJson, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
		var actual = GeneratedJsonSchemas.TestModels_ConditionalValidation;
		
		AssertEqual(expected, actual);
	}

	[Test]
	public void PersonWithSortedProperties_SortsPropertiesByName()
	{
		var actual = GeneratedJsonSchemas.TestModels_PersonWithSortedProperties;
		
		// Use Root.Source to get the JSON element
		var propertiesElement = actual.Root.Source.GetProperty("properties");
		
		var propertyNames = propertiesElement.EnumerateObject().Select(p => p.Name).ToList();
		
		// Properties should be sorted alphabetically: Age, City, Email, Name
		Assert.That(propertyNames.Count, Is.EqualTo(4));
		Assert.That(propertyNames[0], Is.EqualTo("Age"));
		Assert.That(propertyNames[1], Is.EqualTo("City"));
		Assert.That(propertyNames[2], Is.EqualTo("Email"));
		Assert.That(propertyNames[3], Is.EqualTo("Name"));
	}

	[Test]
	public void StrictConditionalValidation_ExcludesConditionalPropertiesFromGlobalScope()
	{
		var actual = GeneratedJsonSchemas.TestModels_StrictConditionalValidation;
		
		// In strict mode, properties with conditional constraints should NOT be in global properties
		var propertiesElement = actual.Root.Source.GetProperty("properties");
		var propertyNames = propertiesElement.EnumerateObject().Select(p => p.Name).ToList();
		
		// Only IsActive should be in global properties, not Name or Age
		Assert.That(propertyNames.Count, Is.EqualTo(1));
		Assert.That(propertyNames[0], Is.EqualTo("IsActive"));
	}

	[Test]
	public void StrictConditionalValidation_UsesUnevaluatedProperties()
	{
		var actual = GeneratedJsonSchemas.TestModels_StrictConditionalValidation;
		
		// In strict mode, should use unevaluatedProperties instead of additionalProperties
		Assert.That(actual.Root.Source.TryGetProperty("unevaluatedProperties", out var unevaluatedProps), Is.True);
		Assert.That(unevaluatedProps.GetBoolean(), Is.False);
		
		// Should NOT have additionalProperties
		Assert.That(actual.Root.Source.TryGetProperty("additionalProperties", out _), Is.False);
	}

	[Test]
	public void StrictConditionalValidation_EmitsFullPropertySchemasInThenClause()
	{
		var actual = GeneratedJsonSchemas.TestModels_StrictConditionalValidation;
		
		// Get the if/then structure
		var ifElement = actual.Root.Source.GetProperty("if");
		var thenElement = actual.Root.Source.GetProperty("then");
		
		// Then clause should have properties with full schemas
		var thenProperties = thenElement.GetProperty("properties");
		
		// Name and Age should be in then clause
		Assert.That(thenProperties.TryGetProperty("Name", out var nameSchema), Is.True);
		Assert.That(thenProperties.TryGetProperty("Age", out var ageSchema), Is.True);
		
		// Name should have type definition (full schema)
		Assert.That(nameSchema.TryGetProperty("type", out var nameType), Is.True);
		
		// Age should have type definition (full schema)
		Assert.That(ageSchema.TryGetProperty("type", out var ageType), Is.True);
		
		// Name should have conditional constraints
		Assert.That(nameSchema.TryGetProperty("minLength", out var minLength), Is.True);
		Assert.That(minLength.GetInt32(), Is.EqualTo(5));
		Assert.That(nameSchema.TryGetProperty("maxLength", out var maxLength), Is.True);
		Assert.That(maxLength.GetInt32(), Is.EqualTo(100));
		
		// Age should have conditional constraints
		Assert.That(ageSchema.TryGetProperty("minimum", out var minimum), Is.True);
		Assert.That(minimum.GetInt32(), Is.EqualTo(0));
		Assert.That(ageSchema.TryGetProperty("maximum", out var maximum), Is.True);
		Assert.That(maximum.GetInt32(), Is.EqualTo(150));
	}

	[Test]
	public void ConditionalValidation_NonStrict_IncludesPropertiesGlobally()
	{
		var actual = GeneratedJsonSchemas.TestModels_ConditionalValidation;
		
		// In non-strict mode (default), all properties should be in global properties
		var propertiesElement = actual.Root.Source.GetProperty("properties");
		var propertyNames = propertiesElement.EnumerateObject().Select(p => p.Name).ToList();
		
		// All three properties should be present: IsActive, Name, Age
		Assert.That(propertyNames.Count, Is.EqualTo(3));
		Assert.That(propertyNames, Does.Contain("IsActive"));
		Assert.That(propertyNames, Does.Contain("Name"));
		Assert.That(propertyNames, Does.Contain("Age"));
		
		// In non-strict mode, should not have unevaluatedProperties or additionalProperties
		Assert.That(actual.Root.Source.TryGetProperty("unevaluatedProperties", out _), Is.False);
		Assert.That(actual.Root.Source.TryGetProperty("additionalProperties", out _), Is.False);
	}
}

