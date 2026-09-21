using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a security requirement.
/// </summary>
[JsonConverter(typeof(SecurityRequirementJsonConverter))]
public class SecurityRequirement : Dictionary<string, IEnumerable<string>>;

internal class SecurityRequirementJsonConverter : JsonConverter<SecurityRequirement>
{
	private const string _objectType = "security requirement";

	public override SecurityRequirement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		var requirement = new SecurityRequirement();

		while (reader.ReadPropertyName() is { } propertyName)
		{
			requirement.Add(propertyName, reader.ReadStringArray(propertyName, _objectType, options));
		}

		return requirement;
	}

	public override void Write(Utf8JsonWriter writer, SecurityRequirement value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		foreach (var kvp in value)
		{
			writer.WritePropertyName(kvp.Key);
			options.WriteList(writer, kvp.Value, ApiSerializerContext.Default.String);
		}

		writer.WriteEndObject();
	}
}