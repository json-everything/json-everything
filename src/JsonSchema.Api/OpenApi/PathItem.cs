using System;
using Json.More;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models an individual path.
/// </summary>
[JsonConverter(typeof(PathItemJsonConverter))]
public class PathItem : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public string? Summary { get; set; }
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets the GET operation.
	/// </summary>
	public Operation? Get { get; set; }
	/// <summary>
	/// Gets or sets the PUT operation.
	/// </summary>
	public Operation? Put { get; set; }
	/// <summary>
	/// Gets or sets the POST operation.
	/// </summary>
	public Operation? Post { get; set; }
	/// <summary>
	/// Gets or sets the DELETE operation.
	/// </summary>
	public Operation? Delete { get; set; }
	/// <summary>
	/// Gets or sets the OPTIONS operation.
	/// </summary>
	public Operation? Options { get; set; }
	/// <summary>
	/// Gets or sets the HEAD operation.
	/// </summary>
	public Operation? Head { get; set; }
	/// <summary>
	/// Gets or sets the PATCH operation.
	/// </summary>
	public Operation? Patch { get; set; }
	/// <summary>
	/// Gets or sets the TRACE operation.
	/// </summary>
	public Operation? Trace { get; set; }
	/// <summary>
	/// Gets or sets the collection of servers.
	/// </summary>
	public IReadOnlyList<Server>? Servers { get; set; }
	/// <summary>
	/// Gets or sets the collection of parameters.
	/// </summary>
	public IReadOnlyList<Parameter>? Parameters { get; set; }
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
			case "get":
				target = Get;
				break;
			case "put":
				target = Put;
				break;
			case "post":
				target = Post;
				break;
			case "delete":
				target = Delete;
				break;
			case "options":
				target = Options;
				break;
			case "head":
				target = Head;
				break;
			case "patch":
				target = Patch;
				break;
			case "trace":
				target = Trace;
				break;
			case "servers":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Servers?.GetFromArray(keys[1]);
				break;
			case "parameters":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Parameters?.GetFromArray(keys[1]);
				break;
		}

		return target != null
			? target.Resolve(keys[keysConsumed..])
			: ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return GeneralHelpers.Collect(
			Get?.FindSchemas(),
			Put?.FindSchemas(),
			Post?.FindSchemas(),
			Delete?.FindSchemas(),
			Options?.FindSchemas(),
			Head?.FindSchemas(),
			Patch?.FindSchemas(),
			Trace?.FindSchemas(),
			Parameters?.SelectMany(x => x.FindSchemas())
		);
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (this is PathItemRef piRef)
			yield return piRef;

		var theRest = GeneralHelpers.Collect(
			Get?.FindRefs(),
			Put?.FindRefs(),
			Post?.FindRefs(),
			Delete?.FindRefs(),
			Options?.FindRefs(),
			Head?.FindRefs(),
			Patch?.FindRefs(),
			Trace?.FindRefs(),
			Parameters?.SelectMany(x => x.FindRefs())
		);

		foreach (var compRef in theRest)
		{
			yield return compRef;
		}

		
	}
}

/// <summary>
/// Models a `$ref` to a path item.
/// </summary>
public class PathItemRef : PathItem, IComponentRef
{
	/// <summary>
	/// The URI for the reference.
	/// </summary>
	public Uri Ref { get; }

	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public new string? Summary { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public new string? Description { get; set; }

	/// <summary>
	/// Gets whether the reference has been resolved.
	/// </summary>
	public bool IsResolved { get; private set; }

	/// <summary>
	/// Creates a new <see cref="PathItemRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public PathItemRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	/// <summary>
	/// Creates a new <see cref="PathItemRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public PathItemRef(string reference)
	{
		Ref = new Uri(reference ?? throw new ArgumentNullException(nameof(reference)), UriKind.RelativeOrAbsolute);
	}

	async Task IComponentRef.Resolve(OpenApiDocument root, JsonSerializerOptions? options)
	{
		void copy(PathItem other)
		{
			// PathItem is different from the other $ref-able objects in that the $ref is
			// integrated and the $ref'd values can be overridden.
			base.Summary = other.Summary;
			base.Description = other.Description;
			Get ??= other.Get;
			Put ??= other.Put;
			Post ??= other.Post;
			Delete ??= other.Delete;
			Options ??= other.Options;
			Head ??= other.Head;
			Patch ??= other.Patch;
			Trace ??= other.Trace;
			Servers ??= other.Servers;
			Parameters ??= other.Parameters;
			ExtensionData = other.ExtensionData;
		}

		IsResolved = await OpenApi.Ref.Resolve(root, Ref, copy, ApiSerializerContext.Default.PathItem, options);
	}
}

internal class PathItemJsonConverter : JsonConverter<PathItem>
{
	private const string _objectType = "path item";

	public override PathItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? reference = null;
		string? summary = null;
		string? description = null;
		Operation? get = null;
		Operation? put = null;
		Operation? post = null;
		Operation? delete = null;
		Operation? optionsOp = null;
		Operation? head = null;
		Operation? patch = null;
		Operation? trace = null;
		List<Server>? servers = null;
		List<Parameter>? parameters = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "$ref":
					reference = reader.ReadUri(propertyName, _objectType);
					break;
				case "summary":
					summary = reader.ReadString(propertyName, _objectType);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "get":
					get = options.Read(ref reader, ApiSerializerContext.Default.Operation);
					break;
				case "put":
					put = options.Read(ref reader, ApiSerializerContext.Default.Operation);
					break;
				case "post":
					post = options.Read(ref reader, ApiSerializerContext.Default.Operation);
					break;
				case "delete":
					delete = options.Read(ref reader, ApiSerializerContext.Default.Operation);
					break;
				case "options":
					optionsOp = options.Read(ref reader, ApiSerializerContext.Default.Operation);
					break;
				case "head":
					head = options.Read(ref reader, ApiSerializerContext.Default.Operation);
					break;
				case "patch":
					patch = options.Read(ref reader, ApiSerializerContext.Default.Operation);
					break;
				case "trace":
					trace = options.Read(ref reader, ApiSerializerContext.Default.Operation);
					break;
				case "servers":
					servers = reader.ReadArray(propertyName, _objectType, options, ApiSerializerContext.Default.Server);
					break;
				case "parameters":
					parameters = reader.ReadArray(propertyName, _objectType, options, ApiSerializerContext.Default.Parameter);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		// PathItem is different from the other $ref-able objects in that the $ref is
		// integrated and the $ref'd values can be overridden, so sibling keys are kept.
		var item = reference is not null
			? new PathItemRef(reference) { Summary = summary, Description = description }
			: new PathItem { Summary = summary, Description = description };

		item.Get = get;
		item.Put = put;
		item.Post = post;
		item.Delete = delete;
		item.Options = optionsOp;
		item.Head = head;
		item.Patch = patch;
		item.Trace = trace;
		item.Servers = servers;
		item.Parameters = parameters;
		item.ExtensionData = extensionData;
		item.UnknownData = unknownData;

		return item;
	}

	public override void Write(Utf8JsonWriter writer, PathItem value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value is PathItemRef reference)
			writer.WriteString("$ref", reference.Ref.ToString());

		writer.MaybeWrite("summary", value.Summary);
		writer.MaybeWrite("description", value.Description);
		writer.MaybeWrite("get", value.Get, options, ApiSerializerContext.Default.Operation);
		writer.MaybeWrite("put", value.Put, options, ApiSerializerContext.Default.Operation);
		writer.MaybeWrite("post", value.Post, options, ApiSerializerContext.Default.Operation);
		writer.MaybeWrite("delete", value.Delete, options, ApiSerializerContext.Default.Operation);
		writer.MaybeWrite("options", value.Options, options, ApiSerializerContext.Default.Operation);
		writer.MaybeWrite("head", value.Head, options, ApiSerializerContext.Default.Operation);
		writer.MaybeWrite("patch", value.Patch, options, ApiSerializerContext.Default.Operation);
		writer.MaybeWrite("trace", value.Trace, options, ApiSerializerContext.Default.Operation);
		writer.MaybeWriteArray("servers", value.Servers, options, ApiSerializerContext.Default.Server);
		writer.MaybeWriteArray("parameters", value.Parameters, options, ApiSerializerContext.Default.Parameter);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
