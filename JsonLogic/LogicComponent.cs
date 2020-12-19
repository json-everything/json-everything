using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Logic.Components;

namespace Json.Logic
{
	[JsonConverter(typeof(LogicComponentConverter))]
	public abstract class LogicComponent
	{
		public abstract JsonElement Apply(JsonElement data);

		public static implicit operator LogicComponent(JsonElement value) => new LiteralComponent(value);
		public static implicit operator LogicComponent(int value) => new LiteralComponent(value);
		public static implicit operator LogicComponent(string value) => new LiteralComponent(value);
		public static implicit operator LogicComponent(bool value) => new LiteralComponent(value);
		public static implicit operator LogicComponent(long value) => new LiteralComponent(value);
		public static implicit operator LogicComponent(decimal value) => new LiteralComponent(value);
		public static implicit operator LogicComponent(float value) => new LiteralComponent(value);
		public static implicit operator LogicComponent(double value) => new LiteralComponent(value);
	}

	public class LogicComponentConverter : JsonConverter<LogicComponent>
	{
		public override LogicComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				return new LiteralComponent(JsonDocument.ParseValue(ref reader).RootElement);

			var data = JsonSerializer.Deserialize<Dictionary<string, ArgumentCollection>>(ref reader, options);
			
			if (data.Count != 1)
				throw new JsonException("Rules must contain exactly one operator key with an array of arguments.");

			var ruleInfo = data.First();
			
			var ruleType = RuleRegistry.GetRule(ruleInfo.Key);

			return (LogicComponent) Activator.CreateInstance(ruleType, ruleInfo.Value.Cast<object>().ToArray());
		}

		public override void Write(Utf8JsonWriter writer, LogicComponent value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}

	[JsonConverter(typeof(ArgumentCollectionConverter))]
	internal class ArgumentCollection : List<LogicComponent>
	{
		public ArgumentCollection(LogicComponent single)
		{
			Add(single);
		}

		public ArgumentCollection(IEnumerable<LogicComponent> components)
			: base(components)
		{
		}
	}
	
	internal class ArgumentCollectionConverter : JsonConverter<ArgumentCollection>
	{
		public override ArgumentCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
				return new ArgumentCollection(JsonSerializer.Deserialize<List<LogicComponent>>(ref reader, options));
			
			return new ArgumentCollection(JsonSerializer.Deserialize<LogicComponent>(ref reader, options));
		}

		public override void Write(Utf8JsonWriter writer, ArgumentCollection value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
