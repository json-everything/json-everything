using Json.Pointer;

namespace Json.Schema.Linting.Tests;

public static class DiagnosticsExtensions
{
	public static void Output(this IEnumerable<Diagnostic> diagnostics)
	{
		Console.WriteLine("| Level | Message | Location | Keyword | Rule |");
		Console.WriteLine("|:------|:--------|:---------|:--------|:-----|");

		foreach (var diagnostic in diagnostics)
		{
			var location = diagnostic.Location.ToString();
			if (diagnostic.Location.Equals(JsonPointer.Empty))
				location = "(root)";

			Console.WriteLine($"| {diagnostic.Level} | {diagnostic.Message} | {location} | {diagnostic.Target} | {diagnostic.RuleId.Split(':').Last()} |");
		}
	}
}