using System.Text.Json.Nodes;
using Json.JsonE.Expressions.Functions;
using NUnit.Framework;

namespace Json.JsonE.Tests;

public class DevTest
{
	[Test]
	[Ignore("dev use only")]
	public void Check()
	{
		var value = JsonValue.Create(new MinFunction())!;

		var stored = value.GetValue<object>();
		Assert.That(stored, Is.InstanceOf<MinFunction>());

		var copy = value.DeepClone();

		stored = copy.GetValue<object>();
		Assert.That(stored, Is.InstanceOf<MinFunction>());
	}
}
