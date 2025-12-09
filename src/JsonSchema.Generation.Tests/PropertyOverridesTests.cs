using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Generation.Tests;

internal class PropertyOverridesTests
{
	[Test]
	public void TypeWithPropertyOverride_GeneratesCorrectly()
	{
		var builder = new JsonSchemaBuilder();
		builder.FromType<DerivedClassWithProperty>();

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "type": "object",
			  "properties": {
			    "MyProperty": {"type": ["null", "string"]}
			  }
			}
			""").RootElement;
	
		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}
}

public class BaseClassWithProperty
{
	public int MyProperty { get; set; }
}

public class DerivedClassWithProperty : BaseClassWithProperty
{
	public new string? MyProperty { get; set; }
}