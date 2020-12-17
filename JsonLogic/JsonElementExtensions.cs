using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Json.Logic
{
	internal static class JsonElementExtensions
	{
		public static bool IsTruthy(this JsonElement element)
		{
			return element.ValueKind switch
			{
				JsonValueKind.Array => element.GetArrayLength() != 0,
				JsonValueKind.String => element.GetString() != string.Empty,
				JsonValueKind.Number => element.GetDecimal() != 0,
				JsonValueKind.True => true,
				JsonValueKind.False => false,
				JsonValueKind.Null => false,
				_ => throw new JsonLogicException($"Cannot determine truthiness of `{element.ValueKind}`.")
			};
		}

		public static string Stringify(this JsonElement element)
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
	}
}