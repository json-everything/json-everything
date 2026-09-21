using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models the contact information.
/// </summary>
[JsonConverter(typeof(ContactInfoJsonConverter))]
public class ContactInfo : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets the contact name.
	/// </summary>
	public string? Name { get; set; }
	/// <summary>
	/// Gets or sets the contact URL.
	/// </summary>
	public Uri? Url { get; set; }
	/// <summary>
	/// Gets or sets the contact email.
	/// </summary>
	public string? Email { get; set; }
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

		return ExtensionData?.Resolve(keys);
	}
}

internal class ContactInfoJsonConverter : JsonConverter<ContactInfo>
{
	private const string _objectType = "contact info";

	public override ContactInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		string? name = null;
		Uri? url = null;
		string? email = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "name":
					name = reader.ReadString(propertyName, _objectType);
					break;
				case "url":
					url = reader.ReadUri(propertyName, _objectType);
					break;
				case "email":
					email = reader.ReadString(propertyName, _objectType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new ContactInfo
		{
			Name = name,
			Url = url,
			Email = email,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, ContactInfo value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.MaybeWrite("name", value.Name);
		writer.MaybeWrite("url", value.Url);
		writer.MaybeWrite("email", value.Email);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
