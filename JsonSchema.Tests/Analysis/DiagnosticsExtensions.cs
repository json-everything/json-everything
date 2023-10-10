using System;
using System.Collections.Generic;
using Json.Pointer;
using Json.Schema.Analysis;

namespace Json.Schema.Tests.Analysis;

public static class DiagnosticsExtensions
{
	public static void Output(this IEnumerable<Diagnostic> diagnostics)
	{
		Console.WriteLine("| Level | Message | Location |");
		Console.WriteLine("|:------|:--------|:---------|");

		foreach (var diagnostic in diagnostics)
		{
			var location = diagnostic.Location.ToString();
			if (diagnostic.Location.Equals(JsonPointer.Empty))
				location = "(root)";

			Console.WriteLine($"| {diagnostic.Level} | {diagnostic.Message} | {location} |");
		}
	}
}