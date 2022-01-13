using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Logic.Rules;

namespace Json.Logic
{
	/// <summary>
	/// Provides a base class for rules.
	/// </summary>
	[JsonConverter(typeof(LogicComponentConverter))]
	public abstract class Rule
	{
		/// <summary>
		/// Applies the rule to the input data.
		/// </summary>
		/// <param name="data">The input data.</param>
		/// <returns>The result of the rule.</returns>
		public abstract JsonElement Apply(JsonElement data);

		/// <summary>
		/// Casts a JSON value to a <see cref="LiteralRule"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Rule(JsonElement value) => new LiteralRule(value);
		/// <summary>
		/// Casts an `int` value to a <see cref="LiteralRule"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Rule(int value) => new LiteralRule(value);
		/// <summary>
		/// Casts a `string` value to a <see cref="LiteralRule"/>.  Can also be used to create a `null` JSON literal.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Rule(string? value) => new LiteralRule(value);
		/// <summary>
		/// Casts a `bool` value to a <see cref="LiteralRule"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Rule(bool value) => new LiteralRule(value);
		/// <summary>
		/// Casts a `long` value to a <see cref="LiteralRule"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Rule(long value) => new LiteralRule(value);
		/// <summary>
		/// Casts a `decimal` value to a <see cref="LiteralRule"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Rule(decimal value) => new LiteralRule(value);
		/// <summary>
		/// Casts a `float` value to a <see cref="LiteralRule"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Rule(float value) => new LiteralRule(value);
		/// <summary>
		/// Casts a `double` value to a <see cref="LiteralRule"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		public static implicit operator Rule(double value) => new LiteralRule(value);
	}

	/// <summary>
	/// Provides serialization for all <see cref="Rule"/> derivatives.
	/// </summary>
	public class LogicComponentConverter : JsonConverter<Rule>
	{
		/// <summary>Reads and converts the JSON to type <see cref="Rule"/>.</summary>
		/// <param name="reader">The reader.</param>
		/// <param name="typeToConvert">The type to convert.</param>
		/// <param name="options">An object that specifies serialization options to use.</param>
		/// <returns>The converted value.</returns>
		public override Rule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, ArgumentCollection>>(ref reader, options);

                if (data.Count != 1)
                    throw new JsonException("Rules must contain exactly one operator key with an array of arguments.");

                var ruleInfo = data.First();

                var ruleType = RuleRegistry.GetRule(ruleInfo.Key);
                if (ruleType == null)
                    throw new JsonException($"Cannot identify rule for {ruleInfo.Key}");

                var value = ruleInfo.Value ?? new ArgumentCollection((Rule?) null);

                return (Rule) Activator.CreateInstance(ruleType,
                    value.Cast<object>()
                        .Select(o => o ?? new LiteralRule(null))
                        .ToArray());
            }

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var data = JsonSerializer.Deserialize<List<Rule>>(ref reader, options);
                return new RuleCollection(data);
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            return new LiteralRule(doc.RootElement.Clone());
        }

		/// <summary>Writes a specified value as JSON.</summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="value">The value to convert to JSON.</param>
		/// <param name="options">An object that specifies serialization options to use.</param>
		public override void Write(Utf8JsonWriter writer, Rule value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}

	[JsonConverter(typeof(ArgumentCollectionConverter))]
	internal class ArgumentCollection : List<Rule>
	{
		public ArgumentCollection(Rule? single)
		{
			Add(single!);
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
