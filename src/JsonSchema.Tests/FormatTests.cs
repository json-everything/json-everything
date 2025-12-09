using System;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public class FormatTests
{
	//private static readonly Uri _formatAssertionMetaSchemaId = new("https://json-everything/test/format-assertion");
	//private static readonly JsonSchema _formatAssertionMetaSchema =
	//	new JsonSchemaBuilder()
	//		.Schema(MetaSchemas.Draft202012Id)
	//		.Id(_formatAssertionMetaSchemaId)
	//		.Vocabulary(
	//			(Vocabulary.Draft201909_Core.Id, true),
	//			(Vocabulary.Draft202012_Applicator.Id, true),
	//			(Vocabulary.Draft202012_MetaData.Id, true),
	//			(Vocabulary.Draft202012_FormatAssertion.Id, false)
	//		)
	//		.DynamicAnchor("meta")
	//		.Title("format assertion meta-schema")
	//		.AllOf(
	//			new JsonSchemaBuilder().Ref(MetaSchemas.Core202012Id),
	//			new JsonSchemaBuilder().Ref(MetaSchemas.Applicator202012Id),
	//			new JsonSchemaBuilder().Ref(MetaSchemas.Metadata202012Id),
	//			new JsonSchemaBuilder().Ref(MetaSchemas.FormatAssertion202012Id)
	//		)
	//		.Type(SchemaValueType.Object | SchemaValueType.Boolean);

	[Test]
	public void UnknownFormat_Annotation_ReportsFormat()
	{
		var schemaText = $@"{{
	""$schema"": ""{MetaSchemas.Draft202012Id}"",
	""type"": ""string"",
	""format"": ""something-dumb""
}}";
		var buildOptions = new BuildOptions { Dialect = Dialect.Draft202012 };
		var schema = JsonSchema.FromText(schemaText, buildOptions);
		var instance = JsonDocument.Parse("\"a value\"").RootElement;

		var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		results.AssertValid();
		var serialized = JsonSerializer.Serialize(results, TestEnvironment.SerializerOptions);
		Assert.That(serialized, Contains.Substring("something-dumb"));
	}

	//[Test]
	//public void UnknownFormat_Assertion_FailsValidation()
	//{
	//	var options = new EvaluationOptions
	//	{
	//		OutputFormat = OutputFormat.Hierarchical,
	//		OnlyKnownFormats = true
	//	};

	//	var schemaText = $@"{{
	//	""$schema"": ""{_formatAssertionMetaSchemaId}"",
	//	""type"": ""string"",
	//	""format"": ""something-dumb""
	//}}";
	//	var buildOptions = new BuildOptions { Dialect = Dialect.Draft202012 };
	//	var schema = JsonSchema.FromText(schemaText, buildOptions);
	//	var instance = JsonDocument.Parse("\"a value\"").RootElement;

	//	var results = schema.Evaluate(instance, options);

	//	results.AssertInvalid();
	//	var serialized = JsonSerializer.Serialize(results, TestEnvironment.SerializerOptions);
	//	Assert.That(serialized, Contains.Substring("something-dumb"));
	//}

	//[Test]
	//public void UnknownFormat_AnnotationWithAssertionOption_FailsValidation()
	//{
	//	var schemaText = $@"{{
	//	""$schema"": ""{MetaSchemas.Draft202012Id}"",
	//	""type"": ""string"",
	//	""format"": ""something-dumb""
	//}}";
	//	var buildOptions = new BuildOptions { Dialect = Dialect.Draft202012 };
	//	var schema = JsonSchema.FromText(schemaText, buildOptions);
	//	var instance = JsonDocument.Parse("\"a value\"").RootElement;

	//	var results = schema.Evaluate(instance, new EvaluationOptions
	//	{
	//		OutputFormat = OutputFormat.Hierarchical,
	//		RequireFormatValidation = true,
	//		OnlyKnownFormats = true
	//	});

	//	results.AssertInvalid();
	//	var serialized = JsonSerializer.Serialize(results, TestEnvironment.SerializerOptions);
	//	Assert.That(serialized, Contains.Substring("something-dumb"));
	//}

	private class RegexBasedFormat : RegexFormat
	{
		public RegexBasedFormat()
			: base("hexadecimal", "^[0-9a-fA-F]+$")
		{
		}
	}

	[TestCase("\"1dd7fe33f97f42cf89c5789018bae64d\"", true)]
	[TestCase("\"nwvoiwe;oiabe23oi32\"", false)]
	[TestCase("true", true)]
	public void RegexBasedFormatWorksProperly(string jsonText, bool isValid)
	{
		Formats.Register(new RegexBasedFormat());

		var json = JsonDocument.Parse(jsonText).RootElement;
		JsonSchema schema = new JsonSchemaBuilder()
			.Format("hexadecimal");

		var results = schema.Evaluate(json, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			RequireFormatValidation = true
		});

		TestConsole.WriteLine(JsonSerializer.Serialize(results, TestEnvironment.TestOutputSerializerOptions));
		Assert.That(results.IsValid, Is.EqualTo(isValid));
	}
}