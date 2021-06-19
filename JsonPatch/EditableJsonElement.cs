using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Patch
{
	[JsonConverter(typeof(EditableJsonElementJsonConverter))]
	internal class EditableJsonElement
	{
		private readonly JsonElement _raw;
		
		public Dictionary<string, EditableJsonElement>? Object { get; }
		public List<EditableJsonElement>? Array { get; }

		public EditableJsonElement(JsonElement raw)
		{
			switch (raw.ValueKind)
			{
				case JsonValueKind.Object:
					Object = raw.EnumerateObject()
						.ToDictionary(kvp => kvp.Name, kvp => new EditableJsonElement(kvp.Value));
					break;
				case JsonValueKind.Array:
					Array = raw.EnumerateArray()
						.Select(v => new EditableJsonElement(v))
						.ToList();
					break;
				case JsonValueKind.Undefined:
				case JsonValueKind.String:
				case JsonValueKind.Number:
				case JsonValueKind.True:
				case JsonValueKind.False:
				case JsonValueKind.Null:
					_raw = raw.Clone();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public JsonElement ToElement()
		{
			using var doc = JsonDocument.Parse(ToString());
			return doc.RootElement.Clone();
		}

		public override string ToString()
		{
			if (Object != null)
				return $"{{{string.Join(",", Object.Select(p => $"{JsonSerializer.Serialize(p.Key)}:{p.Value}"))}}}";

			if (Array != null)
				return $"[{string.Join(",", Array)}]";

			return _raw.ValueKind switch
			{
				JsonValueKind.String => _raw.GetRawText(),
				JsonValueKind.Number => _raw.ToString(),
				JsonValueKind.True => "true",
				JsonValueKind.False => "false",
				JsonValueKind.Null => "null",
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}

	internal class EditableJsonElementJsonConverter : JsonConverter<EditableJsonElement>
	{
		public override EditableJsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

			return new EditableJsonElement(element);
		}

		public override void Write(Utf8JsonWriter writer, EditableJsonElement value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}