using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.Logic;

/// <summary>
/// Provides fuzzy-logic extensions for <see cref="JsonNode"/> values.
/// </summary>
public static class JsonNodeExtensions
{
	/// <summary>
	/// Determines whether a value can be considered as `true`.
	/// </summary>
	/// <param name="node">The element.</param>
	/// <returns>
	/// `true` if the value is:
	///
	/// - a non-empty array
	/// - a non-empty string
	/// - a non-zero number
	/// - true
	///
	///	`false` otherwise
	///
	/// </returns>
	public static bool IsTruthy(this JsonNode? node)
	{
		switch (node)
		{
			case null:
				return false;
			case JsonObject obj:
				return obj.Count != 0;
			case JsonArray arr:
				return arr.Count != 0;
			case JsonValue value:
				if (value.TryGetValue(out string? s)) return s != string.Empty;
				if (value.TryGetValue(out bool b)) return b;
				var n = value.GetNumber();
				return n != null && n != 0;
			default:
				throw new JsonLogicException($"Cannot determine truthiness of `{node}`.");
		}
	}

	/// <summary>
	/// Provides a loose-cast to a string.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <returns>
	///	A string representation of the value as follows:
	///
	/// - arrays are the elements stringified and comma-delimited
	/// - null returns the empty string
	/// - objects return null (not stringifiable)
	///	- numbers and booleans return their JSON equivalents
	/// - strings are unchanged
	/// </returns>
	public static string? Stringify(this JsonNode? node)
	{
		switch (node)
		{
			case null:
				return string.Empty;
			case JsonArray arr:
				return string.Join(",", arr.Select(Stringify));
			case JsonValue value:
				if (value.TryGetValue(out string? s)) return s;
				return node.ToJsonString();
			default:
				return null;
		}
	}

	/// <summary>
	/// Provides a loose-cast to a number.
	/// </summary>
	/// <param name="node">The node.</param>
	/// <returns>
	///	A string representation of the value as follows:
	///
	/// - strings try to parse a number from the value
	/// - true returns 1
	/// - false returns 0
	///	- numbers are unchanged
	/// - null, objects, and arrays return null (not numberifiable)
	/// </returns>
	public static decimal? Numberify(this JsonNode? node)
	{
		if (node is null) return 0;

		if (node is not JsonValue value) return null;

		if (value.TryGetValue(out string? s)) return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var d) ? d : null;

		if (value.TryGetValue(out bool b)) return b ? 1 : 0;

		return value.GetNumber();

	}

	/// <summary>
	/// Flattens an array into its root components (removes intermediate arrays).
	/// </summary>
	/// <param name="root">The element.</param>
	/// <returns>Returns a single array with all of the intermediate arrays removed.</returns>
	public static IEnumerable<JsonNode?> Flatten(this JsonNode? root)
	{
		if (root is not JsonArray array) return new[] { root };

		return array.SelectMany(Flatten);
	}

	/// <summary>
	/// Provides loose equality comparison of <see cref="JsonNode"/> values.
	/// </summary>
	/// <param name="a">The first value.</param>
	/// <param name="b">The second value.</param>
	/// <returns>`true` if the values are loosely equal; `false` otherwise.</returns>
	/// <remarks>
	/// Adapted from [@marvindv/jsonlogic_rs](https://github.com/marvindv/jsonlogic_rs/blob/b2ad93af575f19c6b220a6a54d599e104e72a630/src/operators/logic.rs#L33).
	/// </remarks>
	public static bool LooseEquals(this JsonNode? a, JsonNode? b)
	{
		static string CoerceArrayToString(JsonArray array) => string.Join(",", array.Select(e => e.AsJsonString()));

		if (a is null && b is null) return true;
		if (a is null || b is null) return false;

		if (a is JsonObject && b is JsonObject) return a.IsEquivalentTo(b);
		if (a is JsonObject || b is JsonObject) return false;

		if (a is JsonArray && b is JsonArray) return a.IsEquivalentTo(b);
		if (a is JsonArray aArr)
		{
			var aStr = CoerceArrayToString(aArr);
			var bVal = (JsonValue)b;
			if (bVal.TryGetValue(out string? bStr)) return aStr == bStr;
			var bNum = bVal.GetNumber();
			if (!bNum.HasValue) return LooseEquals(b, aStr);
			return false;
		}
		if (b is JsonArray bArr)
		{
			var bStr = CoerceArrayToString(bArr);
			var aVal = (JsonValue)a;
			if (aVal.TryGetValue(out string? aStr)) return bStr == aStr;
			var aNum = aVal.GetNumber();
			if (!aNum.HasValue) return LooseEquals(b, bStr);
			return false;
		}

		var aNumber = ((JsonValue)a).GetNumber();
		var bNumber = ((JsonValue)b).GetNumber();
		if (aNumber.HasValue && !bNumber.HasValue) return aNumber == b.Numberify();
		if (!aNumber.HasValue && bNumber.HasValue) return a.Numberify() == bNumber;

		return a.IsEquivalentTo(b);
	}

	//fn is_abstract_equal(a: &Value, b: &Value) -> bool {
	//    use Value::*;

	//    match (a, b) {
	//        // 1. Strict equal for same types.
	//        (Array(_), Array(_))
	//        | (Bool(_), Bool(_))
	//        | (Null, Null)
	//        | (Number(_), Number(_))
	//        | (Object(_), Object(_))
	//        | (String(_), String(_)) => is_strict_equal(a, b),
	//        // short-circuit only one operand being null
	//        (Null, _) | (_, Null) => false,
	//        // 4. If Type(a) is number and Type(b) is string, return a == ToNumber(b).
	//        (Number(a), String(_)) => coerce_to_f64(b)
	//            .map(|b| a.as_f64().unwrap() == b)
	//            .unwrap_or(false),
	//        // 5. If Type(a) is string and Type(b) is number, return ToNumber(a) == b.
	//        (String(_), Number(b)) => coerce_to_f64(a)
	//            .map(|a| a == b.as_f64().unwrap())
	//            .unwrap_or(false),
	//        // 6. If Type(a) is bool return ToNumber(a)==b
	//        (Bool(_), _, _) => coerce_to_f64(a)
	//            .map(|a| is_abstract_equal(&Value::Number(serde_json::Number::from_f64(a).unwrap()), b))
	//            .unwrap_or(false),
	//        // 7. If Type(b) is bool return a==ToNumber(b)
	//        (_, Bool(_)) => coerce_to_f64(b)
	//            .map(|b| is_abstract_equal(a, &Value::Number(serde_json::Number::from_f64(b).unwrap())))
	//            .unwrap_or(false),
	//        // 8. something with object
	//        // if non array object:
	//        //   An object is never equal to something else, including another object, since
	//        //   ToPrimitive(object) does not work for json.
	//        (Object(_), _) | (_, Object(_)) => false,
	//        // if array:
	//        //   the only possible operand types that are still possible are Number and String
	//        (String(a), Array(b)) | (Array(b), String(a)) => a == &arr_to_primitive_str(b),
	//        (Number(_), Array(b)) => is_abstract_equal(a, &Value::String(arr_to_primitive_str(b))),
	//        (Array(a), Number(_)) => is_abstract_equal(&Value::String(arr_to_primitive_str(a)), b),
	//    }
	//}

	//fn arr_to_primitive_str(arr: &[Value]) -> String {
	//	arr.iter()
	//			.map(|el| coerce_to_str(el))
	//		.collect::<Vec<String>>()
	//		.join(",")
	//}

	internal static string JsonType(this JsonNode? node)
	{
		return node switch
		{
			null => "null",
			JsonObject => "object",
			JsonArray => "array",
			JsonValue value => value.TryGetValue(out string? _)
				? "string"
				: value.TryGetValue(out bool _)
					? "boolean"
					: value.GetNumber() != null
						? "number"
						: throw new JsonLogicException("Unknown node type"),
			_ => throw new JsonLogicException("Unknown node type")
		};
	}
}