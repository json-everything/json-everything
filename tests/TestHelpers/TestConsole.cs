namespace TestHelpers;

public static class TestConsole
{
	private static readonly bool _outputToConsole;

	static TestConsole()
	{
		var envVar = Environment.GetEnvironmentVariable("JSON_EVERYTHING_TEST_OUTPUT");
		_outputToConsole = !string.IsNullOrWhiteSpace(envVar) && bool.Parse(envVar);
	}

	public static void WriteLine()
	{
		if (!_outputToConsole) return;

		Console.WriteLine();
	}

	public static void WriteLine(object? value)
	{
		if (!_outputToConsole) return;

		Console.WriteLine(value);
	}

	public static void WriteLine(string? value)
	{
		if (!_outputToConsole) return;

		Console.WriteLine(value);
	}

	public static void WriteLine(string? format, params object?[]? values)
	{
		if (!_outputToConsole) return;
		if (format is null) return;

		Console.WriteLine(format, values);
	}
}