using BenchmarkDotNet.Attributes;
﻿using System.Linq;
using Json.Pointer;
using BenchmarkDotNet.Jobs;

namespace Json.Benchmarks.Pointer;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
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

	private static readonly JsonPointer_Old[] _pointers = _pointersToParse
		.Select(JsonPointer_Old.Parse)
		.ToArray();

	[Params(1, 10, 100)]
	public int Count { get; set; }

	[Benchmark]
	public int Parse()
	{
		for (int i = 0; i < Count; i++)
		{
			foreach (var test in _pointersToParse)
			{
				_ = JsonPointer_Old.Parse(test);
			}
		}

		return Count;
	}

	[Benchmark]
	public JsonPointer_Old Combine()
	{
		var p = JsonPointer_Old.Empty;

		for (int i = 0; i < Count; i++)
		{
			foreach (var test in _pointersToParse)
			{
				p = p.Combine(_segments[i % _segments.Length]);
			}
		}

		return p;
	}

	[Benchmark]
	public string PointerToString()
	{
		var s = string.Empty;

		for (int i = 0; i < Count; i++)
		{
			foreach (var test in _pointersToParse)
			{
				s = _pointers[i % _segments.Length].ToString();
			}
		}

		return s;
	}
}