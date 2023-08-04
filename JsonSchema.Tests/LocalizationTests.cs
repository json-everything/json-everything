using System;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class LocalizationTests
{
	[Test]
	public void MinimumReturnsDefaultErrorMessage()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10);

		var instance = JsonNode.Parse("5");

		var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		var message = results.Errors!["minimum"];

		Assert.AreEqual("5 should be at least 10", message);
	}

	[Test]
	[Ignore("Can't test localization since resource file is in a separate dll now.")]
	public void MinimumReturnsDefaultErrorMessageButInSpanish()
	{
		try
		{
			ErrorMessages.Culture = CultureInfo.GetCultureInfo("es-es");

			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Minimum(10);

			var instance = JsonNode.Parse("5");

			var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

			var message = results.Errors!["minimum"];

			Assert.AreEqual("5 es menor o igual que 10", message);
		}
		finally
		{
			ErrorMessages.Culture = null;
		}
	}

	[Test]
	public void MinimumReturnsCustomErrorMessage()
	{
		try
		{
			ErrorMessages.Minimum = "This is a custom error message with [[received]] and [[limit]]";

			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Minimum(10);

			var instance = JsonNode.Parse("5");

			var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

			var message = results.Errors!["minimum"];

			Assert.AreEqual("This is a custom error message with 5 and 10", message);
		}
		finally
		{
			ErrorMessages.Minimum = null!;
		}
	}

	[Test]
	public async Task DifferentCulturesInParallel()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(10);
		var instance = JsonNode.Parse("5");

		string RunWithCulture(CultureInfo culture)
		{
			var results = schema.Evaluate(instance, new EvaluationOptions
			{
				OutputFormat = OutputFormat.Hierarchical,
				Culture = culture
			});

			return results.Errors!["minimum"];
		}

		try
		{
			var messages = await Task.WhenAll(
				Task.Run(() => RunWithCulture(CultureInfo.GetCultureInfo("es"))),
				Task.Run(() => RunWithCulture(CultureInfo.GetCultureInfo("en-us")))
			);

			Assert.AreEqual("5 es menor o igual que 10", messages[0]);
			Assert.AreEqual("5 should be at least 10", messages[1]);
		}
		finally
		{
			ErrorMessages.Culture = null;
		}
	}
}