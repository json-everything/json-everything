using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Logic.Tests
{
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
			var json = JsonDocument.Parse(text).RootElement;

			Assert.AreEqual(expected, json.IsTruthy());
		}
	}
}
