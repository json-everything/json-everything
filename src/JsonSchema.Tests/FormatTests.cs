using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public class FormatTests
{
	[Test]
	public void Ipv4_Pass()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Format(Formats.Ipv4);

		var value = JsonNode.Parse("\"100.2.54.3\"");

		var result = schema.Evaluate(value, new EvaluationOptions { RequireFormatValidation = true });

		result.AssertValid();
	}
	[Test]
	public void Ipv4_Fail()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Format(Formats.Ipv4);

		var value = JsonNode.Parse("\"100.2.5444.3\"");

		var result = schema.Evaluate(value, new EvaluationOptions { RequireFormatValidation = true });

		result.AssertInvalid();
	}

	[TestCase("2023-04-28T21:51:26.56Z")]
	[TestCase("2023-03-22T07:56:28.610645938Z")]
	[TestCase("2023-03-22 07:56:28.610645938Z")]
	[TestCase("2023-04-28T21:50:24-00:00")]
	[TestCase("2023-04-29t09:50:36+12:00")]
	[TestCase("2023-04-28 21:50:44Z")]
	[TestCase("2023-04-28_21:50:58.563Z")]
	[TestCase("2023-04-28_21:51:10Z")]
	public void DateTime_Pass(string dateString)
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Format(Formats.DateTime);

		var value = JsonNode.Parse($"\"{dateString}\"");

		var result = schema.Evaluate(value, new EvaluationOptions { RequireFormatValidation = true });

		result.AssertValid();
	}

	[Test]
	public void DateTime_MissingTimeOffset_Fail()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Format(Formats.DateTime);

		var value = JsonNode.Parse("\"2023-04-28T21:51:26\"");

		var result = schema.Evaluate(value, new EvaluationOptions { RequireFormatValidation = true });

		result.AssertInvalid();
	}

	private static readonly Uri _formatAssertionMetaSchemaId = new("https://json-everything/test/format-assertion");
	private static readonly JsonSchema _formatAssertionMetaSchema =
		new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Id(_formatAssertionMetaSchemaId)
			.Vocabulary(
				(Vocabularies.Core202012Id, true),
				(Vocabularies.Applicator202012Id, true),
				(Vocabularies.Metadata202012Id, true),
				(Vocabularies.FormatAssertion202012Id, false)
			)
			.DynamicAnchor("meta")
			.Title("format assertion meta-schema")
			.AllOf(
				new JsonSchemaBuilder().Ref(MetaSchemas.Core202012Id),
				new JsonSchemaBuilder().Ref(MetaSchemas.Applicator202012Id),
				new JsonSchemaBuilder().Ref(MetaSchemas.Metadata202012Id),
				new JsonSchemaBuilder().Ref(MetaSchemas.FormatAssertion202012Id)
			)
			.Type(SchemaValueType.Object | SchemaValueType.Boolean);

	[Test]
	public void UnknownFormat_Annotation_ReportsFormat()
	{
		var schemaText = $@"{{
	""$schema"": ""{MetaSchemas.Draft202012Id}"",
	""type"": ""string"",
	""format"": ""something-dumb""
}}";
		var schema = JsonSchema.FromText(schemaText);
		var instance = JsonNode.Parse("\"a value\"");

		var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		results.AssertValid();
		var serialized = JsonSerializer.Serialize(results, TestEnvironment.SerializerOptions);
		Assert.That(serialized, Contains.Substring("something-dumb"));
	}

	[Test]
	public void UnknownFormat_Assertion_FailsValidation()
	{
		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			OnlyKnownFormats = true
		};

		var schemaText = $@"{{
	""$schema"": ""{_formatAssertionMetaSchemaId}"",
	""type"": ""string"",
	""format"": ""something-dumb""
}}";
		var schema = JsonSchema.FromText(schemaText);
		options.SchemaRegistry.Register(_formatAssertionMetaSchema);
		var instance = JsonNode.Parse("\"a value\"");

		var results = schema.Evaluate(instance, options);

		results.AssertInvalid();
		var serialized = JsonSerializer.Serialize(results, TestEnvironment.SerializerOptions);
		Assert.That(serialized, Contains.Substring("something-dumb"));
	}

	[Test]
	public void UnknownFormat_AnnotationWithAssertionOption_FailsValidation()
	{
		var schemaText = $@"{{
	""$schema"": ""{MetaSchemas.Draft202012Id}"",
	""type"": ""string"",
	""format"": ""something-dumb""
}}";
		var schema = JsonSchema.FromText(schemaText);
		var instance = JsonNode.Parse("\"a value\"");

		var results = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			RequireFormatValidation = true,
			OnlyKnownFormats = true
		});

		results.AssertInvalid();
		var serialized = JsonSerializer.Serialize(results, TestEnvironment.SerializerOptions);
		Assert.That(serialized, Contains.Substring("something-dumb"));
	}

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

		var json = JsonNode.Parse(jsonText);
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

	[Test]
	public void HostnameShouldSupportLongerStrings()
	{
		var schema = new JsonSchemaBuilder().Format(Formats.Hostname);
		JsonNode instance = "hostname.exceeding24characte.rs";

		var results = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			RequireFormatValidation = true
		});

		TestConsole.WriteLine(JsonSerializer.Serialize(results, TestEnvironment.TestOutputSerializerOptions));
		Assert.That(results.IsValid, Is.True);
	}

	[Test]
	public void IdnHostnameShouldSupportLongerStrings()
	{
		var schema = new JsonSchemaBuilder().Format(Formats.Hostname);
		JsonNode instance = "hostname.exceeding24characte.rs";

		var results = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			RequireFormatValidation = true
		});

		TestConsole.WriteLine(JsonSerializer.Serialize(results, TestEnvironment.TestOutputSerializerOptions));
		Assert.That(results.IsValid, Is.True);
	}
}