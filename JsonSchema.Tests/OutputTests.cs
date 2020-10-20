using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class OutputTests
	{
		private static readonly JsonSchema _schema =
			new JsonSchemaBuilder()
				.Id("https://test.com/schema")
				.Definitions(
					("integer", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
					("minimum", new JsonSchemaBuilder().Minimum(5))
				)
				.Type(SchemaValueType.Object)
				.Properties(
					("passes", true),
					("fails", false),
					("refs", new JsonSchemaBuilder().Ref("#/$defs/integer")),
					("multi", new JsonSchemaBuilder()
						.AllOf(
							new JsonSchemaBuilder().Ref("#/$defs/integer"),
							new JsonSchemaBuilder().Ref("#/$defs/minimum")
						)
					)
				);

		[Test]
		public void Flag_Success()
		{
			var result = Validate("{\"passes\":\"value\"}", OutputFormat.Flag);

			result.AssertValid();
			Assert.IsEmpty(result.NestedResults);
			Assert.IsEmpty(result.Annotations);
		}

		[Test]
		public void Flag_Failure()
		{
			var result = Validate("{\"fails\":\"value\"}", OutputFormat.Flag);

			result.AssertInvalid();
			Assert.IsEmpty(result.NestedResults);
			Assert.IsEmpty(result.Annotations);
		}

		[Test]
		public void Basic_Success()
		{
			var result = Validate("{\"passes\":\"value\"}", OutputFormat.Basic);

			result.AssertValid();
			Assert.IsNotEmpty(result.NestedResults);
			foreach (var node in result.NestedResults)
			{
				Assert.IsEmpty(node.NestedResults);
				Assert.IsEmpty(node.Annotations);
			}
		}

		[Test]
		public void Basic_Failure()
		{
			var result = Validate("{\"fails\":\"value\"}", OutputFormat.Basic);

			result.AssertInvalid();
			Assert.IsNotEmpty(result.NestedResults);
			foreach (var node in result.NestedResults)
			{
				Assert.IsEmpty(node.NestedResults);
				Assert.IsEmpty(node.Annotations);
			}
		}

		[Test]
		public void Detailed_Success()
		{
			var result = Validate("{\"passes\":\"value\"}", OutputFormat.Detailed);

			result.AssertValid();
			Assert.IsEmpty(result.NestedResults);
			Assert.IsNotEmpty(result.Annotations);
		}

		[Test]
		public void Detailed_Failure()
		{
			var result = Validate("{\"fails\":\"value\"}", OutputFormat.Detailed);

			result.AssertInvalid();
			Assert.IsNotEmpty(result.NestedResults);
			foreach (var node in result.NestedResults)
			{
				Assert.IsEmpty(node.NestedResults);
				Assert.IsEmpty(node.Annotations);
			}
		}

		private static ValidationResults Validate(string json, OutputFormat format)
		{
			var instance = JsonDocument.Parse(json);
			var options = ValidationOptions.From(ValidationOptions.Default);
			options.OutputFormat = format;

			var result = _schema.Validate(instance.RootElement, options);
			return result;
		}
	}
}
