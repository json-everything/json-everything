using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a server.
/// </summary>
[JsonConverter(typeof(ServerJsonConverter))]
public class Server : IRefTargetContainer
{
	/// <summary>
	/// Gets the URL of the server.
	/// </summary>
	public string Url { get; } // may include variables
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets the variable map.
	/// </summary>
	public Dictionary<string, ServerVariable>? Variables { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="Server"/>
	/// </summary>
	/// <param name="url">The server URL</param>
	public Server(string url)
	{
		Url = url;
	}

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		if (keys[0] == "variables")
		{
			if (keys.Length == 1) return null;
			return Variables.GetFromMap(keys[1])?.Resolve(keys.Slice(2));
		}

		return ExtensionData?.Resolve(keys);
	}
}

internal class ServerJsonConverter : JsonConverter<Server>
{
	private const string _objectType = "server";

	public override Server Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		string? url = null;
		string? description = null;
		Dictionary<string, ServerVariable>? variables = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "url":
					url = reader.ReadString(propertyName, _objectType);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "variables":
					variables = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.ServerVariable);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new Server(ConverterHelpers.ExpectPresent(url, "url", _objectType))
		{
			Description = description,
			Variables = variables,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, Server value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("url", value.Url);
		writer.MaybeWrite("description", value.Description);
		writer.MaybeWriteMap("variables", value.Variables, options, ApiSerializerContext.Default.ServerVariable);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}