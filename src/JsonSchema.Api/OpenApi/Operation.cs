using System;
using Json.More;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models an operation.
/// </summary>
[JsonConverter(typeof(OperationJsonConverter))]
public class Operation : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	public IReadOnlyList<string>? Tags { get; set; }
	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public string? Summary { get; set; }
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets external documentation.
	/// </summary>
	public ExternalDocumentation? ExternalDocs { get; set; }
	/// <summary>
	/// Gets or sets the operation ID.
	/// </summary>
	public string? OperationId { get; set; }
	/// <summary>
	/// Gets or sets the parameters.
	/// </summary>
	public IReadOnlyList<Parameter>? Parameters { get; set; }
	/// <summary>
	/// Gets or sets the request body.
	/// </summary>
	public RequestBody? RequestBody { get; set; }
	/// <summary>
	/// Gets or sets the response collection.
	/// </summary>
	public ResponseCollection? Responses { get; set; }
	/// <summary>
	/// Gets or sets the callbacks collection.
	/// </summary>
	public Dictionary<string, Callback>? Callbacks { get; set; }
	/// <summary>
	/// Gets or sets whether the operation is deprecated.
	/// </summary>
	public bool? Deprecated { get; set; }
	/// <summary>
	/// Gets or sets the security requirements.
	/// </summary>
	public IReadOnlyList<SecurityRequirement>? Security { get; set; }
	/// <summary>
	/// Gets or sets the server collection.
	/// </summary>
	public IReadOnlyList<Server>? Servers { get; set; }
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
			case "externalDocs":
				target = ExternalDocs;
				break;
			case "parameters":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Parameters?.GetFromArray(keys[1]);
				break;
			case "requestBody":
				target = RequestBody;
				break;
			case "responses":
				target = Responses;
				break;
			case "callbacks":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Callbacks?.GetFromMap(keys[1]);
				break;
			case "servers":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Servers?.GetFromArray(keys[1]);
				break;
		}

		return target != null
			? target.Resolve(keys.Slice(keysConsumed))
			: ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return GeneralHelpers.Collect(
			Parameters?.SelectMany(x => x.FindSchemas()),
			RequestBody?.FindSchemas(),
			Responses?.FindSchemas(),
			Callbacks?.Values.SelectMany(x => x.FindSchemas())
		);
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		return GeneralHelpers.Collect(
			Parameters?.SelectMany(x => x.FindRefs()),
			RequestBody?.FindRefs(),
			Responses?.FindRefs(),
			Callbacks?.Values.SelectMany(x => x.FindRefs())
		);
	}
}

internal class OperationJsonConverter : JsonConverter<Operation>
{
	private const string _objectType = "operation";

	public override Operation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		List<string>? tags = null;
		string? summary = null;
		string? description = null;
		ExternalDocumentation? externalDocs = null;
		string? operationId = null;
		List<Parameter>? parameters = null;
		RequestBody? requestBody = null;
		ResponseCollection? responses = null;
		Dictionary<string, Callback>? callbacks = null;
		bool? deprecated = null;
		List<SecurityRequirement>? security = null;
		List<Server>? servers = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "tags":
					tags = reader.ReadStringArray(propertyName, _objectType, options);
					break;
				case "summary":
					summary = reader.ReadString(propertyName, _objectType);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "externalDocs":
					externalDocs = options.Read(ref reader, ApiSerializerContext.Default.ExternalDocumentation);
					break;
				case "operationId":
					operationId = reader.ReadString(propertyName, _objectType);
					break;
				case "parameters":
					parameters = reader.ReadArray(propertyName, _objectType, options, ApiSerializerContext.Default.Parameter);
					break;
				case "requestBody":
					requestBody = options.Read(ref reader, ApiSerializerContext.Default.RequestBody);
					break;
				case "responses":
					responses = options.Read(ref reader, ApiSerializerContext.Default.ResponseCollection);
					break;
				case "callbacks":
					callbacks = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Callback);
					break;
				case "deprecated":
					deprecated = reader.ReadBool(propertyName, _objectType);
					break;
				case "security":
					security = reader.ReadArray(propertyName, _objectType, options, ApiSerializerContext.Default.SecurityRequirement);
					break;
				case "servers":
					servers = reader.ReadArray(propertyName, _objectType, options, ApiSerializerContext.Default.Server);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new Operation
		{
			Tags = tags,
			Summary = summary,
			Description = description,
			ExternalDocs = externalDocs,
			OperationId = operationId,
			Parameters = parameters,
			RequestBody = requestBody,
			Responses = responses,
			Callbacks = callbacks,
			Deprecated = deprecated,
			Security = security,
			Servers = servers,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, Operation value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.MaybeWriteStringArray("tags", value.Tags, options);
		writer.MaybeWrite("summary", value.Summary);
		writer.MaybeWrite("description", value.Description);
		writer.MaybeWrite("externalDocs", value.ExternalDocs, options, ApiSerializerContext.Default.ExternalDocumentation);
		writer.MaybeWrite("operationId", value.OperationId);
		writer.MaybeWriteArray("parameters", value.Parameters, options, ApiSerializerContext.Default.Parameter);
		writer.MaybeWrite("requestBody", value.RequestBody, options, ApiSerializerContext.Default.RequestBody);
		writer.MaybeWrite("responses", value.Responses, options, ApiSerializerContext.Default.ResponseCollection);
		writer.MaybeWriteMap("callbacks", value.Callbacks, options, ApiSerializerContext.Default.Callback);
		writer.MaybeWrite("deprecated", value.Deprecated);
		writer.MaybeWriteArray("security", value.Security, options, ApiSerializerContext.Default.SecurityRequirement);
		writer.MaybeWriteArray("servers", value.Servers, options, ApiSerializerContext.Default.Server);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
