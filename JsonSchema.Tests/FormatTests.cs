using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class FormatTests
	{
		[Test]
		public void Ipv4_Pass()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Format(Formats.Ipv4);

			var value = JsonDocument.Parse("\"100.2.54.3\"");

			var result = schema.Validate(value.RootElement, new ValidationOptions{ValidateFormat = true});

			Assert.True(result.IsValid);
		}
		[Test]
		public void Ipv4_Fail()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Format(Formats.Ipv4);

			var value = JsonDocument.Parse("\"100.2.5444.3\"");

			var result = schema.Validate(value.RootElement, new ValidationOptions {ValidateFormat = true});

			Assert.False(result.IsValid);
		}
	}
}