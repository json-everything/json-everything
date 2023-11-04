using System.Collections.Generic;

namespace Json.JsonE.Expressions;

internal static class FunctionRepository
{
	private class FunctionRegistration
	{

	}

	private static readonly Dictionary<string, FunctionDefinition> _functions = new();

	static FunctionRepository()
	{
		//Register(new LengthFunction());
		//Register(new CountFunction());
		//Register(new MatchFunction());
		//Register(new SearchFunction());
		//Register(new ValueFunction());
	}

	private static void Register(FunctionDefinition function)
	{
		_functions[function.Name] = function;
	}

	public static bool TryGet(string name, out FunctionDefinition? function)
	{
		return _functions.TryGetValue(name, out function);
	}
}