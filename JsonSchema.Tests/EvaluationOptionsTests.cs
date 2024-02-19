using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class EvaluationOptionsTests
{
	[Test]
	public void FormatValidatesInTheSameSchemaOnlyWhenConfigured()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.JsonPointer)
			.Build();

		JsonNode instance = "not a pointer";

		var result = schema.Evaluate(instance);
		result.AssertValid();

		result = schema.Evaluate(instance, new EvaluationOptions { RequireFormatValidation = true });
		result.AssertInvalid();
	}

	[Test]
	public void FormatThrowsForUnknownOnlyWhenConfigured()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format("not-a-format")
			.Build();

		JsonNode instance = "not a pointer";

		var result = schema.Evaluate(instance);
		result.AssertValid();

		result = schema.Evaluate(instance, new EvaluationOptions { OnlyKnownFormats = true });
		result.AssertInvalid();
	}

	[Test]
	public void FormatThrowsForUnknownOnlyWhenConfigured_SameOptionsObjectButChanged()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format("not-a-format")
			.Build();

		JsonNode instance = "not a pointer";

		var options = new EvaluationOptions();
		var result = schema.Evaluate(instance, options);
		result.AssertValid();

		options.OnlyKnownFormats = true;
		result = schema.Evaluate(instance, options);
		result.AssertInvalid();
	}

	[Test]
	public void ChangingSchemaRegistryAffectsOutcome()
	{
		var schema = new JsonSchemaBuilder()
			.Id("https://json-everything.test/foo")
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder().Ref("bar"))
			.Build();
		var reffed = new JsonSchemaBuilder()
			.Id("https://json-everything.test/bar")
			.Type(SchemaValueType.Integer)
			.Build();

		var instance = new JsonArray(1, 2, 3);

		var options = new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical };
		options.SchemaRegistry.Register(reffed);

		var result = schema.Evaluate(instance, options);
		result.AssertValid();

		reffed = new JsonSchemaBuilder()
			.Id("https://json-everything.test/bar")
			.Type(SchemaValueType.String)
			.Build();

		options = new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical };
		options.SchemaRegistry.Register(reffed);

		result = schema.Evaluate(instance, options);
		result.AssertInvalid();
	}

	[Test]
	public void ChangingSchemaVersionAffectsOutcome()
	{
		var schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Array)
			.PrefixItems(
				new JsonSchemaBuilder().Type(SchemaValueType.Integer),
				new JsonSchemaBuilder().Type(SchemaValueType.String)
			)
			.Items(new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			.Build();

		var instance = new JsonArray(1, "string", false, true);

		var result = schema.Evaluate(instance, new EvaluationOptions { EvaluateAs = SpecVersion.Draft202012 });
		result.AssertValid();

		result = schema.Evaluate(instance, new EvaluationOptions { EvaluateAs = SpecVersion.Draft201909 });
		result.AssertInvalid();
	}
}