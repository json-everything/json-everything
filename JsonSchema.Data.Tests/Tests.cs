using System.Text.Json;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.Data.Tests
{
	public class Tests
	{
		private static JsonSchema InstanceRef { get; } = new JsonSchemaBuilder()
			.Schema("https://gregsdennis.github.io/json-everything/meta/data")
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Data(("minimum", "#/minValue"))
				)
			);

		private static JsonSchema ExternalRef { get; } = new JsonSchemaBuilder()
			.Schema("https://gregsdennis.github.io/json-everything/meta/data")
			.Type(SchemaValueType.Object)
			.Properties(
				("foo", new JsonSchemaBuilder()
					.Type(SchemaValueType.Integer)
					.Data(("minimum", "http://json.test/data#/minValue"))
				)
			);

		[OneTimeSetUp]
		public void Setup()
		{ 
			Vocabularies.Register();

			ValidationOptions.Default.OutputFormat = OutputFormat.Detailed;
		}

		[Test]
		public void InstanceRef_Passing()
		{
			var instanceData = "{\"minValue\":5,\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = InstanceRef.Validate(instance);

			result.AssertValid();
		}

		[Test]
		public void InstanceRef_Failing()
		{
			var instanceData = "{\"minValue\":15,\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = InstanceRef.Validate(instance);

			result.AssertInvalid();
		}

		[Test]
		public void InstanceRef_InvalidValueType()
		{
			var instanceData = "{\"minValue\":true,\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = InstanceRef.Validate(instance);

			result.AssertInvalid();
		}

		[Test]
		public void InstanceRef_Unresolvable()
		{
			var instanceData = "{\"minValu\":5,\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = InstanceRef.Validate(instance);

			result.AssertInvalid();
		}

		[Test]
		public void ExternalRef_Passing()
		{
			try
			{
				DataKeyword.Get = uri => "{\"minValue\":5}";

				var instanceData = "{\"foo\":10}";
				var instance = JsonDocument.Parse(instanceData).RootElement;

				var result = ExternalRef.Validate(instance);

				result.AssertValid();
			}
			finally
			{
				DataKeyword.Get = null;
			}
		}

		[Test]
		public void ExternalRef_Failing()
		{
			try
			{
				DataKeyword.Get = uri => "{\"minValue\":15}";

				var instanceData = "{\"foo\":10}";
				var instance = JsonDocument.Parse(instanceData).RootElement;

				var result = ExternalRef.Validate(instance);

				result.AssertInvalid();
			}
			finally
			{
				DataKeyword.Get = null;
			}
		}
	}
}