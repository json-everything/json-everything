using System.Linq;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests;

internal class PropertyOverridesTests
{
	[Test]
	public void TypeWithPropertyOverride_GeneratesCorrectly()
	{
		var schema = new JsonSchemaBuilder().FromType<DerivedClassWithProperty>().Build();

		Assert.That(schema, Is.Not.Null, "Schema should not be null.");
		Assert.That(schema.GetProperties(), Is.Not.Null, "Schema should have properties.");
		Assert.That(schema.GetProperties().ContainsKey("MyProperty"), Is.True, "Schema should contain MyProperty.");
		Assert.That((schema.GetProperties()["MyProperty"].Keywords.First() as TypeKeyword).Type == (SchemaValueType.String | SchemaValueType.Null));
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