using BenchmarkDotNet.Attributes;
using Json.Pointer;

namespace Json.Benchmarks.Pointer;

[MemoryDiagnoser]
public class Runner
{
	private static readonly string[] _pointersToParse =
	[
		"",
		"/foo",
		"/foo/0",
		"/",
		"/a~1b",
		"/c%d",
		"/e^f",
		"/g|h",
		"/i\\j",
		"/k\"l",
		"/ ",
		"/m~0n",
		"/c%25d",
		"/e%5Ef",
		"/g%7Ch",
		"/i%5Cj",
		"/k%22l",
		"/%20",
// Url
		"#",
		"#/foo",
		"#/foo/0",
		"#/",
		"#/a~1b",
		"#/c%25d",
		"#/e%5Ef",
		"#/g%7Ch",
		"#/i%5Cj",
		"#/k%22l",
		"#/%20",
		"#/m~0n",
	];

	private static readonly string[] _segments = 
	[
		"user",
		"name",
		"age",
		"g%7Ch",
		"theme",
		"notifications",
		"email",
		"m~0n",
		"comments",
		"metadata",
		"version",
		"a~1b",
		"errors",
		"message",
		"a~0b"
	];

	[Params(1, 10, 100)]
	public int Count { get; set; }

	[Benchmark]
	public int Parse()
	{
		for (int i = 0; i < Count; i++)
		{
			foreach (var test in _pointersToParse)
			{
				_ = JsonPointer.Parse(test);
			}
		}

		return Count;
	}

	[Benchmark]
	public JsonPointer Combine()
	{
		var p = JsonPointer.Empty;

		for (int i = 0; i < Count; i++)
		{
			foreach (var test in _pointersToParse)
			{
				p = p.Combine(_segments[i % _segments.Length]);
			}
		}

		return p;
	}
}