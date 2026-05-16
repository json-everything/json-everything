using System;
using System.Text.Json.Nodes;

namespace Json.Logic;

/// <summary>
/// Defines a logger that writes structured log entries represented as JSON nodes.
/// </summary>
/// <remarks>Implementations of this interface can be used to record log information in a structured format,
/// enabling easier parsing and analysis of log data. The interface is intended for scenarios where log content is best
/// represented as JSON, such as for telemetry or diagnostics in distributed systems.</remarks>
public interface ILogicLogger
{
	/// <summary>
	/// Writes the specified JSON content followed by a line terminator.
	/// </summary>
	/// <param name="content">The JSON content to write. Can be null, in which case no content is written but a line terminator is still
	/// appended.</param>
	void WriteLine(JsonNode? content);
}

/// <summary>
/// Provides commonly used implementations of the ILogicLogger interface for application logic logging.
/// </summary>
/// <remarks>This static class offers ready-to-use logger instances, including a console logger for outputting log
/// messages to the console and a null logger that ignores all log messages. These loggers can be used to simplify
/// logging setup in applications or for testing purposes.</remarks>
public static class LogicLoggers
{
	/// <summary>
	/// Gets a logger instance that writes log messages to the console.
	/// </summary>
	public static ILogicLogger ConsoleLogger { get; } = new ConsoleLogger();
	/// <summary>
	/// Gets a logger instance that performs no logging operations.  This is the default logger.
	/// </summary>
	/// <remarks>This property provides a no-op implementation of the logger interface. It can be used in scenarios
	/// where logging is optional or should be suppressed, such as in testing or when a logger is required but no actual
	/// logging is desired.</remarks>
	public static ILogicLogger NullLogger { get; } = new NullLogger();
}

internal class NullLogger : ILogicLogger
{
	public void WriteLine(JsonNode? content)
	{
	}
}

internal class ConsoleLogger : ILogicLogger
{
	public void WriteLine(JsonNode? content)
	{
		Console.WriteLine(content);
	}
}