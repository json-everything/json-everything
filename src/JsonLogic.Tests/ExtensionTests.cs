using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Logic.Tests;

public class ExtensionTests
{
	[TestCase("0", false)]
	[TestCase("1", true)]
	[TestCase("-1", true)]
	[TestCase("[]", false)]
	[TestCase("[1,2]", true)]
	[TestCase("\"\"", false)]
	[TestCase("\"anything\"", true)]
	[TestCase("null", false)]
	public void Truthiness(string text, bool expected)
	{
		var json = JsonNode.Parse(text);

		Assert.AreEqual(expected, json.IsTruthy());
	}
}