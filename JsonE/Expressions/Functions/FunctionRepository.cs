using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions.Functions;

internal static class FunctionRepository
{
	private static readonly Dictionary<string, FunctionDefinition> _functions =
		typeof(FunctionRepository)
			.Assembly
			.DefinedTypes
			.Where(x => typeof(FunctionDefinition).IsAssignableFrom(x) &&
						!x.IsAbstract &&
						x != typeof(ContextedFunction))
			.Select(x => (FunctionDefinition)Activator.CreateInstance(x))
			.ToDictionary(x => x.Name);

	public static FunctionDefinition Get(string name)
	{
		return _functions.TryGetValue(name, out var function)
			? function
			: new ContextedFunction(name);
	}
}