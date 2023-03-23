using System;
using System.Collections.Generic;
using System.Linq;
using Json.Path.Expressions;

namespace Json.Path;

internal static class GeneralExtensions
{
	public static IEnumerable<FunctionType> ToArgumentTypes(this object?[] arguments)
	{
		return arguments.Select(x =>
		{
			if (x is JsonPathValue) return FunctionType.Value;
			if (x is LogicalPathValue) return FunctionType.Logical;
			if (x is NodeListPathValue) return FunctionType.Nodelist;
			return FunctionType.Unspecified;
		});
	}

	public static object?[] ExtractArgumentValues(this object?[] arguments, FunctionType[] parameterTypes)
	{
		return arguments
			.Zip(parameterTypes, (a, p) => (a,p))
			.Select(x =>
		{
			return (object?)(x.a switch
			{
				JsonPathValue j => j.Value,
				LogicalPathValue l => l.Value,
				NodeListPathValue n => x.p switch
				{
					 FunctionType.Value => n.Value.Count == 1 ? n.Value[0].Value : null,
					 FunctionType.Logical => n.Value.Count == 0,
					 FunctionType.Nodelist => n.Value,
					 _ => throw new ArgumentOutOfRangeException()
				},
				_ => null
			});
		}).ToArray();
	}
}