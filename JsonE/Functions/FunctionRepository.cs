using System;
using System.Collections.Generic;
using Json.JsonE.Expressions;

namespace Json.JsonE.Functions;

internal static class FunctionRepository
{
	private static readonly Dictionary<string, FunctionDefinition> _functions = new();

	static FunctionRepository()
	{
		Register(new LenFunction());
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