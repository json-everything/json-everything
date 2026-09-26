using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a response.
/// </summary>
[JsonConverter(typeof(ResponseJsonConverter))]
public class Response : IRefTargetContainer
{
	/// <summary>
	/// Gets the description.
	/// </summary>
	public string Description { get; private protected set; }
	/// <summary>
	/// Gets or sets the header collection.
	/// </summary>
	public Dictionary<string, Header>? Headers { get; set; }
	/// <summary>
	/// Gets or sets the content collection.
	/// </summary>
	public Dictionary<string, MediaType>? Content { get; set; }
	/// <summary>
	/// Gets or sets the link collection.
	/// </summary>
	public Dictionary<string, Link>? Links { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="Response"/>
	/// </summary>
	/// <param name="description">The description</param>
	public Response(string description)
	{
		Description = description;
	}
#pragma warning disable CS8618
	private protected Response(){}
#pragma warning restore CS8618

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		int keysConsumed = 1;
		IRefTargetContainer? target = null;
		switch (keys[0])
		{
			case "headers":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Headers?.GetFromMap(keys[1]);
				break;
			case "content":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Content?.GetFromMap(keys[1]);
				break;
			case "links":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Links?.GetFromMap(keys[1]);
				break;
		}

		return target != null
			? target.Resolve(keys[keysConsumed..])
			: ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return GeneralHelpers.Collect(
			Headers?.Values.SelectMany(x => x.FindSchemas()),
			Content?.Values.SelectMany(x => x.FindSchemas())
		);
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (this is ResponseRef rRef)
			yield return rRef;

		var theRest = GeneralHelpers.Collect(
			Headers?.Values.SelectMany(x => x.FindRefs()),
			Content?.Values.SelectMany(x => x.FindRefs()),
			Links?.Values.SelectMany(x => x.FindRefs())
		);

		foreach (var parameter in theRest)
		{
			yield return parameter;
		}
	}
}

/// <summary>
/// Models a `$ref` to a response.
/// </summary>
public class ResponseRef : Response, IComponentRef
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
	/// Creates a new <see cref="ResponseRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public ResponseRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	/// <summary>
	/// Creates a new <see cref="ResponseRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public ResponseRef(string reference)
	{
		Ref = new Uri(reference ?? throw new ArgumentNullException(nameof(reference)), UriKind.RelativeOrAbsolute);
	}

	async Task IComponentRef.Resolve(OpenApiDocument root, JsonSerializerOptions? options)
	{
		void copy(Response other)
		{
			base.Description = other.Description;
			Headers = other.Headers;
			Content = other.Content;
			Links = other.Links;
			ExtensionData = other.ExtensionData;
		}

		IsResolved = await OpenApi.Ref.Resolve(root, Ref, copy, ApiSerializerContext.Default.Response, options);
	}
}

internal class ResponseJsonConverter : JsonConverter<Response>
{
	private const string _objectType = "response";

	public override Response Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? reference = null;
		string? summary = null;
		string? description = null;
		Dictionary<string, Header>? headers = null;
		Dictionary<string, MediaType>? content = null;
		Dictionary<string, Link>? links = null;
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
				case "headers":
					headers = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Header);
					break;
				case "content":
					content = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.MediaType);
					break;
				case "links":
					links = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.Link);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		if (reference is not null)
			return new ResponseRef(reference)
			{
				Summary = summary,
				Description = description
			};

		return new Response(ConverterHelpers.ExpectPresent(description, "description", _objectType))
		{
			Headers = headers,
			Content = content,
			Links = links,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, Response value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value is ResponseRef reference)
		{
			writer.WriteString("$ref", reference.Ref.ToString());
			writer.MaybeWrite("summary", reference.Summary);
			writer.MaybeWrite("description", reference.Description);
		}
		else
		{
			writer.WriteString("description", value.Description);
			writer.MaybeWriteMap("headers", value.Headers, options, ApiSerializerContext.Default.Header);
			writer.MaybeWriteMap("content", value.Content, options, ApiSerializerContext.Default.MediaType);
			writer.MaybeWriteMap("links", value.Links, options, ApiSerializerContext.Default.Link);
			writer.WriteExtensions(value.ExtensionData, value.UnknownData);
		}

		writer.WriteEndObject();
	}
}