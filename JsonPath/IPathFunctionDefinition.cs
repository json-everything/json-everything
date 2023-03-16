using System;
using System.Reflection;
using System.Text.Json.Nodes;
using Json.Path.Expressions;

namespace Json.Path;

/// <summary>
/// Defines properties and methods required for an expression function.
/// </summary>
/// <remarks>Functions must be registered with one of the static `Register()`
/// methods defined on <see cref="FunctionRepository"/></remarks>
public interface IPathFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	string Name { get; }
}

internal interface IReflectiveFunctionDefinition
{
	internal (FunctionType[] ArgTypes, MethodInfo Method) Evaluator { get; set; }
}

/// <summary>
/// Base class for defining an expression function which returns `ValueType`.
/// </summary>
public abstract class ValueFunctionDefinition : IReflectiveFunctionDefinition, IPathFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public abstract string Name { get; }

	(FunctionType[] ArgTypes, MethodInfo Method) IReflectiveFunctionDefinition.Evaluator { get; set; }

	internal PathValue? Invoke(object?[] arguments)
	{
		var (parameterTypes, method) = ((IReflectiveFunctionDefinition)this).Evaluator;

		if (method == null)
			throw new InvalidOperationException("Cannot find appropriate method. This should have been caught during parsing.");

		var result = (JsonNode?)method.Invoke(this, arguments.ExtractArgumentValues(parameterTypes));

		return result;
	}
}

/// <summary>
/// Base class for defining an expression function which returns `LogicalType`.
/// </summary>
public abstract class LogicalFunctionDefinition : IReflectiveFunctionDefinition, IPathFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public abstract string Name { get; }

	(FunctionType[] ArgTypes, MethodInfo Method) IReflectiveFunctionDefinition.Evaluator { get; set; }

	internal bool? Invoke(object?[] arguments)
	{
		var (parameterTypes, method) = ((IReflectiveFunctionDefinition)this).Evaluator;

		if (method == null)
			throw new InvalidOperationException("Cannot find appropriate method. This should have been caught during parsing.");

		return (bool?)method.Invoke(this, arguments.ExtractArgumentValues(parameterTypes));
	}
}

/// <summary>
/// Base class for defining an expression function which returns `NodesType`.
/// </summary>
public abstract class NodelistFunctionDefinition : IReflectiveFunctionDefinition, IPathFunctionDefinition
{
	/// <summary>
	/// Gets the function name.
	/// </summary>
	public abstract string Name { get; }

	(FunctionType[] ArgTypes, MethodInfo Method) IReflectiveFunctionDefinition.Evaluator { get; set; }

	internal NodeList? Invoke(object?[] arguments)
	{
		var (parameterTypes, method) = ((IReflectiveFunctionDefinition)this).Evaluator;

		if (method == null)
			throw new InvalidOperationException("Cannot find appropriate method. This should have been caught during parsing.");

		return (NodeList?)method.Invoke(this, arguments.ExtractArgumentValues(parameterTypes));
	}
}