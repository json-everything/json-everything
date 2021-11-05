using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests
{
	public class NumberRangeTests
	{
		public static IEnumerable<TestCaseData> UnionCases
		{
			get
			{
				yield return new TestCaseData(
					new NumberRange(0, 1),
					new NumberRange(2, 3),
					new[] {new NumberRange(0, 1), new NumberRange(2, 3)}
				) {TestName = "Disjoint"};
			}
		}

		[TestCaseSource(nameof(UnionCases))]
		public void Union(NumberRange a, NumberRange b, IEnumerable<NumberRange> expected)
		{
			var result = NumberRange.Union(a, b);

			result.Should().BeEquivalentTo(expected);
		}
	}
}
