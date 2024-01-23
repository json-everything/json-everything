using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace Json.More;

internal static class ValueWriter
{
	public static void WriteValues(Utf8JsonWriter writer, IEnumerable<(object? value, Type type)> values, JsonSerializerOptions options)
	{
		foreach (var item in values)
		{
			JsonSerializer.Serialize(writer, item.value, item.type, options);
		}
	}

	public static IEnumerable<(object?, Type)> Unwrap1<T>(ValueTuple<T> value)
	{
		yield return (value.Item1, typeof(T));
	}

	public static IEnumerable<(object?, Type)> Unwrap2<T1, T2>(ValueTuple<T1, T2> value)
	{
		yield return (value.Item1, typeof(T1));
		yield return (value.Item2, typeof(T2));
	}

	public static IEnumerable<(object?, Type)> Unwrap3<T1, T2, T3>(ValueTuple<T1, T2, T3> value)
	{
		yield return (value.Item1, typeof(T1));
		yield return (value.Item2, typeof(T2));
		yield return (value.Item3, typeof(T3));
	}

	public static IEnumerable<(object?, Type)> Unwrap4<T1, T2, T3, T4>(ValueTuple<T1, T2, T3, T4> value)
	{
		yield return (value.Item1, typeof(T1));
		yield return (value.Item2, typeof(T2));
		yield return (value.Item3, typeof(T3));
		yield return (value.Item4, typeof(T4));
	}

	public static IEnumerable<(object?, Type)> Unwrap5<T1, T2, T3, T4, T5>(ValueTuple<T1, T2, T3, T4, T5> value)
	{
		yield return (value.Item1, typeof(T1));
		yield return (value.Item2, typeof(T2));
		yield return (value.Item3, typeof(T3));
		yield return (value.Item4, typeof(T4));
		yield return (value.Item5, typeof(T5));
	}

	public static IEnumerable<(object?, Type)> Unwrap6<T1, T2, T3, T4, T5, T6>(ValueTuple<T1, T2, T3, T4, T5, T6> value)
	{
		yield return (value.Item1, typeof(T1));
		yield return (value.Item2, typeof(T2));
		yield return (value.Item3, typeof(T3));
		yield return (value.Item4, typeof(T4));
		yield return (value.Item5, typeof(T5));
		yield return (value.Item6, typeof(T6));
	}

	public static IEnumerable<(object?, Type)> Unwrap7<T1, T2, T3, T4, T5, T6, T7>(ValueTuple<T1, T2, T3, T4, T5, T6, T7> value)
	{
		yield return (value.Item1, typeof(T1));
		yield return (value.Item2, typeof(T2));
		yield return (value.Item3, typeof(T3));
		yield return (value.Item4, typeof(T4));
		yield return (value.Item5, typeof(T5));
		yield return (value.Item6, typeof(T6));
		yield return (value.Item7, typeof(T7));
	}

	public static IEnumerable<(object?, Type)> Unwrap8<T1, T2, T3, T4, T5, T6, T7, TRest>(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value)
		where TRest : struct
	{
		yield return (value.Item1, typeof(T1));
		yield return (value.Item2, typeof(T2));
		yield return (value.Item3, typeof(T3));
		yield return (value.Item4, typeof(T4));
		yield return (value.Item5, typeof(T5));
		yield return (value.Item6, typeof(T6));
		yield return (value.Item7, typeof(T7));

		var typeParams = typeof(TRest).GetGenericArguments();
		var method = GetUnwrapMethod($"Unwrap{typeParams.Length}", typeParams);

		var unwrapped = (IEnumerable<(object?, Type)>) method.Invoke(null, new object[] { value.Rest });
		foreach (var item in unwrapped)
		{
			yield return item;
		}
	}

	private static MethodInfo GetUnwrapMethod(string methodName, Type[] types)
	{
		var type = typeof(ValueWriter);
		var generic = type.GetMethod(methodName);
		return generic!.MakeGenericMethod(types);
	}
}