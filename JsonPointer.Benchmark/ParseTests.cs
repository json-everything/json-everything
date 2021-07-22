using System;
using System.Diagnostics;
using System.Linq;

namespace JsonPointer.Benchmark
{
	public static class ParseTests
	{
		public static void Run()
		{
			Console.WriteLine("Parsing Tests");
			Console.WriteLine("=============");
			Console.WriteLine();
			_InitializeTypes();
			_RunParse("JsonPointer", _ParseLocal, _CheckLocal);
			_RunParse("Manatee.Json", _ParseManatee, _CheckManatee);
			Console.WriteLine();
			Console.WriteLine();
		}

		private static void _InitializeTypes()
		{
			var pointer = "/this/is/definitely/valid";
			Json.Pointer.JsonPointer.Parse(pointer);
			Manatee.Json.Pointer.JsonPointer.Parse(pointer);
		}

		private static void _RunParse(string testName, Func<string, object> runParse, Func<string, object, bool, string[], bool, bool> check)
		{
			Console.WriteLine(testName);
			Console.WriteLine(new string('-', testName.Length));

			var items = new[]
				{
					new {pointer = "", isValid = true, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "/foo", isValid = true, segments = new[] {"foo"}, isUriEncoded = false},
					new {pointer = "/foo/0", isValid = true, segments = new[] {"foo", "0"}, isUriEncoded = false},
					new {pointer = "/", isValid = true, segments = new[] {""}, isUriEncoded = false},
					new {pointer = "/a~1b", isValid = true, segments = new[] {"a/b"}, isUriEncoded = false},
					new {pointer = "/c%d", isValid = true, segments = new[] {"c%d"}, isUriEncoded = false},
					new {pointer = "/e^f", isValid = true, segments = new[] {"e^f"}, isUriEncoded = false},
					new {pointer = "/g|h", isValid = true, segments = new[] {"g|h"}, isUriEncoded = false},
					new {pointer = "/i\\j", isValid = true, segments = new[] {"i\\j"}, isUriEncoded = false},
					new {pointer = "/k\"l", isValid = true, segments = new[] {"k\"l"}, isUriEncoded = false},
					new {pointer = "/ ", isValid = true, segments = new[] {" "}, isUriEncoded = false},
					new {pointer = "/m~0n", isValid = true, segments = new[] {"m~n"}, isUriEncoded = false},
					new {pointer = "/c%25d", isValid = true, segments = new[] {"c%25d"}, isUriEncoded = false},
					new {pointer = "/e%5Ef", isValid = true, segments = new[] {"e%5Ef"}, isUriEncoded = false},
					new {pointer = "/g%7Ch", isValid = true, segments = new[] {"g%7Ch"}, isUriEncoded = false},
					new {pointer = "/i%5Cj", isValid = true, segments = new[] {"i%5Cj"}, isUriEncoded = false},
					new {pointer = "/k%22l", isValid = true, segments = new[] {"k%22l"}, isUriEncoded = false},
					new {pointer = "/%20", isValid = true, segments = new[] {"%20"}, isUriEncoded = false},
					new {pointer = "#", isValid = true, segments = new string[] { }, isUriEncoded = true},
					new {pointer = "#/foo", isValid = true, segments = new[] {"foo"}, isUriEncoded = true},
					new {pointer = "#/foo/0", isValid = true, segments = new[] {"foo", "0"}, isUriEncoded = true},
					new {pointer = "#/", isValid = true, segments = new[] {""}, isUriEncoded = true},
					new {pointer = "#/a~1b", isValid = true, segments = new[] {"a/b"}, isUriEncoded = true},
					new {pointer = "#/c%25d", isValid = true, segments = new[] {"c%d"}, isUriEncoded = true},
					new {pointer = "#/e%5Ef", isValid = true, segments = new[] {"e^f"}, isUriEncoded = true},
					new {pointer = "#/g%7Ch", isValid = true, segments = new[] {"g|h"}, isUriEncoded = true},
					new {pointer = "#/i%5Cj", isValid = true, segments = new[] {"i\\j"}, isUriEncoded = true},
					new {pointer = "#/k%22l", isValid = true, segments = new[] {"k\"l"}, isUriEncoded = true},
					new {pointer = "#/%20", isValid = true, segments = new[] {" "}, isUriEncoded = true},
					new {pointer = "#/m~0n", isValid = true, segments = new[] {"m~n"}, isUriEncoded = true},
					new {pointer = "starts/with/segment", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "/invalid/escap~e/sequence", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "/ends/with~", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "#uses/anchor/name", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "#/invalid/escap~e/sequence", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "#/ends/with~", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "#/invalid/hex/%2h", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "#/short/%2/hex", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "#/end/short/hex/%5", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "#/missing/value/%/hex", isValid = false, segments = new string[] { }, isUriEncoded = false},
					new {pointer = "#/end/missing/value/hex/%", isValid = false, segments = new string[] { }, isUriEncoded = false},
				};
			var testCount = 10000;

			var stopwatch = new Stopwatch();
			int score = 0;
			for (int i = 0; i < testCount; i++)
			{
				var item = items[i % items.Length];
				stopwatch.Start();
				var pointerObj = runParse(item.pointer);
				stopwatch.Stop();
				score += check(item.pointer, pointerObj, item.isValid, item.segments, item.isUriEncoded) ? 1 : 0;
			}

			Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");
			Console.WriteLine($"  Score: {score*100/testCount}%");
		}

		private static object _ParseLocal(string source)
		{
			return Json.Pointer.JsonPointer.TryParse(source, out var pointer) ? (object)pointer : null;
		}

		private static bool _CheckLocal(string source, object pointerObj, bool isValid, string[] expectedSegments, bool isUriEncoded)
		{
			if (pointerObj == null) return !isValid;
			if (!isValid) return false;
			var pointer = (Json.Pointer.JsonPointer)pointerObj;
			if (expectedSegments.Length != pointer.Segments.Length) return false;
			if (expectedSegments.Where((t, i) => t != pointer.Segments[i].Value).Any()) return false;
			if (pointer.ToString() != source) return false;

			return pointer.IsUriEncoded == isUriEncoded;
		}

		private static object _ParseManatee(string source)
		{
			try
			{
				return Manatee.Json.Pointer.JsonPointer.Parse(source);
			}
			catch
			{
				return null;
			}

		}

		private static bool _CheckManatee(string source, object pointerObj, bool isValid, string[] expectedSegments, bool isUriEncoded)
		{
			if (pointerObj == null) return !isValid;
			if (!isValid) return false;
			var pointer = (Manatee.Json.Pointer.JsonPointer)pointerObj;
			if (expectedSegments.Length != pointer.Count) return false;
			if (expectedSegments.Where((t, i) => t != pointer[i]).Any()) return false;
			if (pointer.ToString() != source) return false;

			return pointer.ToString().StartsWith("#") == isUriEncoded;
		}
	}
}