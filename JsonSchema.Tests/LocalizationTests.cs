using System.Globalization;
using System.Text.Json;
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

		var instance = JsonDocument.Parse("5");

		var results = schema.Validate(instance.RootElement, new ValidationOptions { OutputFormat = OutputFormat.Basic });

		var message = results.Errors["minimum"];

		Assert.AreEqual("5 is less than or equal to 10", message);
	}

	[Test]
	public void MinimumReturnsDefaultErrorMessageButInSpanish()
	{
		try
		{
			ErrorMessages.Culture = CultureInfo.GetCultureInfo("es");

			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Number)
				.Minimum(10);

			var instance = JsonDocument.Parse("5");

			var results = schema.Validate(instance.RootElement, new ValidationOptions { OutputFormat = OutputFormat.Basic });

			var message = results.Errors["minimum"];

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

			var instance = JsonDocument.Parse("5");

			var results = schema.Validate(instance.RootElement, new ValidationOptions { OutputFormat = OutputFormat.Basic });

			var message = results.Errors["minimum"];

			Assert.AreEqual("This is a custom error message with 5 and 10", message);
		}
		finally
		{
			ErrorMessages.Minimum = null!;
		}
	}
}