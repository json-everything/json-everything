using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Common;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class MetaTests
	{
		public static IEnumerable<TestCaseData> Keywords =>
			typeof(IJsonSchemaKeyword)
				.Assembly
				.DefinedTypes
				.Where(t => t.Implements(typeof(IJsonSchemaKeyword)) &&
				            !t.IsAbstract &&
				            !t.IsInterface)
				.Select(t => new TestCaseData(t){TestName = t.Name});

		[TestCaseSource(nameof(Keywords))]
		public void AllKeywordsImplementIEquatable(Type keywordType)
		{
			var equatableType = typeof(IEquatable<>).MakeGenericType(keywordType);
			Assert.IsTrue(keywordType.Implements(equatableType));
		}
	}
}