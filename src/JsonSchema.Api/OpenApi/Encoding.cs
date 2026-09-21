using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models an encoding object.
/// </summary>
[JsonConverter(typeof(EncodingJsonConverter))]
public class Encoding : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets the encoding content type.
	/// </summary>
	public string? ContentType { get; set; }
	/// <summary>
	/// Gets or sets headers.
	/// </summary>
	public Dictionary<string, Header>? Headers { get; set; }
	/// <summary>
	/// Gets or sets the encoding parameter style.
	/// </summary>
	public ParameterStyle? Style { get; set; }
	/// <summary>
	/// Gets or sets whether this will be exploded into multiple parameters.
	/// </summary>
	public bool? Explode { get; set; }
	/// <summary>
	/// Gets or sets whether the parameter value SHOULD allow reserved characters.
	/// </summary>
	public bool? AllowReserved { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		if (keys[0] == "headers")
		{
			if (keys.Length == 1) return null;
			return Headers.GetFromMap(keys[1])?.Resolve(keys.Slice(2));
		}

		return ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return Headers?.Values.SelectMany(x => x.FindSchemas()) ?? Enumerable.Empty<JsonSchema>();
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		return GeneralHelpers.Collect(
			Headers?.Values.SelectMany(x => x.FindRefs())
		);
	}
}

internal class EncodingJsonConverter : JsonConverter<Encoding>
{
	private const string _objectType = "encoding";

	public override Encoding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		string? contentType = null;
		Dictionary<string, Header>? headers = null;
		ParameterStyle? style = null;
		bool? explode = null;
		bool? allowReserved = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "contentType":
					contentType = reader.ReadString(propertyName, _objectType);
					break;
				case "headers":
					headers = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Header);
					break;
				case "style":
					style = reader.ReadEnum(propertyName, _objectType, options, ApiSerializerContext.Default.ParameterStyle);
					break;
				case "explode":
					explode = reader.ReadBool(propertyName, _objectType);
					break;
				case "allowReserved":
					allowReserved = reader.ReadBool(propertyName, _objectType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new Encoding
		{
			ContentType = contentType,
			Headers = headers,
			Style = style,
			Explode = explode,
			AllowReserved = allowReserved,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, Encoding value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.MaybeWrite("contentType", value.ContentType);
		writer.MaybeWriteMap("headers", value.Headers, options, ApiSerializerContext.Default.Header);
		writer.MaybeWriteEnum("style", value.Style, options, ApiSerializerContext.Default.ParameterStyle);
		writer.MaybeWrite("explode", value.Explode);
		writer.MaybeWrite("allowReserved", value.AllowReserved);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
