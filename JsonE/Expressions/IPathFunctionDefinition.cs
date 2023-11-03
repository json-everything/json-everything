using System;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Json.JsonE.Expressions;

internal interface IFunctionDefinition
{
	string Name { get; }
}

internal interface IReflectiveFunctionDefinition
{
	internal (FunctionArgType[] ArgTypes, MethodInfo Method) Evaluator { get; set; }
}

internal abstract class FunctionDefinition : IReflectiveFunctionDefinition, IFunctionDefinition
{
	public abstract string Name { get; }

	(FunctionArgType[] ArgTypes, MethodInfo Method) IReflectiveFunctionDefinition.Evaluator { get; set; }

	internal JsonNode? Invoke(JsonNode?[] arguments)
	{
		var (_, method) = ((IReflectiveFunctionDefinition)this).Evaluator;

		if (method == null)
			throw new InvalidOperationException("Cannot find appropriate method. This should have been caught during parsing.");

		var result = (JsonNode?)method.Invoke(this, arguments);

		return result;
	}
}

[Flags]
internal enum FunctionArgType
{
	Undefined,
	Null = 1,
	Boolean = 1 << 1,
	Number = 1 << 2,
	String = 1 << 3,
	Array = 1 << 4,
	Object = 1 << 5
}