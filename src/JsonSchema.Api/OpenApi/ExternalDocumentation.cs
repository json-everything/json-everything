using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models external documentation.
/// </summary>
[JsonConverter(typeof(ExternalDocumentationJsonConverter))]
public class ExternalDocumentation : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets the URL for the target documentation.
	/// </summary>
	public Uri Url { get; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="ExternalDocumentation"/>
	/// </summary>
	/// <param name="url">The URL for the target documentation.</param>
	public ExternalDocumentation(Uri url)
	{
		Url = url;
	}

	/// <summary>
	/// Creates a new <see cref="ExternalDocumentation"/>
	/// </summary>
	/// <param name="url">The URL for the target documentation.</param>
	public ExternalDocumentation(string url)
	{
		Url = new Uri(url, UriKind.RelativeOrAbsolute);
	}

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		return ExtensionData?.Resolve(keys);
	}
}

internal class ExternalDocumentationJsonConverter : JsonConverter<ExternalDocumentation>
{
	private const string _objectType = "external documentation";

	public override ExternalDocumentation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? url = null;
		string? description = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "url":
					url = reader.ReadUri(propertyName, _objectType);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new ExternalDocumentation(ConverterHelpers.ExpectPresent(url, "url", _objectType))
		{
			Description = description,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, ExternalDocumentation value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("url", value.Url.ToString());
		writer.MaybeWrite("description", value.Description);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
