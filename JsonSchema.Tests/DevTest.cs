using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class DevTest
	{
		[Test]
		public void Test()
		{
			var schema = JsonSchema.FromText(@"{}");
			var instance = JsonDocument.Parse(@"{}").RootElement;

			var validationOptions = new ValidationOptions{OutputFormat = OutputFormat.Basic};
			var results = schema.Validate(instance, validationOptions);

            var serializerOptions = new JsonSerializerOptions{WriteIndented = true};
            Console.WriteLine(JsonSerializer.Serialize(schema, serializerOptions));
            Console.WriteLine();
            Console.WriteLine(JsonSerializer.Serialize(instance, serializerOptions));
            Console.WriteLine();
            Console.WriteLine(JsonSerializer.Serialize(results, serializerOptions));
		}
	}
}
