using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Json.Path.Tests
{
	public class InternalJsonParsingTests
	{
		[TestCaseSource(nameof(GetJson))]
		public static void Parse(string json)
		{
			int i = 0;
			if (!json.AsSpan().TryParseJsonElement(ref i, out var element))
				Assert.Fail();

			Console.WriteLine(element);
		}

		private static IEnumerable<string> GetJson()
		{
			return new[]
			{
				"1+5",
				"true",
				"false",
				"true and some more",
				"false this is different",
				"5.6/7",
				"\"a string\"; then this",
				"[null, true]; and some more",
				"{\"key\": \"value\"}, then the end"
			};
		}
	}
}