using System.Collections.Generic;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class ExtensionTests
	{
		public static IEnumerable<TestCaseData> ContentsEqualCases
		{
			get
			{
				yield return new TestCaseData(new[] {1, 2, 3}, new[] {1, 2, 3}, true);
				yield return new TestCaseData(new[] {1, 2, 3}, new[] {3, 1, 2}, true);
				yield return new TestCaseData(new[] {1, 1, 3}, new[] {3, 1, 1}, true);
				yield return new TestCaseData(new[] {1, 1, 3}, new[] {3, 1, 2}, false);
				yield return new TestCaseData(new[] {1, 1, 3, 2}, new[] {3, 1, 2, 1}, true);
				yield return new TestCaseData(new[] {1, 1, 3, 2}, new[] {3, 1, 2, 2}, false);
			}
		}

		[TestCaseSource(nameof(ContentsEqualCases))]
		public void CollectionsAgree(IReadOnlyList<int> a, IReadOnlyList<int> b, bool matches)
		{
			Assert.AreEqual(matches, a.ContentsEqual(b));
			Assert.AreEqual(matches, b.ContentsEqual(a));
		}
	}
}