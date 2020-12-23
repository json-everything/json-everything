using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Logic.Rules;

namespace Json.Logic
{
	[JsonConverter(typeof(LogicComponentConverter))]
	public abstract class Rule
	{
		public abstract JsonElement Apply(JsonElement data);

		public static implicit operator Rule(JsonElement value) => new LiteralRule(value);
		public static implicit operator Rule(int value) => new LiteralRule(value);
		public static implicit operator Rule(string value) => new LiteralRule(value);
		public static implicit operator Rule(bool value) => new LiteralRule(value);
		public static implicit operator Rule(long value) => new LiteralRule(value);
		public static implicit operator Rule(decimal value) => new LiteralRule(value);
		public static implicit operator Rule(float value) => new LiteralRule(value);
		public static implicit operator Rule(double value) => new LiteralRule(value);
	}

	public class LogicComponentConverter : JsonConverter<Rule>
	{
		public override Rule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				return new LiteralRule(JsonDocument.ParseValue(ref reader).RootElement);

			var data = JsonSerializer.Deserialize<Dictionary<string, ArgumentCollection>>(ref reader, options);
			
			if (data.Count != 1)
				throw new JsonException("Rules must contain exactly one operator key with an array of arguments.");

			var ruleInfo = data.First();
			
			var ruleType = RuleRegistry.GetRule(ruleInfo.Key);

			var value = ruleInfo.Value ?? new ArgumentCollection((Rule) null);

			return (Rule) Activator.CreateInstance(ruleType,
				value.Cast<object>()
					.Select(o => o ?? new LiteralRule(null))
					.ToArray());
		}

		public override void Write(Utf8JsonWriter writer, Rule value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}

	[JsonConverter(typeof(ArgumentCollectionConverter))]
	internal class ArgumentCollection : List<Rule>
	{
		public ArgumentCollection(Rule single)
		{
			Add(single);
		}

		public ArgumentCollection(IEnumerable<Rule> components)
			: base(components)
		{
		}
	}
	
	internal class ArgumentCollectionConverter : JsonConverter<ArgumentCollection>
	{
		public override ArgumentCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
				return new ArgumentCollection(JsonSerializer.Deserialize<List<Rule>>(ref reader, options));
			
			return new ArgumentCollection(JsonSerializer.Deserialize<Rule>(ref reader, options));
		}

		public override void Write(Utf8JsonWriter writer, ArgumentCollection value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
