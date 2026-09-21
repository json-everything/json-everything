using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a server variable.
/// </summary>
[JsonConverter(typeof(ServerVariableJsonConverter))]
public class ServerVariable : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets an enumeration of string values to be used if the substitution options are from a limited set.
	/// </summary>
	public IReadOnlyList<string>? Enum { get; set; }
	/// <summary>
	/// Gets the default value to use for substitution.
	/// </summary>
	public string Default { get; }
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="ServerVariable"/>
	/// </summary>
	/// <param name="default">The default value</param>
	public ServerVariable(string @default)
	{
		Default = @default;
	}

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		return ExtensionData?.Resolve(keys);
	}
}

internal class ServerVariableJsonConverter : JsonConverter<ServerVariable>
{
	private const string _objectType = "server variable";

	public override ServerVariable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		List<string>? enumValues = null;
		string? defaultValue = null;
		string? description = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "enum":
					enumValues = reader.ReadStringArray(propertyName, _objectType, options);
					break;
				case "default":
					defaultValue = reader.ReadString(propertyName, _objectType);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new ServerVariable(ConverterHelpers.ExpectPresent(defaultValue, "default", _objectType))
		{
			Enum = enumValues,
			Description = description,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, ServerVariable value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("default", value.Default);
		writer.MaybeWriteStringArray("enum", value.Enum, options);
		writer.MaybeWrite("description", value.Description);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}