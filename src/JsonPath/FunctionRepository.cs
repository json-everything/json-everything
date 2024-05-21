using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Json.Path;

/// <summary>
/// Stores expression function definitions.
/// </summary>
public static class FunctionRepository
{
	private static readonly Dictionary<string, IPathFunctionDefinition> _functions = [];

	static FunctionRepository()
	{
		RegisterValueFunction<LengthFunction>();
		RegisterValueFunction<CountFunction>();
		RegisterLogicalFunction<MatchFunction>();
		RegisterLogicalFunction<SearchFunction>();
		RegisterValueFunction<ValueFunction>();
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	/// <param name="function">The function.</param>
	[RequiresUnreferencedCode("Use the overloads that take a Type for AOT compatibility")]
	public static void Register(ValueFunctionDefinition function)
	{
		Register(function, function.GetType());
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	/// <param name="function">The function.</param>
	/// <param name="functionType">The type of the function.</param>
	public static void Register(ValueFunctionDefinition function,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type functionType)
	{
		_functions[function.Name] = function;

		FindEvaluationMethods(function, functionType, typeof(JsonNode));
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	/// <param name="function">The function.</param>
	[RequiresUnreferencedCode("Use the overloads that take a Type for AOT compatibility")]
	public static void Register(LogicalFunctionDefinition function)
	{
		Register(function, function.GetType());
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	/// <param name="function">The function.</param>
	/// <param name="functionType">The type of the function.</param>
	public static void Register(LogicalFunctionDefinition function,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type functionType)
	{
		_functions[function.Name] = function;

		FindEvaluationMethods(function, functionType, typeof(bool));
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	/// <param name="function">The function.</param>
	[RequiresUnreferencedCode("Use the overloads that take a Type for AOT compatibility")]
	public static void Register(NodelistFunctionDefinition function)
	{
		Register(function, function.GetType());
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	/// <param name="function">The function.</param>
	/// <param name="functionType">The function type.</param>
	public static void Register(NodelistFunctionDefinition function,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type functionType)
	{
		_functions[function.Name] = function;

		FindEvaluationMethods(function, functionType, typeof(NodeList));
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	public static void RegisterValueFunction<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>() 
		where T : ValueFunctionDefinition, new ()
	{
		Register(new T(), typeof(T));
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	public static void RegisterLogicalFunction<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>() 
		where T : LogicalFunctionDefinition, new()
	{
		Register(new T(), typeof(T));
	}

	/// <summary>
	/// Registers a new function implementation, allowing it to be parsed.
	/// </summary>
	public static void RegisterNodelistFunction<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>()
		where T : NodelistFunctionDefinition, new()
	{
		Register(new T(), typeof(T));
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

	private static void FindEvaluationMethods(
		IReflectiveFunctionDefinition function,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type functionType,
		Type returnType)
	{
		var methods = functionType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.Name == "Evaluate" && m.ReturnType == returnType)
			.ToList();

		if (methods.Count == 0)
			throw new ArgumentException($"Cannot find an 'Evaluate' method that returns type '{returnType.FullName}'.");
		if (methods.Count > 1)
			throw new ArgumentException($"Found more than one 'Evaluate' method that returns type '{returnType.FullName}'.  Overloads are not permitted.");

		var method = methods[0];

		var parameters = method.GetParameters()
			.Select(x =>
			{
				if (x.ParameterType == typeof(JsonNode)) return FunctionType.Value;
				if (x.ParameterType == typeof(bool)) return FunctionType.Logical;
				if (x.ParameterType == typeof(NodeList)) return FunctionType.Nodelist;
				return (FunctionType?)null;
			}).ToArray();

		if (parameters.Any(x => x == null))
			throw new ArgumentException($"'Evaluate' method parameters may only be of types '{typeof(JsonNode)}', '{typeof(bool)}', or '{typeof(NodeList)}'.");

		function.Evaluator = (parameters.Cast<FunctionType>().ToArray(), method);
	}
}