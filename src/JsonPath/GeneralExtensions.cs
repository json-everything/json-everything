using System;
using System.Linq;
using Json.Path.Expressions;

namespace Json.Path;

internal static class GeneralExtensions
{
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