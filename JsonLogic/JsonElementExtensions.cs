using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Logic
{
	/// <summary>
	/// Provides fuzzy-logic extensions for <see cref="JsonElement"/> values.
	/// </summary>
	public static class JsonElementExtensions
	{
		/// <summary>
		/// Determines whether a value can be considered as `true`.
		/// </summary>
		/// <param name="element">The element.</param>
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
		public static bool IsTruthy(this JsonElement element)
		{
			return element.ValueKind switch
			{
				JsonValueKind.Object => element.EnumerateObject().Count() != 0,
				JsonValueKind.Array => element.GetArrayLength() != 0,
				JsonValueKind.String => element.GetString() != string.Empty,
				JsonValueKind.Number => element.GetDecimal() != 0,
				JsonValueKind.True => true,
				JsonValueKind.False => false,
				JsonValueKind.Null => false,
				_ => throw new JsonLogicException($"Cannot determine truthiness of `{element.ValueKind}`.")
			};
		}

		/// <summary>
		/// Provides a loose-cast to a string.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>
		///	A string representation of the value as follows:
		///
		/// - arrays are the elements stringified and comma-delimited
		/// - null returns the empty string
		/// - objects return null (not stringifiable)
		///	- numbers and booleans return their JSON equivalents
		/// - strings are unchanged
		/// </returns>
		public static string? Stringify(this JsonElement element)
		{
			return element.ValueKind switch
			{
				JsonValueKind.Array => string.Join(",", element.EnumerateArray().Select(Stringify)),
				JsonValueKind.String => element.GetString(),
				JsonValueKind.Number => element.GetDecimal().ToString(CultureInfo.InvariantCulture),
				JsonValueKind.True => "true",
				JsonValueKind.False => "false",
				JsonValueKind.Null => string.Empty,
				_ => null
			};
		}

		/// <summary>
		/// Provides a loose-cast to a number.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns>
		///	A string representation of the value as follows:
		///
		/// - strings try to parse a number from the value
		/// - true returns 1
		/// - false returns 0
		///	- numbers are unchanged
		/// - null, objects, and arrays return null (not numberifiable)
		/// </returns>
		public static decimal? Numberify(this JsonElement element)
		{
			return element.ValueKind switch
			{
				JsonValueKind.String => decimal.TryParse(element.GetString(), out var d) ? d : (decimal?) null,
				JsonValueKind.Number => element.GetDecimal(),
				JsonValueKind.True => 1,
				JsonValueKind.False => 0,
				_ => null
			};
		}

		/// <summary>
		/// Flattens an array into its root components (removes intermediate arrays).
		/// </summary>
		/// <param name="root">The element.</param>
		/// <returns>Returns a single array with all of the intermediate arrays removed.</returns>
		public static IEnumerable<JsonElement> Flatten(this JsonElement root)
		{
			if (root.ValueKind != JsonValueKind.Array) return new[] {root};

			return root.EnumerateArray().SelectMany(Flatten);
		}

		/// <summary>
		/// Provides loose equality comparison of <see cref="JsonElement"/> values.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <returns>`true` if the values are loosely equal; `false` otherwise.</returns>
		/// <remarks>
		/// Ported from [@marvindv/jsonlogic_rs](https://github.com/marvindv/jsonlogic_rs/blob/b2ad93af575f19c6b220a6a54d599e104e72a630/src/operators/logic.rs#L33).
		/// </remarks>
		public static bool LooseEquals(this JsonElement a, JsonElement b)
		{
			static string CoerceArrayToString(JsonElement array) => string.Join(",", array.EnumerateArray().Select(e => e.ToJsonString()));

			return (a.ValueKind, b.ValueKind) switch
			{
				(JsonValueKind.Array, JsonValueKind.Array) => a.IsEquivalentTo(b),
				(JsonValueKind.Number, JsonValueKind.Number) => a.IsEquivalentTo(b),
				(JsonValueKind.Object, JsonValueKind.Object) => a.IsEquivalentTo(b),
				(JsonValueKind.String, JsonValueKind.String) => a.IsEquivalentTo(b),
				(JsonValueKind.True, JsonValueKind.True) => true,
				(JsonValueKind.False, JsonValueKind.False) => true,
				(JsonValueKind.Null, JsonValueKind.Null) => true,
				(JsonValueKind.True, JsonValueKind.False) => false,
				(JsonValueKind.False, JsonValueKind.True) => false,
				(JsonValueKind.Null, _) => false,
				(_, JsonValueKind.Null) => false,
				(JsonValueKind.Object, _) => false,
				(_, JsonValueKind.Object) => false,
				(JsonValueKind.Number, JsonValueKind.Array) => LooseEquals(a, CoerceArrayToString(b).AsJsonElement()),
				(JsonValueKind.Array, JsonValueKind.Number) => LooseEquals(b, CoerceArrayToString(a).AsJsonElement()),
				(JsonValueKind.Number, _) => a.GetDecimal() == b.Numberify(),
				(_, JsonValueKind.Number) => a.Numberify() == a.GetDecimal(),
				(JsonValueKind.Array, JsonValueKind.String) => CoerceArrayToString(a) == b.GetString(),
				(JsonValueKind.String, JsonValueKind.Array) => a.GetString() == CoerceArrayToString(b),
				_ => false
			};
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
		//        (Bool(_), _) => coerce_to_f64(a)
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
	}
}