using System.Collections.Generic;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class ExtensionTests
	{
		public static IEnumerable<TestCaseData> ArrayCases
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

		[TestCaseSource(nameof(ArrayCases))]
		public void CollectionsAgree(IReadOnlyList<int> a, IReadOnlyList<int> b, bool matches)
		{
			Assert.AreEqual(matches, a.ContentsEqual(b));
			Assert.AreEqual(matches, b.ContentsEqual(a));
			Assert.AreEqual(matches, a.GetUnorderedCollectionHashCode() == b.GetUnorderedCollectionHashCode());
		}

		public static IEnumerable<TestCaseData> DictionaryCases
		{
			get
			{
				yield return new TestCaseData(
					new Dictionary<string, int>
					{
						["1"] = 1,
						["2"] = 2,
						["3"] = 3,
					},
					new Dictionary<string, int>()
					{
						["1"] = 1,
						["2"] = 2,
						["3"] = 3,
					},
					true
				) {TestName = "Dictionary 1"};
				yield return new TestCaseData(
					new Dictionary<string, int>
					{
						["1"] = 1,
						["2"] = 2,
						["3"] = 3,
					},
					new Dictionary<string, int>()
					{
						["1"] = 1,
						["3"] = 3,
						["2"] = 2,
					},
					true
				) {TestName = "Dictionary 2" };
				yield return new TestCaseData(
					new Dictionary<string, int>
					{
						["1"] = 1,
						["2"] = 2,
						["3"] = 3,
					},
					new Dictionary<string, int>()
					{
						["3"] = 3,
						["2"] = 2,
						["1"] = 1,
					},
					true
				) {TestName = "Dictionary 3" };
				yield return new TestCaseData(
					new Dictionary<string, int>
					{
						["1"] = 1,
						["4"] = 2,
						["3"] = 3,
					},
					new Dictionary<string, int>()
					{
						["1"] = 1,
						["2"] = 2,
						["3"] = 3,
					},
					false
				) {TestName = "Dictionary 4" };
				yield return new TestCaseData(
					new Dictionary<string, int>
					{
						["1"] = 1,
						["2"] = 2,
						["3"] = 3,
					},
					new Dictionary<string, int>()
					{
						["1"] = 3,
						["2"] = 2,
						["3"] = 1,
					},
					false
				) {TestName = "Dictionary 5" };
			}
		}

		[TestCaseSource(nameof(DictionaryCases))]
		public void CollectionsAgree(IReadOnlyDictionary<string, int> a, IReadOnlyDictionary<string, int> b, bool matches)
		{
			Assert.AreEqual(matches, a.GetStringDictionaryHashCode() == b.GetStringDictionaryHashCode());
		}
	}
}