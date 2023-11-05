using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.JsonE.Functions;

internal static class FunctionRepository
{
	private static readonly Dictionary<string, FunctionDefinition> _functions =
		typeof(FunctionRepository)
			.Assembly
			.DefinedTypes
			.Where(x => typeof(FunctionDefinition).IsAssignableFrom(x) &&
			            !x.IsAbstract)
			.Select(x => (FunctionDefinition)Activator.CreateInstance(x))
			.ToDictionary(x => x.Name);

	public static bool TryGet(string name, out FunctionDefinition? function)
	{
		return _functions.TryGetValue(name, out function);
	}
}