using System;
using System.Collections.Generic;
using System.Linq;
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
	internal (FunctionType[] ArgTypes, MethodInfo Method)[]? Evaluators { get; set; }
}

internal class ArgumentTypeComparer : IEqualityComparer<FunctionType>
{
	public static ArgumentTypeComparer Instance { get; } = new();

	private ArgumentTypeComparer(){}

	public bool Equals(FunctionType x, FunctionType y)
	{
		return x != FunctionType.Unspecified && y != FunctionType.Unspecified &&
		       x.HasFlag(y);
	}

	public int GetHashCode(FunctionType obj)
	{
		return 0;
	}
}

public abstract class ValueFunctionDefinition : IReflectiveFunctionDefinition, IPathFunctionDefinition
{
	public abstract string Name { get; }

	(FunctionType[] ArgTypes, MethodInfo Method)[]? IReflectiveFunctionDefinition.Evaluators { get; set; }

	internal PathValue? Invoke(object?[] arguments)
	{
		// TODO: support for overloads
		//var argumentTypes = arguments.ToArgumentTypes();
		//var method = ((IReflectiveFunctionDefinition)this).Evaluators?
		//	.FirstOrDefault(x => x.ArgTypes.SequenceEqual(argumentTypes, ArgumentTypeComparer.Instance))
		//	.Method;

		var (parameterTypes, method) = ((IReflectiveFunctionDefinition)this).Evaluators!.Single();

		if (method == null)
			throw new InvalidOperationException("Cannot find appropriate method. This should have been caught during parsing.");

		var result = (JsonNode?)method.Invoke(this, arguments.ExtractArgumentValues(parameterTypes));

		return result;
	}
}

public abstract class LogicalFunctionDefinition : IReflectiveFunctionDefinition, IPathFunctionDefinition
{
	public abstract string Name { get; }

	(FunctionType[] ArgTypes, MethodInfo Method)[]? IReflectiveFunctionDefinition.Evaluators { get; set; }

	internal bool? Invoke(object?[] arguments)
	{
		// TODO: support for overloads
		//var argumentTypes = arguments.ToArgumentTypes();
		//var method = ((IReflectiveFunctionDefinition)this).Evaluators?
		//	.FirstOrDefault(x => x.ArgTypes.SequenceEqual(argumentTypes, ArgumentTypeComparer.Instance))
		//	.Method;

		var (parameterTypes, method) = ((IReflectiveFunctionDefinition)this).Evaluators!.Single();

		if (method == null)
			throw new InvalidOperationException("Cannot find appropriate method. This should have been caught during parsing.");

		return (bool?)method.Invoke(this, arguments.ExtractArgumentValues(parameterTypes));
	}
}

public abstract class NodelistFunctionDefinition : IReflectiveFunctionDefinition, IPathFunctionDefinition
{
	public abstract string Name { get; }

	(FunctionType[] ArgTypes, MethodInfo Method)[]? IReflectiveFunctionDefinition.Evaluators { get; set; }

	internal NodeList? Invoke(object?[] arguments)
	{
		// TODO: support for overloads
		//var argumentTypes = arguments.ToArgumentTypes();
		//var method = ((IReflectiveFunctionDefinition)this).Evaluators?
		//	.FirstOrDefault(x => x.ArgTypes.SequenceEqual(argumentTypes, ArgumentTypeComparer.Instance))
		//	.Method;

		var (parameterTypes, method) = ((IReflectiveFunctionDefinition)this).Evaluators!.Single();

		if (method == null)
			throw new InvalidOperationException("Cannot find appropriate method. This should have been caught during parsing.");

		return (NodeList?)method.Invoke(this, arguments.ExtractArgumentValues(parameterTypes));
	}
}