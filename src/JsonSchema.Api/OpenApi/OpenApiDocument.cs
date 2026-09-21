using System;
using Json.More;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Json.Pointer;
using OpenApiVocabulary = Json.Schema.OpenApi.Vocabulary;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models the OpenAPI document.
/// </summary>
[JsonConverter(typeof(OpenApiDocumentJsonConverter))]
public class OpenApiDocument : IBaseDocument
{
	internal static readonly string[] SupportedVersions =
	[
		"3.1.0",
		"3.1.1"
	];

	private readonly Dictionary<JsonPointer, object> _lookup = new();

	/// <summary>
	/// Gets the OpenAPI document version.
	/// </summary>
	public string OpenApi { get; }
	/// <summary>
	/// Gets the API information.
	/// </summary>
	public OpenApiInfo Info { get; }
	/// <summary>
	/// Gets or sets the default JSON Schema dialect.
	/// </summary>
	public Uri? JsonSchemaDialect { get; set; }
	/// <summary>
	/// Gets or sets the server collection.
	/// </summary>
	public IReadOnlyList<Server>? Servers { get; set; }
	/// <summary>
	/// Gets or sets the paths collection.
	/// </summary>
	public PathCollection? Paths { get; set; }
	/// <summary>
	/// Gets or sets the webhooks collection.
	/// </summary>
	public Dictionary<string, PathItem>? Webhooks { get; set; }
	/// <summary>
	/// Gets or sets the components collection.
	/// </summary>
	public ComponentCollection? Components { get; set; }
	/// <summary>
	/// Gets or sets the security requirements collection.
	/// </summary>
	public IReadOnlyList<SecurityRequirement>? Security { get; set; }
	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	public IReadOnlyList<Tag>? Tags { get; set; }
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

	Uri IBaseDocument.BaseUri { get; } = GenerateBaseUri();

	private static Uri GenerateBaseUri() => new($"graeae:models:{Guid.NewGuid().ToString("N").AsSpan(0, 10).ToString()}");

	static OpenApiDocument()
	{
		FormatRegistry.Global.Register(Formats.Double);
		FormatRegistry.Global.Register(Formats.Float);
		FormatRegistry.Global.Register(Formats.Int32);
		FormatRegistry.Global.Register(Formats.Int64);
		FormatRegistry.Global.Register(Formats.Password);

		VocabularyRegistry.Global.Register(OpenApiVocabulary.OpenApi_31);
	}

	/// <summary>
	/// Creates a new <see cref="OpenApiDocument"/>
	/// </summary>
	/// <param name="openApi">The OpenAPI version</param>
	/// <param name="info">The API information</param>
	public OpenApiDocument(string openApi, OpenApiInfo info)
	{
		OpenApi = openApi;
		Info = info;
	}

	JsonSchemaNode? IBaseDocument.FindSubschema(JsonPointer pointer, BuildContext context)
	{
		return Find<JsonSchemaNode>(pointer);
	}



	/// <summary>
	/// Initializes the document model.
	/// </summary>
	/// <param name="schemaRegistry">(optional) A schema registry.</param>
	/// <param name="options">(optional) Serializer options</param>
	/// <exception cref="RefResolutionException">Thrown if a reference cannot be resolved.</exception>
	public async Task Initialize(SchemaRegistry? schemaRegistry = null, JsonSerializerOptions? options = null)
	{
		schemaRegistry ??= SchemaRegistry.Global;

		schemaRegistry.Register(this);

		// find all JSON Schemas and populate their base URIs (if they don't have $id)
		RegisterSchemas(schemaRegistry);

		// find and attempt to resolve all reference objects
		await TryResolveRefs(options);
	}

	private void RegisterSchemas(SchemaRegistry schemaRegistry)
	{
		var allSchemas = GeneralHelpers.Collect(
			Paths?.FindSchemas(),
			Webhooks?.Values.SelectMany(x => x.FindSchemas()),
			Components?.FindSchemas()
		);

		foreach (var schema in allSchemas)
		{
			schemaRegistry.Register(schema);
		}
	}

	private async Task TryResolveRefs(JsonSerializerOptions? options)
	{
		var allRefs = GeneralHelpers.Collect(
			Paths?.FindRefs(),
			Webhooks?.Values.SelectMany(x => x.FindRefs()),
			Components?.FindRefs()
		);

		await Task.WhenAll(allRefs.Select(x => x.Resolve(this, options)));
	}

	/// <summary>
	/// Finds and retrieves an object within the document at a specified location.
	/// </summary>
	/// <typeparam name="T">The type of object</typeparam>
	/// <param name="pointer">The expected location</param>
	/// <returns>The object, if an object of that type exists at that location; otherwise null.</returns>
	/// <remarks>
	/// The lookup walks the model, so the value found is already a model object.  Pointers
	/// that land inside extension data or an example value yield a <see cref="JsonNode"/>,
	/// which is returned only when <typeparamref name="T"/> can hold it.
	/// </remarks>
	public T? Find<T>(JsonPointer pointer)
		where T : class
	{
		if (!_lookup.TryGetValue(pointer, out var val))
		{
			var keys = new string[pointer.SegmentCount];
			for (var i = 0; i < pointer.SegmentCount; i++)
			{
				keys[i] = pointer[i].ToString();
			}

			val = PerformLookup(keys) as T;
			if (val != null)
				_lookup[pointer] = val;
		}

		return val as T;
	}

	private object? PerformLookup(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		int keysConsumed = 1;
		IRefTargetContainer? target = null;
		switch (keys[0])
		{
			case "info":
				target = Info;
				break;
			case "servers":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Servers?.GetFromArray(keys[1]);
				break;
			case "paths":
				target = Paths;
				break;
			case "webhooks":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Webhooks?.GetFromMap(keys[1]);
				break;
			case "components":
				target = Components;
				break;
			case "tags":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Tags?.GetFromArray(keys[1]);
				break;
			case "externalDocs":
				target = ExternalDocs;
				break;
		}

		return target != null
			? target.Resolve(keys.Slice(keysConsumed))
			: ExtensionData?.Resolve(keys);
	}
}

/// <summary>
/// JSON converter for <see cref="OpenApiDocument"/>.
/// </summary>
public class OpenApiDocumentJsonConverter : JsonConverter<OpenApiDocument>
{
	private const string _objectType = "open api document";

	/// <summary>Reads and converts the JSON to an <see cref="OpenApiDocument"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override OpenApiDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		string? openapi = null;
		OpenApiInfo? info = null;
		Uri? jsonSchemaDialect = null;
		List<Server>? servers = null;
		PathCollection? paths = null;
		Dictionary<string, PathItem>? webhooks = null;
		ComponentCollection? components = null;
		List<SecurityRequirement>? security = null;
		List<Tag>? tags = null;
		ExternalDocumentation? externalDocs = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "openapi":
					openapi = reader.ReadString(propertyName, _objectType);
					break;
				case "info":
					info = options.Read(ref reader, ApiSerializerContext.Default.OpenApiInfo);
					break;
				case "jsonSchemaDialect":
					jsonSchemaDialect = reader.ReadUri(propertyName, _objectType);
					break;
				case "servers":
					servers = reader.ReadArray(propertyName, _objectType, options, ApiSerializerContext.Default.Server);
					break;
				case "paths":
					paths = options.Read(ref reader, ApiSerializerContext.Default.PathCollection);
					break;
				case "webhooks":
					webhooks = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.PathItem);
					break;
				case "components":
					components = options.Read(ref reader, ApiSerializerContext.Default.ComponentCollection);
					break;
				case "security":
					security = reader.ReadArray(propertyName, _objectType, options, ApiSerializerContext.Default.SecurityRequirement);
					break;
				case "tags":
					tags = reader.ReadArray(propertyName, _objectType, options, ApiSerializerContext.Default.Tag);
					break;
				case "externalDocs":
					externalDocs = options.Read(ref reader, ApiSerializerContext.Default.ExternalDocumentation);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		openapi = ConverterHelpers.ExpectPresent(openapi, "openapi", _objectType);
		if (!OpenApiDocument.SupportedVersions.Contains(openapi))
			throw new JsonException($"Version '{openapi}' is not supported.");

		return new OpenApiDocument(openapi, ConverterHelpers.ExpectPresent(info, "info", _objectType))
		{
			JsonSchemaDialect = jsonSchemaDialect,
			Servers = servers,
			Paths = paths,
			Webhooks = webhooks,
			Components = components,
			Security = security,
			Tags = tags,
			ExternalDocs = externalDocs,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, OpenApiDocument value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("openapi", value.OpenApi);
		writer.MaybeWrite("info", value.Info, options, ApiSerializerContext.Default.OpenApiInfo);
		writer.MaybeWrite("jsonSchemaDialect", value.JsonSchemaDialect);
		writer.MaybeWriteArray("servers", value.Servers, options, ApiSerializerContext.Default.Server);
		writer.MaybeWrite("paths", value.Paths, options, ApiSerializerContext.Default.PathCollection);
		writer.MaybeWriteMap("webhooks", value.Webhooks, options, ApiSerializerContext.Default.PathItem);
		writer.MaybeWrite("components", value.Components, options, ApiSerializerContext.Default.ComponentCollection);
		writer.MaybeWriteArray("security", value.Security, options, ApiSerializerContext.Default.SecurityRequirement);
		writer.MaybeWriteArray("tags", value.Tags, options, ApiSerializerContext.Default.Tag);
		writer.MaybeWrite("externalDocs", value.ExternalDocs, options, ApiSerializerContext.Default.ExternalDocumentation);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}