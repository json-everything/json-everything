using System;
using Json.More;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models the info object.
/// </summary>
[JsonConverter(typeof(OpenApiInfoJsonConverter))]
public class OpenApiInfo : IRefTargetContainer
{
	/// <summary>
	/// Gets the title.
	/// </summary>
	public string Title { get; set; }
	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public string? Summary { get; set; }
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets the link to the terms of service.
	/// </summary>
	public Uri? TermsOfService { get; set; }
	/// <summary>
	/// Gets or sets the contact information.
	/// </summary>
	public ContactInfo? Contact { get; set; }
	/// <summary>
	/// Gets or sets the license information.
	/// </summary>
	public LicenseInfo? License { get; set; }
	/// <summary>
	/// Gets or sets the API version.
	/// </summary>
	public string Version { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="OpenApiInfo"/>
	/// </summary>
	/// <param name="title">The title</param>
	/// <param name="version">The API version</param>
	public OpenApiInfo(string title, string version)
	{
		Title = title;
		Version = version;
	}

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		IRefTargetContainer? target = keys[0] switch
		{
			"contact" => Contact,
			"license" => License,
			_ => null
		};

		return target != null
			? target.Resolve(keys[1..])
			: ExtensionData?.Resolve(keys);
	}
}

internal class OpenApiInfoJsonConverter : JsonConverter<OpenApiInfo>
{
	private const string _objectType = "open api info";

	public override OpenApiInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		string? title = null;
		string? version = null;
		string? summary = null;
		string? description = null;
		Uri? termsOfService = null;
		ContactInfo? contact = null;
		LicenseInfo? license = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "title":
					title = reader.ReadString(propertyName, _objectType);
					break;
				case "version":
					version = reader.ReadString(propertyName, _objectType);
					break;
				case "summary":
					summary = reader.ReadString(propertyName, _objectType);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "termsOfService":
					termsOfService = reader.ReadUri(propertyName, _objectType);
					break;
				case "contact":
					contact = options.Read(ref reader, ApiSerializerContext.Default.ContactInfo);
					break;
				case "license":
					license = options.Read(ref reader, ApiSerializerContext.Default.LicenseInfo);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new OpenApiInfo(
			ConverterHelpers.ExpectPresent(title, "title", _objectType),
			ConverterHelpers.ExpectPresent(version, "version", _objectType))
		{
			Summary = summary,
			Description = description,
			TermsOfService = termsOfService,
			Contact = contact,
			License = license,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, OpenApiInfo value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("title", value.Title);
		writer.WriteString("version", value.Version);
		writer.MaybeWrite("summary", value.Summary);
		writer.MaybeWrite("description", value.Description);
		writer.MaybeWrite("termsOfService", value.TermsOfService);
		writer.MaybeWrite("contact", value.Contact, options, ApiSerializerContext.Default.ContactInfo);
		writer.MaybeWrite("license", value.License, options, ApiSerializerContext.Default.LicenseInfo);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
