using System;
using Json.More;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a path collection.
/// </summary>
[JsonConverter(typeof(PathCollectionJsonConverter))]
public class PathCollection : Dictionary<PathTemplate, PathItem>, IRefTargetContainer
{
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
		if (keys.Length == 0) return null;

		return this.GetFromMap(keys[0])?.Resolve(keys.Slice(1)) ??
		       ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return Values.SelectMany(x => x.FindSchemas());
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		return Values.SelectMany(x => x.FindRefs());
	}
}

internal class PathCollectionJsonConverter : JsonConverter<PathCollection>
{
	private const string _objectType = "path collection";

	public override PathCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		var collection = new PathCollection();
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			if (ConverterHelpers.IsExtensionKey(propertyName))
			{
				reader.ReadExtension(propertyName, ref extensionData);
				continue;
			}

			if (!PathTemplate.TryParse(propertyName, out var template))
				throw new JsonException($"`{propertyName}` is not a valid path template");

			collection.Add(template, options.Read(ref reader, ApiSerializerContext.Default.PathItem)!);
		}

		collection.ExtensionData = extensionData;
		collection.UnknownData = unknownData;

		return collection;
	}

	public override void Write(Utf8JsonWriter writer, PathCollection value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		foreach (var kvp in value)
		{
			writer.WritePropertyName(kvp.Key.ToString());
			options.Write(writer, kvp.Value, ApiSerializerContext.Default.PathItem);
		}

		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
