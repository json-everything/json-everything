using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.More
{
	public class EnumStringConverter<T> : JsonConverter<T>
		where T : Enum
	{
		private static Dictionary<string, T> _readValues;
		private static Dictionary<T, string> _writeValues;
		private static readonly object _lock = new object();

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			EnsureMap();

			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();

			return _readValues.TryGetValue(str, out var value)
				? value
				: throw new JsonException($"Could not find appropriate value for {str} in type {typeToConvert.Name}");
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			EnsureMap();

			writer.WriteStringValue(_writeValues[value]);
		}

		private static void EnsureMap()
		{
			if (_readValues != null) return;
			lock (_lock)
			{
				if (_readValues != null) return;

				var map = typeof(T).GetFields()
					.Where(f => !f.IsSpecialName)
					.Select(f => new
					{
						Value = (T) Enum.Parse(typeof(T), f.Name),
						Description = f.GetCustomAttribute<DescriptionAttribute>()?.Description ?? f.Name
					})
					.ToList();
				_readValues = map.ToDictionary(v => v.Description, v => v.Value);
				_writeValues = map.ToDictionary(v => v.Value, v => v.Description);
			}
		}
	}
}