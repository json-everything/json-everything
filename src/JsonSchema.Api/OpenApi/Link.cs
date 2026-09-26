using System;
using Json.More;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a link object.
/// </summary>
[JsonConverter(typeof(LinkJsonConverter))]
public class Link : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets a relative or absolute URI reference to an OAS operation.
	/// </summary>
	public Uri? OperationRef { get; set; }
	/// <summary>
	/// Gets or sets the name of the operation.
	/// </summary>
	public string? OperationId { get; set; }
	/// <summary>
	/// Gets or sets the parameter collection.
	/// </summary>
	public Dictionary<string, RuntimeExpression>? Parameters { get; set; } // might also be JsonNode
	/// <summary>
	/// Gets or sets the request body for the target operation.
	/// </summary>
	public RuntimeExpression? RequestBody { get; set; } // might also be JsonNode
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets the server for the target operation.
	/// </summary>
	public Server? Server { get; set; }
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

		if (keys[0] == "server")
		{
			if (keys.Length == 1) return Server;
			return Server?.Resolve(keys.Slice(1));
		}

		return ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (this is LinkRef lRef)
			yield return lRef;
	}
}

/// <summary>
/// Models a `$ref` to a link.
/// </summary>
public class LinkRef : Link, IComponentRef
{
	/// <summary>
	/// The URI for the reference.
	/// </summary>
	public Uri Ref { get; }

	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public string? Summary { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public new string? Description { get; set; }

	/// <summary>
	/// Gets whether the reference has been resolved.
	/// </summary>
	public bool IsResolved { get; private set; }

	/// <summary>
	/// Creates a new <see cref="LinkRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public LinkRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	/// <summary>
	/// Creates a new <see cref="LinkRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public LinkRef(string reference)
	{
		Ref = new Uri(reference ?? throw new ArgumentNullException(nameof(reference)), UriKind.RelativeOrAbsolute);
	}

	async Task IComponentRef.Resolve(OpenApiDocument root, JsonSerializerOptions? options)
	{
		void copy(Link other)
		{
			OperationRef = other.OperationRef;
			OperationId = other.OperationId;
			Parameters = other.Parameters;
			RequestBody = other.RequestBody;
			base.Description = other.Description;
			Server = other.Server;
			ExtensionData = other.ExtensionData;
		}

		IsResolved = await OpenApi.Ref.Resolve(root, Ref, copy, ApiSerializerContext.Default.Link, options);
	}
}

internal class LinkJsonConverter : JsonConverter<Link>
{
	private const string _objectType = "link";

	public override Link Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? reference = null;
		string? summary = null;
		Uri? operationRef = null;
		string? operationId = null;
		Dictionary<string, RuntimeExpression>? parameters = null;
		RuntimeExpression? requestBody = null;
		string? description = null;
		Server? server = null;
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
				case "operationRef":
					operationRef = reader.ReadUri(propertyName, _objectType);
					break;
				case "operationId":
					operationId = reader.ReadString(propertyName, _objectType);
					break;
				case "parameters":
					parameters = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.RuntimeExpression);
					break;
				case "requestBody":
					requestBody = RuntimeExpression.Parse(reader.ReadString(propertyName, _objectType));
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "server":
					server = options.Read(ref reader, ApiSerializerContext.Default.Server);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		if (reference is not null)
			return new LinkRef(reference)
			{
				Summary = summary,
				Description = description
			};

		return new Link
		{
			OperationRef = operationRef,
			OperationId = operationId,
			Parameters = parameters,
			RequestBody = requestBody,
			Description = description,
			Server = server,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, Link value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value is LinkRef reference)
		{
			writer.WriteString("$ref", reference.Ref.ToString());
			writer.MaybeWrite("summary", reference.Summary);
			writer.MaybeWrite("description", reference.Description);
		}
		else
		{
			writer.MaybeWrite("operationRef", value.OperationRef);
			writer.MaybeWrite("operationId", value.OperationId);
			writer.MaybeWriteMap("parameters", value.Parameters, options, ApiSerializerContext.Default.RuntimeExpression);
			writer.MaybeWrite("requestBody", value.RequestBody?.ToString());
			writer.MaybeWrite("description", value.Description);
			writer.MaybeWrite("server", value.Server, options, ApiSerializerContext.Default.Server);
			writer.WriteExtensions(value.ExtensionData, value.UnknownData);
		}

		writer.WriteEndObject();
	}
}
