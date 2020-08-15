using System;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Json.More;
using Manatee.Json;

namespace JsonPointer.Benchmark
{
	public class EvaluateTests
	{
		public static void Run()
		{
			Console.WriteLine("Evaluation Tests");
			Console.WriteLine("=============");
			Console.WriteLine();
			_InitializeTypes();
			_RunEvaluate("JsonPointer", _EvalLocal);
			_RunEvaluate("Manatee.Json", _EvalManatee);
			Console.WriteLine();
			Console.WriteLine();
		}

		private static void _InitializeTypes()
		{
			 
		}

		private static void _RunEvaluate(string testName, Func<string, string, string> runEval)
		{
			Console.WriteLine(testName);
			Console.WriteLine(new string('-', testName.Length));

			var specTestJson = "{\"foo\":[\"bar\",\"baz\"],\"\":0,\"a/b\":1,\"c%d\":2,\"e^f\":3,\"g|h\":4,\"i\\\\j\":5,\"k\\\"l\":6,\" \":7,\"m~n\":8}";
			var specTestJsonUnencoded = "{\"foo\":[\"bar\",\"baz\"],\"\":0,\"a/b\":1,\"c%25d\":2,\"e%5Ef\":3,\"g%7Ch\":4,\"i%5Cj\":5,\"k%22l\":6,\"%20\":7,\"m~n\":8}";
			var items = new[]
				{
					new {pointer = "", expected = specTestJson, json = specTestJson},
					new {pointer = "/foo", expected = "[\"bar\",\"baz\"]", json = specTestJson},
					new {pointer = "/foo/0", expected = "\"bar\"", json = specTestJson},
					new {pointer = "/", expected = "0", json = specTestJson},
					new {pointer = "/a~1b", expected = "1", json = specTestJson},
					new {pointer = "/c%d", expected = "2", json = specTestJson},
					new {pointer = "/e^f", expected = "3", json = specTestJson},
					new {pointer = "/g|h", expected = "4", json = specTestJson},
					new {pointer = "/i\\j", expected = "5", json = specTestJson},
					new {pointer = "/k\"l", expected = "6", json = specTestJson},
					new {pointer = "/ ", expected = "7", json = specTestJson},
					new {pointer = "/m~0n", expected = "8", json = specTestJson},
					new {pointer = "/c%25d", expected = "2", json = specTestJsonUnencoded},
					new {pointer = "/e%5Ef", expected = "3", json = specTestJsonUnencoded},
					new {pointer = "/g%7Ch", expected = "4", json = specTestJsonUnencoded},
					new {pointer = "/i%5Cj", expected = "5", json = specTestJsonUnencoded},
					new {pointer = "/k%22l", expected = "6", json = specTestJsonUnencoded},
					new {pointer = "/%20", expected = "7", json = specTestJsonUnencoded},
					new {pointer = "#", expected = specTestJson, json = specTestJson},
					new {pointer = "#/foo", expected = "[\"bar\",\"baz\"]", json = specTestJson},
					new {pointer = "#/foo/0", expected = "\"bar\"", json = specTestJson},
					new {pointer = "#/", expected = "0", json = specTestJson},
					new {pointer = "#/a~1b", expected = "1", json = specTestJson},
					new {pointer = "#/c%25d", expected = "2", json = specTestJson},
					new {pointer = "#/e%5Ef", expected = "3", json = specTestJson},
					new {pointer = "#/g%7Ch", expected = "4", json = specTestJson},
					new {pointer = "#/i%5Cj", expected = "5", json = specTestJson},
					new {pointer = "#/k%22l", expected = "6", json = specTestJson},
					new {pointer = "#/%20", expected = "7", json = specTestJson},
					new {pointer = "#/m~0n", expected = "8", json = specTestJson},
				};
			var testCount = 10000;

			var stopwatch = new Stopwatch();
			int score = 0;
			for (int i = 0; i < testCount; i++)
			{
				var item = items[i % items.Length];
				stopwatch.Start();
				var result = runEval(item.pointer, item.json);
				stopwatch.Stop();
				score += _Check(result, item.expected) ? 1 : 0;
			}

			Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds}ms");
			Console.WriteLine($"  Score: {score * 100 / testCount}%");
		}

		private static string _EvalLocal(string pointerString, string jsonString)
		{
			using var document = JsonDocument.Parse(jsonString);
			var pointer = Json.Pointer.JsonPointer.Parse(pointerString);

			var result = pointer.Evaluate(document.RootElement);

			var evalLocal = JsonSerializer.Serialize(result, new JsonSerializerOptions {Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping});
			return evalLocal;
		}

		private static string _EvalManatee(string pointerString, string jsonString)
		{
			var json = JsonValue.Parse(jsonString);
			var pointer = Manatee.Json.Pointer.JsonPointer.Parse(pointerString);

			var result = pointer.Evaluate(json);

			return result.Result?.ToString();
		}
		
		private static bool _Check(string actual, string expected)
		{
			if (actual != null)
			{
				using var actualJson = JsonDocument.Parse(actual);
				using var expectedJson = JsonDocument.Parse(expected);

				if (actualJson.IsEquivalentTo(expectedJson)) return true;
			}

			//Console.WriteLine(actual); 
			//Console.WriteLine(expected);

			return false;
		}
	}
}