using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Pointer.Tests;

public class GithubTests
{
	[Test]
	public void Issue408_ArrayDerefThrowsIndexOutOfRange()
	{
		var pointer = JsonPointer.Parse("/");
		var data = JsonNode.Parse("[]");

		var success = pointer.TryEvaluate(data, out var array);

		Assert.Multiple(() =>
		{
			Assert.That(success, Is.False);
			Assert.That(array, Is.Null);
		});
	}
}