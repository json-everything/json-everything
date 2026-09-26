using System;
using Json.More;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a response collection.
/// </summary>
[JsonConverter(typeof(ResponseCollectionJsonConverter))]
public class ResponseCollection : Dictionary<HttpStatusCode, Response>, IRefTargetContainer
{
	/// <summary>
	/// Gets or sets the default response for the collection.
	/// </summary>
	public Response? Default { get; set; }
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

		var first = keys[0];
		return this.FirstOrDefault(x => ((int)x.Key).ToString() == first).Value?.Resolve(keys.Slice(1)) ??
		       ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return GeneralHelpers.Collect(
			Default?.FindSchemas(),
			Values.SelectMany(x => x.FindSchemas())
		);
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (Default is ResponseRef rRef)
			yield return rRef;

		var theRest = Values.SelectMany(x => x.FindRefs());

		foreach (var reference in theRest)
		{
			yield return reference;
		}
	}
}

internal class ResponseCollectionJsonConverter : JsonConverter<ResponseCollection>
{
	private const string _objectType = "response collection";

	public override ResponseCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		var collection = new ResponseCollection();
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			if (propertyName == "default")
			{
				collection.Default = options.Read(ref reader, ApiSerializerContext.Default.Response);
				continue;
			}

			if (ConverterHelpers.IsExtensionKey(propertyName))
			{
				reader.ReadExtension(propertyName, ref extensionData);
				continue;
			}

			if (!int.TryParse(propertyName, out var statusCode))
			{
				unknownData ??= [];
				unknownData.Add(propertyName, reader.ReadNode());
				continue;
			}

			collection.Add((HttpStatusCode)statusCode, options.Read(ref reader, ApiSerializerContext.Default.Response)!);
		}

		collection.ExtensionData = extensionData;
		collection.UnknownData = unknownData;

		return collection;
	}

	public override void Write(Utf8JsonWriter writer, ResponseCollection value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.MaybeWrite("default", value.Default, options, ApiSerializerContext.Default.Response);

		foreach (var kvp in value)
		{
			writer.WritePropertyName(((int)kvp.Key).ToString());
			options.Write(writer, kvp.Value, ApiSerializerContext.Default.Response);
		}

		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}