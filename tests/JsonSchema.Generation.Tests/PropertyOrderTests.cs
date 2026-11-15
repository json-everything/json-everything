using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;
// ReSharper disable ClassNeverInstantiated.Local

namespace Json.Schema.Generation.Tests;

public class PropertyOrderTests
{
	private class SpecifiedOrder
	{
		public int Second { get; set; }
		public int First { get; set; }
	}

	private class SpecifiedOrderDerived : SpecifiedOrder
	{
		public int Third { get; set; }
	}

	[Test]
	public void PropertiesAsDeclared()
	{
		var config = new SchemaGeneratorConfiguration
		{
			PropertyOrder = PropertyOrder.AsDeclared
		};

		var builder = new JsonSchemaBuilder();
		builder.FromType<SpecifiedOrder>(config);

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "type": "object",
			  "properties": {
			    "Second": {"type": "integer"},
			    "First": {"type": "integer"}
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}

	[Test]
	public void PropertiesAsDeclaredByType()
	{
		var config = new SchemaGeneratorConfiguration
		{
			PropertyOrder = PropertyOrder.AsDeclared
		};

		var builder = new JsonSchemaBuilder();
		builder.FromType<SpecifiedOrderDerived>(config);

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "type": "object",
			  "properties": {
			    "Second": {"type": "integer"},
			    "First": {"type": "integer"},
			    "Third": {"type": "integer"}
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}

	[Test]
	public void PropertiesByName()
	{
		var config = new SchemaGeneratorConfiguration
		{
			PropertyOrder = PropertyOrder.ByName
		};

		var builder = new JsonSchemaBuilder();
		builder.FromType<SpecifiedOrder>(config);

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "type": "object",
			  "properties": {
			    "First": {"type": "integer"},
			    "Second": {"type": "integer"}
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}
}