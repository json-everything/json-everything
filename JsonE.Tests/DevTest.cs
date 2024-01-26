using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.JsonE.Expressions.Functions;
using Json.More;
using NUnit.Framework;

namespace Json.JsonE.Tests;

#if NET8_0_OR_GREATER

public class DevTest
{
	[Test]
	[Ignore("dev use only")]
	public void Check()
	{
		var value = JsonValue.Create(new MinFunction())!;

		var stored = value.GetValue<object>();
		Assert.IsInstanceOf<MinFunction>(stored);

		var copy = value.DeepClone();

		stored = copy.GetValue<object>();
		Assert.IsInstanceOf<MinFunction>(stored);
	}
}

#endif