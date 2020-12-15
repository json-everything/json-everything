using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Logic.Tests
{
	public class Tests
	{
		[Test]
		public void BasicEquals_False()
		{
			var rule = new EqualsOperator(new Literal(1), new Literal(2));

			Assert.AreEqual(JsonValueKind.False, rule.Apply("null".AsJsonElement()).ValueKind);
		}

		[Test]
		public void BasicEquals_True()
		{
			var rule = new EqualsOperator(new Literal(1), new Literal(1));

			Assert.AreEqual(JsonValueKind.True, rule.Apply("null".AsJsonElement()).ValueKind);
		}
	}
}