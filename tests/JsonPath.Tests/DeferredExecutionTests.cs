using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Path.Tests;

public class DeferredExecutionTests
{
	[Test]
	public void RepeatedQueryOnChangedDataSet()
	{
		var data = new JsonArray("bob", "sam", "alice");
		var query = JsonPath.Parse("$[? length(@) > 3 ]");

		var result = query.Evaluate(data);

		Assert.That(result.Matches!, Has.Count.EqualTo(1));

		data[0] = "sally";

		Assert.That(result.Matches!, Has.Count.EqualTo(2));
	}
}