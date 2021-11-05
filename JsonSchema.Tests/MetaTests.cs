using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class MetaTests
	{
		public static IEnumerable<TestCaseData> Keywords =>
			typeof(IJsonSchemaKeyword)
				.Assembly
				.DefinedTypes
				.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
				            !t.IsAbstract &&
				            !t.IsInterface)
				.Select(t => new TestCaseData(t) {TestName = t.Name});

		[TestCaseSource(nameof(Keywords))]
		public void AllKeywordsImplementIEquatable(Type keywordType)
		{
			var equatableType = typeof(IEquatable<>).MakeGenericType(keywordType);
			Assert.IsTrue(equatableType.IsAssignableFrom(keywordType));
		}
	}
}