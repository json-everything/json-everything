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
                ) {TestName = "Disjoint, A first"};
                yield return new TestCaseData(
                    new NumberRange(2, 3),
                    new NumberRange(0, 1),
                    new[] {new NumberRange(0, 1), new NumberRange(2, 3)}
                ) {TestName = "Disjoint, B first"};
                yield return new TestCaseData(
                    new NumberRange(0, 2),
                    new NumberRange(1, 3),
                    new[] {new NumberRange(0, 3)}
                ) {TestName = "Overlap, A first"};
                yield return new TestCaseData(
                    new NumberRange(1, 3),
                    new NumberRange(0, 2),
                    new[] {new NumberRange(0, 3)}
                ) {TestName = "Overlap, B first"};
                yield return new TestCaseData(
                    new NumberRange(0, 3),
                    new NumberRange(1, 2),
                    new[] {new NumberRange(0, 3)}
                ) {TestName = "Contained, A is outer"};
                yield return new TestCaseData(
                    new NumberRange(1, 2),
                    new NumberRange(0, 3),
                    new[] {new NumberRange(0, 3)}
                ) {TestName = "Contained, A is inner"};
                yield return new TestCaseData(
                    new NumberRange(1, (2, false)),
                    new NumberRange((2, false), 3),
                    new[] {new NumberRange(1, (2, false)), new NumberRange((2, false), 3)}
                ) {TestName = "Tangent, A ends exclusive, b starts exclusive"};
                yield return new TestCaseData(
                    new NumberRange(1, 2),
                    new NumberRange((2, false), 3),
                    new[] {new NumberRange(1, 3)}
                ) {TestName = "Tangent, A ends inclusive, b starts exclusive"};
                yield return new TestCaseData(
                    new NumberRange(1, (2, false)),
                    new NumberRange(2, 3),
                    new[] {new NumberRange(1, 3)}
                ) {TestName = "Tangent, A ends exclusive, b starts inclusive"};
                yield return new TestCaseData(
                    new NumberRange(1, 2),
                    new NumberRange(2, 3),
                    new[] {new NumberRange(1, 3)}
                ) {TestName = "Tangent, A ends inclusive, b starts inclusive"};
            }
        }

        [TestCaseSource(nameof(UnionCases))]
		public void Union(NumberRange a, NumberRange b, IEnumerable<NumberRange> expected)
		{
			var result = NumberRange.Union(a, b);

			result.Should().BeEquivalentTo(expected);
		}

        public static IEnumerable<TestCaseData> IntersectionCases
        {
            get
            {
                yield return new TestCaseData(
                    new NumberRange(0, 1),
                    new NumberRange(2, 3),
                    new NumberRange[] { }
                ) {TestName = "Disjoint, A first"};
                yield return new TestCaseData(
                    new NumberRange(2, 3),
                    new NumberRange(0, 1),
                    new NumberRange[] { }
                ) {TestName = "Disjoint, B first"};
                yield return new TestCaseData(
                    new NumberRange(0, 2),
                    new NumberRange(1, 3),
                    new[] {new NumberRange(1, 2)}
                ) {TestName = "Overlap, A first"};
                yield return new TestCaseData(
                    new NumberRange(1, 3),
                    new NumberRange(0, 2),
                    new[] {new NumberRange(1, 2)}
                ) {TestName = "Overlap, B first"};
                yield return new TestCaseData(
                    new NumberRange(0, 3),
                    new NumberRange(1, 2),
                    new[] {new NumberRange(1, 2)}
                ) {TestName = "Contained, A is outer"};
                yield return new TestCaseData(
                    new NumberRange(1, 2),
                    new NumberRange(0, 3),
                    new[] {new NumberRange(1, 2)}
                ) {TestName = "Contained, A is inner"};
                yield return new TestCaseData(
                    new NumberRange(1, (2, false)),
                    new NumberRange((2, false), 3),
                    new NumberRange[] { }
                ) {TestName = "Tangent, A ends exclusive, b starts exclusive"};
                yield return new TestCaseData(
                    new NumberRange(1, 2),
                    new NumberRange((2, false), 3),
                    new NumberRange[] { }
                ) {TestName = "Tangent, A ends inclusive, b starts exclusive"};
                yield return new TestCaseData(
                    new NumberRange(1, (2, false)),
                    new NumberRange(2, 3),
                    new NumberRange[] { }
                ) {TestName = "Tangent, A ends exclusive, b starts inclusive"};
                yield return new TestCaseData(
                    new NumberRange(1, 2),
                    new NumberRange(2, 3),
                    new[] {new NumberRange(2, 2)}
                ) {TestName = "Tangent, A ends inclusive, b starts inclusive"};
            }
        }

        [TestCaseSource(nameof(IntersectionCases))]
		public void Intersection(NumberRange a, NumberRange b, IEnumerable<NumberRange> expected)
		{
			var result = NumberRange.Intersection(a, b);

			result.Should().BeEquivalentTo(expected);
		}
	}
}
