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

		Assert.AreEqual(1, result.Matches!.Count);

		data[0] = "sally";

		Assert.AreEqual(2, result.Matches!.Count);
	}
}