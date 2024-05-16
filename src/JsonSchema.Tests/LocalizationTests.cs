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

		Assert.That(message, Is.EqualTo("5 should be at least 10"));
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

			Assert.That(message, Is.EqualTo("5 es menor o igual que 10"));
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

			Assert.That(message, Is.EqualTo("This is a custom error message with 5 and 10"));
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

			Assert.Multiple(() =>
			{
				Assert.That(messages[0], Is.EqualTo("5 es menor o igual que 10"));
				Assert.That(messages[1], Is.EqualTo("5 should be at least 10"));
			});
		}
		finally
		{
			ErrorMessages.Culture = null;
		}
	}
}