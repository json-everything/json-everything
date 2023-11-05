using System;
using System.Text.Json.Nodes;

namespace Json.JsonE.Functions;

internal abstract class FunctionDefinition
{
	public abstract string Name { get; }

	public abstract FunctionValueType[] ParameterTypes { get; }

	public virtual bool AcceptsParamsList => false;

	public abstract FunctionValueType ReturnType { get; }

	internal abstract JsonNode? Invoke(JsonNode?[] arguments, EvaluationContext context);
}

[Flags]
internal enum FunctionValueType
{
	Undefined,
	Null = 1,
	Boolean = 1 << 1,
	Number = 1 << 2,
	String = 1 << 3,
	Array = 1 << 4,
	Object = 1 << 5,
	Any = 1 << 6 - 1
}