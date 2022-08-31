using System.Collections.Generic;
using System.Text.Json;

namespace Json.Logic;

/// <summary>
/// Provides extended functionality for serializing rules.
/// </summary>
public static class JsonWriterExtensions
{
	/// <summary>
	/// Writes a rule to the stream, taking its specific type into account.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="rule">The rule.</param>
	/// <param name="options">Serializer options.</param>
	public static void WriteRule(this Utf8JsonWriter writer, Rule rule, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, rule, rule.GetType(), options);
	}
	/// <summary>
	/// Writes a rule to the stream, taking its specific type into account.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="rules">The rules.</param>
	/// <param name="options">Serializer options.</param>
	public static void WriteRules(this Utf8JsonWriter writer, IEnumerable<Rule> rules, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var rule in rules)
		{
			writer.WriteRule(rule, options);
		}
		writer.WriteEndArray();
	}
}