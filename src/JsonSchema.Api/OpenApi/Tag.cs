using System;
using Json.More;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a tag.
/// </summary>
[JsonConverter(typeof(TagJsonConverter))]
public class Tag : IRefTargetContainer
{
	/// <summary>
	/// Gets the tag name.
	/// </summary>
	public string Name { get; }
	/// <summary>
	/// Gets or sets the tag description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets external documentation.
	/// </summary>
	public ExternalDocumentation? ExternalDocs { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="Tag"/>
	/// </summary>
	/// <param name="name">The tag name</param>
	public Tag(string name)
	{
		Name = name;
	}

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		if (keys[0] == "externalDocs")
		{
			if (keys.Length == 1) return ExternalDocs;
			return ExternalDocs?.Resolve(keys.Slice(1));
		}

		return ExtensionData?.Resolve(keys);
	}
}

internal class TagJsonConverter : JsonConverter<Tag>
{
	private const string _objectType = "tag";

	public override Tag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		string? name = null;
		string? description = null;
		ExternalDocumentation? externalDocs = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "name":
					name = reader.ReadString(propertyName, _objectType);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "externalDocs":
					externalDocs = options.Read(ref reader, ApiSerializerContext.Default.ExternalDocumentation);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new Tag(ConverterHelpers.ExpectPresent(name, "name", _objectType))
		{
			Description = description,
			ExternalDocs = externalDocs,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, Tag value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("name", value.Name);
		writer.MaybeWrite("description", value.Description);
		writer.MaybeWrite("externalDocs", value.ExternalDocs, options, ApiSerializerContext.Default.ExternalDocumentation);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}