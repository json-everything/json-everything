using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Json.Path.Tests
{
	public class OtherParsingTests
	{
		public static IEnumerable<TestCaseData> SuccessCases =>
			new[]
			{
				new TestCaseData("$.baz"),
				new TestCaseData("$[?(@.foo)]"),
				new TestCaseData("$[?(@.foo && @.bar)]"),
				new TestCaseData("$[?(!@.foo)]"),
				new TestCaseData("$[?(@.foo && !@.bar)]"),
				new TestCaseData("$[?(!@.foo == false)]"),
				new TestCaseData("$[?(@.foo == !false)]"),
				new TestCaseData("$[?(@.foo==(4+5))]"),
				new TestCaseData("$[?(@.foo==2*(4+5))]"),
				new TestCaseData("$[?(@.foo==2+(4+5))]"),
				new TestCaseData("$[?(@.foo==2-(4+5))]"),
				new TestCaseData("$[?(@['name'] == null || @['name'] == 'abc')]"),
			};

		[TestCaseSource(nameof(SuccessCases))]
		public void Parse(string path)
		{
			Console.WriteLine(JsonPath.Parse(path));
		}
	}
}
