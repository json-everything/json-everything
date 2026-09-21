using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models the `components` collection.
/// </summary>
[JsonConverter(typeof(ComponentCollectionJsonConverter))]
public class ComponentCollection : IRefTargetContainer
{

	/// <summary>
	/// Gets or sets the schema components.
	/// </summary>
	public Dictionary<string, JsonSchema>? Schemas { get; set; }
	/// <summary>
	/// Gets or sets the response components.
	/// </summary>
	public Dictionary<string, Response>? Responses { get; set; }
	/// <summary>
	/// Gets or sets the parameter components.
	/// </summary>
	public Dictionary<string, Parameter>? Parameters { get; set; }
	/// <summary>
	/// Gets or sets the example components.
	/// </summary>
	public Dictionary<string, Example>? Examples { get; set; }
	/// <summary>
	/// Gets or sets the request body components.
	/// </summary>
	public Dictionary<string, RequestBody>? RequestBodies { get; set; }
	/// <summary>
	/// Gets or sets the header components.
	/// </summary>
	public Dictionary<string, Header>? Headers { get; set; }
	/// <summary>
	/// Gets or sets the security scheme components.
	/// </summary>
	public Dictionary<string, SecurityScheme>? SecuritySchemes { get; set; }
	/// <summary>
	/// Gets or sets the link components.
	/// </summary>
	public Dictionary<string, Link>? Links { get; set; }
	/// <summary>
	/// Gets or sets the callback components.
	/// </summary>
	public Dictionary<string, Callback>? Callbacks { get; set; }
	/// <summary>
	/// Gets or sets the path item components.
	/// </summary>
	public Dictionary<string, PathItem>? PathItems { get; set; }
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

		IRefTargetContainer? target = null;
		switch (keys[0])
		{
			case "schemas":
				if (Schemas == null || !Schemas.TryGetValue(keys[1], out var targetSchema)) return null;
				if (keys.Length == 2) return targetSchema;
				// TODO: consider some other kind of value being buried in a schema
				throw new NotImplementedException();
			case "responses":
				if (keys.Length == 1) return null;
				target = Responses?.GetFromMap(keys[1]);
				break;
			case "parameters":
				if (keys.Length == 1) return null;
				target = Parameters?.GetFromMap(keys[1]);
				break;
			case "examples":
				if (keys.Length == 1) return null;
				target = Examples?.GetFromMap(keys[1]);
				break;
			case "requestBodies":
				if (keys.Length == 1) return null;
				target = RequestBodies?.GetFromMap(keys[1]);
				break;
			case "headers":
				if (keys.Length == 1) return null;
				target = Headers?.GetFromMap(keys[1]);
				break;
			case "securitySchemes":
				if (keys.Length == 1) return null;
				target = SecuritySchemes?.GetFromMap(keys[1]);
				break;
			case "links":
				if (keys.Length == 1) return null;
				target = Links?.GetFromMap(keys[1]);
				break;
			case "callbacks":
				if (keys.Length == 1) return null;
				target = Callbacks?.GetFromMap(keys[1]);
				break;
			case "pathItems":
				if (keys.Length == 1) return null;
				target = PathItems?.GetFromMap(keys[1]);
				break;
		}

		return target != null
			? target.Resolve(keys[2..])
			: ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return GeneralHelpers.Collect(Schemas?.Values,
			Responses?.Values.SelectMany(x => x.FindSchemas()),
			Parameters?.Values.SelectMany(x => x.FindSchemas()),
			RequestBodies?.Values.SelectMany(x => x.FindSchemas()),
			Headers?.Values.SelectMany(x => x.FindSchemas()),
			Callbacks?.Values.SelectMany(x => x.FindSchemas()),
			PathItems?.Values.SelectMany(x => x.FindSchemas())
		);
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		return GeneralHelpers.Collect(
			Responses?.Values.SelectMany(x => x.FindRefs()),
			Parameters?.Values.SelectMany(x => x.FindRefs()),
			Examples?.Values.SelectMany(x => x.FindRefs()),
			RequestBodies?.Values.SelectMany(x => x.FindRefs()),
			Headers?.Values.SelectMany(x => x.FindRefs()),
			SecuritySchemes?.Values.SelectMany(x => x.FindRefs()),
			Links?.Values.SelectMany(x => x.FindRefs()),
			Callbacks?.Values.SelectMany(x => x.FindRefs()),
			PathItems?.Values.SelectMany(x => x.FindRefs())
		);
	}
}

internal class ComponentCollectionJsonConverter : JsonConverter<ComponentCollection>
{
	private const string _objectType = "components";

	public override ComponentCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Dictionary<string, JsonSchema>? schemas = null;
		Dictionary<string, Response>? responses = null;
		Dictionary<string, Parameter>? parameters = null;
		Dictionary<string, Example>? examples = null;
		Dictionary<string, RequestBody>? requestBodies = null;
		Dictionary<string, Header>? headers = null;
		Dictionary<string, SecurityScheme>? securitySchemes = null;
		Dictionary<string, Link>? links = null;
		Dictionary<string, Callback>? callbacks = null;
		Dictionary<string, PathItem>? pathItems = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "schemas":
					schemas = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.JsonSchema);
					break;
				case "responses":
					responses = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Response);
					break;
				case "parameters":
					parameters = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Parameter);
					break;
				case "examples":
					examples = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Example);
					break;
				case "requestBodies":
					requestBodies = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.RequestBody);
					break;
				case "headers":
					headers = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Header);
					break;
				case "securitySchemes":
					securitySchemes = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.SecurityScheme);
					break;
				case "links":
					links = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Link);
					break;
				case "callbacks":
					callbacks = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Callback);
					break;
				case "pathItems":
					pathItems = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.PathItem);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new ComponentCollection
		{
			Schemas = schemas,
			Responses = responses,
			Parameters = parameters,
			Examples = examples,
			RequestBodies = requestBodies,
			Headers = headers,
			SecuritySchemes = securitySchemes,
			Links = links,
			Callbacks = callbacks,
			PathItems = pathItems,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, ComponentCollection value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.MaybeWriteMap("schemas", value.Schemas, options, ApiSerializerContext.Default.JsonSchema);
		writer.MaybeWriteMap("responses", value.Responses, options, ApiSerializerContext.Default.Response);
		writer.MaybeWriteMap("parameters", value.Parameters, options, ApiSerializerContext.Default.Parameter);
		writer.MaybeWriteMap("examples", value.Examples, options, ApiSerializerContext.Default.Example);
		writer.MaybeWriteMap("requestBodies", value.RequestBodies, options, ApiSerializerContext.Default.RequestBody);
		writer.MaybeWriteMap("headers", value.Headers, options, ApiSerializerContext.Default.Header);
		writer.MaybeWriteMap("securitySchemes", value.SecuritySchemes, options, ApiSerializerContext.Default.SecurityScheme);
		writer.MaybeWriteMap("links", value.Links, options, ApiSerializerContext.Default.Link);
		writer.MaybeWriteMap("callbacks", value.Callbacks, options, ApiSerializerContext.Default.Callback);
		writer.MaybeWriteMap("pathItems", value.PathItems, options, ApiSerializerContext.Default.PathItem);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}