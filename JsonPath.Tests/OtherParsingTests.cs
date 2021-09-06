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
				new TestCaseData("$[?(@['name'] == null || @['name'] == 'abc')]"),
			};

		[TestCaseSource(nameof(SuccessCases))]
		public void ParseSingleProperty(string path)
		{
			JsonPath.Parse(path);
		}
	}
}
