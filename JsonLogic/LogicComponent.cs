using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Logic
{
	[JsonConverter(typeof(LogicComponentConverter))]
	public abstract class LogicComponent
	{
		public abstract JsonElement Apply(JsonElement data);
	}

	public class LogicComponentConverter : JsonConverter<LogicComponent>
	{
		public override LogicComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, LogicComponent value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
