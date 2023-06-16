using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class TraversalTests
{
	[Test]
	public async Task PropertyDependenciesCanBeNavigated()
	{
		var schema = new JsonSchemaBuilder()
			.Id("https://traversal.test")
			.PropertyDependencies(
				("foo", new[]
					{
						("foo1", new JsonSchemaBuilder().Required("foo1")),
						("foo2", new JsonSchemaBuilder().Required("foo2")),
					}
				)
			)
			.Ref("#/propertyDependencies/foo/foo1")
			;

		var instance = new JsonObject
		{
			["bar"] = true
		};

		var result = await schema.Evaluate(instance);

		result.AssertInvalid();
	}
}