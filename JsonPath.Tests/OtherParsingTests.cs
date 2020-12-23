using System.Collections.Generic;
using NUnit.Framework;

namespace Json.Path.Tests
{
	public class OtherParsingTests
	{
		public static IEnumerable<TestCaseData> SuccessCases =>
			new[]
			{
				new TestCaseData("$.baz")
			};

		[TestCaseSource(nameof(SuccessCases))]
		public void ParseSingleProperty(string path)
		{
			JsonPath.Parse(path);
		}
	}
}
