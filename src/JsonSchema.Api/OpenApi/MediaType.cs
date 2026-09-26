using System;
using Json.More;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a media type object.
/// </summary>
[JsonConverter(typeof(MediaTypeJsonConverter))]
public class MediaType : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets a schema for the meta type.
	/// </summary>
	public JsonSchema? Schema { get; set; }
	/// <summary>
	/// Gets or sets an example.
	/// </summary>
	public JsonNode? Example { get; set; }
	/// <summary>
	/// Gets or sets a collection of examples.
	/// </summary>
	public Dictionary<string, Example>? Examples { get; set; }
	/// <summary>
	/// Gets or sets a collection of encodings.
	/// </summary>
	public Dictionary<string, Encoding>? Encoding { get; set; }
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

		int keysConsumed = 1;
		IRefTargetContainer? target = null;
		switch (keys[0])
		{
			case "schema":
				if (Schema == null) return null;
				if (keys.Length == 1) return Schema;
				// TODO: consider some other kind of value being buried in a schema
				throw new NotImplementedException();
			case "example":
				return Example?.GetFromNode(keys.Slice(1));
			case "examples":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Examples?.GetFromMap(keys[1]);
				break;
			case "encoding":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Encoding?.GetFromMap(keys[1]);
				break;
		}

		return target != null
			? target.Resolve(keys[keysConsumed..])
			: ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		if (Schema != null)
			yield return Schema;

		var theRest = GeneralHelpers.Collect(Encoding?.Values.SelectMany(x => x.FindSchemas()));

		foreach (var schema in theRest)
		{
			yield return schema;
		}
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		return GeneralHelpers.Collect(
			Examples?.Values.SelectMany(x => x.FindRefs()),
			Encoding?.Values.SelectMany(x => x.FindRefs())
		);
	}
}

internal class MediaTypeJsonConverter : JsonConverter<MediaType>
{
	private const string _objectType = "media type";

	public override MediaType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		JsonSchema? schema = null;
		JsonNode? example = null;
		Dictionary<string, Example>? examples = null;
		Dictionary<string, Encoding>? encoding = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "schema":
					schema = options.Read(ref reader, ApiSerializerContext.Default.JsonSchema);
					break;
				case "example":
					example = reader.ReadNode();
					break;
				case "examples":
					examples = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Example);
					break;
				case "encoding":
					encoding = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Encoding);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new MediaType
		{
			Schema = schema,
			Example = example,
			Examples = examples,
			Encoding = encoding,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, MediaType value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.MaybeWrite("schema", value.Schema, options, ApiSerializerContext.Default.JsonSchema);
		writer.MaybeWriteNode("example", value.Example);
		writer.MaybeWriteMap("examples", value.Examples, options, ApiSerializerContext.Default.Example);
		writer.MaybeWriteMap("encoding", value.Encoding, options, ApiSerializerContext.Default.Encoding);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
