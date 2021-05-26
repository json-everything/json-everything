using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class NullTests
	{
		private static readonly JsonSchema _schema =
			new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Required("foo");

		[Test]
		public void PropertyWithNullValuePasses()
		{
			var json = JsonDocument.Parse("{\"foo\": null}").RootElement;

			var result = _schema.Validate(json, new ValidationOptions {OutputFormat = OutputFormat.Detailed});

			result.AssertValid();
		}

		[Test]
		public void MissingPropertyFails()
		{
			var json = JsonDocument.Parse("{}").RootElement;

			var result = _schema.Validate(json, new ValidationOptions { OutputFormat = OutputFormat.Detailed });

			result.AssertInvalid();
		}
	}
}
