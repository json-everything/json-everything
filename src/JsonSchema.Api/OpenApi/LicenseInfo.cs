using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models the license information.
/// </summary>
[JsonConverter(typeof(LicenseInfoJsonConverter))]
public class LicenseInfo : IRefTargetContainer
{
	/// <summary>
	/// Gets license name used for the API.
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// Gets or sets an SPDX license expression for the API.
	/// </summary>
	public string? Identifier { get; set; }
	/// <summary>
	/// Gets or sets URL to the license used for the API.
	/// </summary>
	public Uri? Url { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="LicenseInfo"/>
	/// </summary>
	/// <param name="name">The license name used for the API.</param>
	public LicenseInfo(string name)
	{
		Name = name;
	}

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		return ExtensionData?.Resolve(keys);
	}
}

internal class LicenseInfoJsonConverter : JsonConverter<LicenseInfo>
{
	private const string _objectType = "license info";

	public override LicenseInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		string? name = null;
		string? identifier = null;
		Uri? url = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "name":
					name = reader.ReadString(propertyName, _objectType);
					break;
				case "identifier":
					identifier = reader.ReadString(propertyName, _objectType);
					break;
				case "url":
					url = reader.ReadUri(propertyName, _objectType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new LicenseInfo(ConverterHelpers.ExpectPresent(name, "name", _objectType))
		{
			Identifier = identifier,
			Url = url,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, LicenseInfo value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("name", value.Name);
		writer.MaybeWrite("identifier", value.Identifier);
		writer.MaybeWrite("url", value.Url);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
