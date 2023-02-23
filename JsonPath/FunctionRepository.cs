using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Json.Path;

/// <summary>
/// Stores expression function definitions.
/// </summary>
public static class FunctionRepository
{
	private static readonly Dictionary<string, IPathFunctionDefinition> _functions = new();

	static FunctionRepository()
	{
		Register(new LengthFunction());
		Register(new CountFunction());
		Register(new MatchFunction());
		Register(new SearchFunction());
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	/// <param name="function">The function.</param>
	public static void Register(IPathFunctionDefinition function)
	{
		_functions[function.Name] = function;
	}

	/// <summary>
	/// Unregisters a function implementation.
	/// </summary>
	/// <typeparam name="T">The type of function.</typeparam>
	public static void Unregister<T>()
		where T : IPathFunctionDefinition
	{
		var function = _functions.Values.FirstOrDefault(x => x is T);
		if (function == null) return;

		_functions.Remove(function.Name);
	}

	/// <summary>
	/// Unregisters a function implementation.
	/// </summary>
	public static void Unregister(string name)
	{
		_functions.Remove(name);
	}

	/// <summary>
	/// Gets a function implementation by name.
	/// </summary>
	/// <param name="name">A function name.</param>
	/// <param name="function">The function, if found; otherwise null.</param>
	/// <returns>True if found; otherwise false.</returns>
	public static bool TryGet(string name, [NotNullWhen(true)] out IPathFunctionDefinition? function)
	{
		return _functions.TryGetValue(name, out function);
	}
}