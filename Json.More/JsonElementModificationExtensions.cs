using System.Linq;
using System.Text.Json;

namespace Json.More
{
	public static class JsonElementModificationExtensions
	{
		public static JsonElement AddOrReplaceKeyInObject(this JsonElement obj, string key, JsonElement value)
		{
			if (obj.ValueKind != JsonValueKind.Object)
				throw new JsonException("This extension method is only valid for objects.");

			var dict = obj.EnumerateObject().ToDictionary(kvp => kvp.Name, kvp => kvp.Value);
			dict[key] = value.Clone();

			return dict.AsJsonElement();
		}

		public static JsonElement RemoveKeyFromObject(this JsonElement obj, string key)
		{
			if (obj.ValueKind != JsonValueKind.Object)
				throw new JsonException("This extension method is only valid for objects.");

			var dict = obj.EnumerateObject().ToDictionary(kvp => kvp.Name, kvp => kvp.Value);
			dict.Remove(key);

			return dict.AsJsonElement();
		}

		public static JsonElement InsertElementInArray(this JsonElement array, int index, JsonElement value)
		{
			if (array.ValueKind != JsonValueKind.Array)
				throw new JsonException("This extension method is only valid for arrays.");

			var arr = array.EnumerateArray().ToList();
			arr.Insert(index, value.Clone());

			return arr.AsJsonElement();
		}

		public static JsonElement RemoveElementFromArray(this JsonElement array, int index)
		{
			if (array.ValueKind != JsonValueKind.Array)
				throw new JsonException("This extension method is only valid for arrays.");

			var arr = array.EnumerateArray().ToList();
			arr.RemoveAt(index);

			return arr.AsJsonElement();
		}
	}
}