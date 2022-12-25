using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Json.Path;

public static class FunctionRepository
{
	private static readonly Dictionary<string, IPathFunctionDefinition> _functions = new();

	static FunctionRepository()
	{
		Register(new LengthFunction());
	}

	public static void Register(IPathFunctionDefinition function)
	{
		_functions[function.Name] = function;
	}

	public static void Unregister<T>()
		where T : IPathFunctionDefinition
	{
		var function = _functions.Values.FirstOrDefault(x => x is T);
		if (function == null) return;

		_functions.Remove(function.Name);
	}

	public static bool TryGet(string name, [NotNullWhen(true)]out IPathFunctionDefinition? function)
	{
		return _functions.TryGetValue(name, out function);
	}
}