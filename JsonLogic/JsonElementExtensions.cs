using System;
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
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}