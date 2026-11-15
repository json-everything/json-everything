using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class BaseDocumentTests
{
	[Test]
	public void SchemasEmbeddedInJsonCanBeReferenced_Valid()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};

		JsonSchema targetSchema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer);

		var schemaAsText = JsonSerializer.Serialize(targetSchema, TestEnvironment.SerializerOptions);

		var jsonText = $$"""
						 {
						   "prop1": "foo",
						   "prop2": [
						     "bar",
						     {{schemaAsText}}
						   ]
						 }
						 """;
		var json = JsonDocument.Parse(jsonText).RootElement;

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};

		var jsonBaseDoc = new JsonElementBaseDocument(json, new Uri("http://localhost:1234/doc"));
		buildOptions.SchemaRegistry.Register(jsonBaseDoc);

		JsonSchema subjectSchema = new JsonSchemaBuilder(buildOptions)
			.Ref("http://localhost:1234/doc#/prop2/1");

		var instance = JsonDocument.Parse("42").RootElement;

		var result = subjectSchema.Evaluate(instance, options);

		result.AssertValid();
	}

	[Test]
	public void SchemasEmbeddedInJsonCanBeReferenced_Invalid()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};

		JsonSchema targetSchema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Integer);

		var schemaAsText = JsonSerializer.Serialize(targetSchema, TestEnvironment.SerializerOptions);

		var jsonText = $$"""
						 {
						   "prop1": "foo",
						   "prop2": [
						     "bar",
						     {{schemaAsText}}
						   ]
						 }
						 """;
		var json = JsonDocument.Parse(jsonText).RootElement;

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};

		var jsonBaseDoc = new JsonElementBaseDocument(json, new Uri("http://localhost:1234/doc"));
		buildOptions.SchemaRegistry.Register(jsonBaseDoc);

		JsonSchema subjectSchema = new JsonSchemaBuilder(buildOptions)
			.Ref("http://localhost:1234/doc#/prop2/1");

		var instance = JsonDocument.Parse("\"baz\"").RootElement;

		var result = subjectSchema.Evaluate(instance, options);

		result.AssertInvalid();
	}

	[Test]
	public void ReferencesFromWithinEmbeddedSchemas()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};

		var jsonText = """
		               {
		                 "prop1": "foo",
		                 "prop2": [
		                   "bar",
		                   {
		                     "type": "integer"
		                   }
		                 ],
		                 "prop3": {
		                   "$ref": "#/prop2/1"
		                 }
		               }
		               """;
		var json = JsonDocument.Parse(jsonText).RootElement;

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};

		var jsonBaseDoc = new JsonElementBaseDocument(json, new Uri("http://localhost:1234/doc"));
		buildOptions.SchemaRegistry.Register(jsonBaseDoc);

		JsonSchema subjectSchema = new JsonSchemaBuilder(buildOptions)
			.Ref("http://localhost:1234/doc#/prop3");

		var instance = JsonDocument.Parse("42").RootElement;

		var result = subjectSchema.Evaluate(instance, options);

		result.AssertValid();
	}

	[Test]
	public void NestedReferencesFromWithinEmbeddedSchemas()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};

		var jsonText = """
		               {
		                 "prop1": "foo",
		                 "prop2": [
		                   "bar",
		                   {
		                     "type": "integer"
		                   }
		                 ],
		                 "prop3": {
		                   "properties": {
		                     "data": {
		                       "$ref": "#/prop2/1"
		                     }
		                   }
		                 }
		               }
		               """;
		var json = JsonDocument.Parse(jsonText).RootElement;

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};

		var jsonBaseDoc = new JsonElementBaseDocument(json, new Uri("http://localhost:1234/doc"));
		buildOptions.SchemaRegistry.Register(jsonBaseDoc);

		JsonSchema subjectSchema = new JsonSchemaBuilder(buildOptions)
			.Ref("http://localhost:1234/doc#/prop3");

		var instance = JsonDocument.Parse("""{ "data": 42 }""").RootElement;

		var result = subjectSchema.Evaluate(instance, options);

		result.AssertValid();
	}

	[Test]
	[Ignore("Maybe this isn't a valid thing to do?")]
	public void ReferenceEmbeddedSchemaStartingWithOtherEmbeddedSchema()
	{
		// TODO: having an issue with this.  Starting at /prop3 automatically generates
		//       an ID for that subschema, causing the $ref to resolve to that subschema
		//       location instead of the document root.
		var jsonDocBaseUri = new Uri("http://localhost:1234/doc");
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new()
		};

		var jsonText = """
		               {
		                 "prop1": "foo",
		                 "prop2": [
		                   "bar",
		                   {
		                     "type": "integer"
		                   }
		                 ],
		                 "prop3": {
		                   "properties": {
		                     "data": {
		                       "$ref": "#/prop2/1"
		                     }
		                   }
		                 }
		               }
		               """;
		var json = JsonDocument.Parse(jsonText).RootElement;
		var subjectSchemaJson = json.GetProperty("prop3");

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};

		var jsonBaseDoc = new JsonElementBaseDocument(json, jsonDocBaseUri);
		buildOptions.SchemaRegistry.Register(jsonBaseDoc);

		var subjectSchema = JsonSchema.Build(subjectSchemaJson, buildOptions, jsonDocBaseUri); // throws 

		var instance = JsonDocument.Parse("""{ "data": 42 }""").RootElement;

		var result = subjectSchema.Evaluate(instance, options);

		result.AssertValid();
	}
}