using System;
using System.Reflection;
using System.Text.Json;

namespace Json.More;

internal static class ValueReader
{
	public static ValueTuple<T> ReadValues1<T>(JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
	{
		var value1 = JsonElementExtensions.ReadValue<T>(ref enumerator, options);

		return new ValueTuple<T>(value1);
	}

	public static (T1, T2) ReadValues2<T1, T2>(JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
	{
		var value1 = JsonElementExtensions.ReadValue<T1>(ref enumerator, options);
		var value2 = JsonElementExtensions.ReadValue<T2>(ref enumerator, options);

		return (value1, value2);
	}

	public static (T1, T2, T3) ReadValues3<T1, T2, T3>(JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
	{
		var value1 = JsonElementExtensions.ReadValue<T1>(ref enumerator, options);
		var value2 = JsonElementExtensions.ReadValue<T2>(ref enumerator, options);
		var value3 = JsonElementExtensions.ReadValue<T3>(ref enumerator, options);

		return (value1, value2, value3);
	}

	public static (T1, T2, T3, T4) ReadValues4<T1, T2, T3, T4>(JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
	{
		var value1 = JsonElementExtensions.ReadValue<T1>(ref enumerator, options);
		var value2 = JsonElementExtensions.ReadValue<T2>(ref enumerator, options);
		var value3 = JsonElementExtensions.ReadValue<T3>(ref enumerator, options);
		var value4 = JsonElementExtensions.ReadValue<T4>(ref enumerator, options);

		return (value1, value2, value3, value4);
	}

	public static (T1, T2, T3, T4, T5) ReadValues5<T1, T2, T3, T4, T5>(JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
	{
		var value1 = JsonElementExtensions.ReadValue<T1>(ref enumerator, options);
		var value2 = JsonElementExtensions.ReadValue<T2>(ref enumerator, options);
		var value3 = JsonElementExtensions.ReadValue<T3>(ref enumerator, options);
		var value4 = JsonElementExtensions.ReadValue<T4>(ref enumerator, options);
		var value5 = JsonElementExtensions.ReadValue<T5>(ref enumerator, options);

		return (value1, value2, value3, value4, value5);
	}

	public static (T1, T2, T3, T4, T5, T6) ReadValues6<T1, T2, T3, T4, T5, T6>(JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
	{
		var value1 = JsonElementExtensions.ReadValue<T1>(ref enumerator, options);
		var value2 = JsonElementExtensions.ReadValue<T2>(ref enumerator, options);
		var value3 = JsonElementExtensions.ReadValue<T3>(ref enumerator, options);
		var value4 = JsonElementExtensions.ReadValue<T4>(ref enumerator, options);
		var value5 = JsonElementExtensions.ReadValue<T5>(ref enumerator, options);
		var value6 = JsonElementExtensions.ReadValue<T6>(ref enumerator, options);

		return (value1, value2, value3, value4, value5, value6);
	}

	public static (T1, T2, T3, T4, T5, T6, T7) ReadValues7<T1, T2, T3, T4, T5, T6, T7>(JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
	{
		var value1 = JsonElementExtensions.ReadValue<T1>(ref enumerator, options);
		var value2 = JsonElementExtensions.ReadValue<T2>(ref enumerator, options);
		var value3 = JsonElementExtensions.ReadValue<T3>(ref enumerator, options);
		var value4 = JsonElementExtensions.ReadValue<T4>(ref enumerator, options);
		var value5 = JsonElementExtensions.ReadValue<T5>(ref enumerator, options);
		var value6 = JsonElementExtensions.ReadValue<T6>(ref enumerator, options);
		var value7 = JsonElementExtensions.ReadValue<T7>(ref enumerator, options);

		return (value1, value2, value3, value4, value5, value6, value7);
	}

	public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> ReadValues8<T1, T2, T3, T4, T5, T6, T7, TRest>(JsonElement.ArrayEnumerator enumerator, JsonSerializerOptions options)
		where TRest : struct
	{
		var value1 = JsonElementExtensions.ReadValue<T1>(ref enumerator, options);
		var value2 = JsonElementExtensions.ReadValue<T2>(ref enumerator, options);
		var value3 = JsonElementExtensions.ReadValue<T3>(ref enumerator, options);
		var value4 = JsonElementExtensions.ReadValue<T4>(ref enumerator, options);
		var value5 = JsonElementExtensions.ReadValue<T5>(ref enumerator, options);
		var value6 = JsonElementExtensions.ReadValue<T6>(ref enumerator, options);
		var value7 = JsonElementExtensions.ReadValue<T7>(ref enumerator, options);

		var restParams = typeof(TRest).GetGenericArguments();
		var method = GetReadValuesMethod($"ReadValues{restParams.Length}", restParams);

		var rest = (TRest) method.Invoke(null, new object[] { enumerator, options })!;

		return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(value1, value2, value3, value4, value5, value6, value7, rest);
	}

	private static MethodInfo GetReadValuesMethod(string methodName, Type[] types)
	{
		var type = typeof(ValueReader);
		var generic = type.GetMethod(methodName);
		return generic!.MakeGenericMethod(types);
	}
}