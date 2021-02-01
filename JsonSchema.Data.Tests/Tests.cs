using System.Text.Json;
using Json.Schema.Tests;
using NUnit.Framework;

namespace Json.Schema.Data.Tests
{
	public class Tests
	{
		[OneTimeSetUp]
		public void Setup()
		{ 
			VocabularyRegistry.Global.Register(Vocabularies.Data);
			SchemaKeywordRegistry.Register<DataKeyword>();
			SchemaRegistry.Global.Register(MetaSchemas.DataId, MetaSchemas.Data);

			ValidationOptions.Default.OutputFormat = OutputFormat.Detailed;
		}

		[Test]
		public void SpecExample_Passing()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("https://gregsdennis.github.io/json-everything/meta/data")
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Data(("minimum", "#/minValue"))
					),
					("minValue", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				)
				.DependentRequired(("foo", new[] {"minValue"}));

			var instanceData = "{\"minValue\":5,\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = schema.Validate(instance);

			result.AssertValid();
		}

		[Test]
		public void SpecExample_Failing()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("https://gregsdennis.github.io/json-everything/meta/data")
				.Type(SchemaValueType.Object)
				.Properties(
					("foo", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Data(("minimum", "#/minValue"))
					),
					("minValue", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				)
				.DependentRequired(("foo", new[] {"minValue"}));

			var instanceData = "{\"minValue\":15,\"foo\":10}";
			var instance = JsonDocument.Parse(instanceData).RootElement;

			var result = schema.Validate(instance);

			result.AssertInvalid();
		}
	}
}