using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a request body.
/// </summary>
[JsonConverter(typeof(RequestBodyJsonConverter))]
public class RequestBody : IRefTargetContainer
{
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets the content collection.
	/// </summary>
	public Dictionary<string, MediaType> Content { get; private protected set; }
	/// <summary>
	/// Gets or sets whether the request body is required.
	/// </summary>
	public bool? Required { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="RequestBody"/>
	/// </summary>
	/// <param name="content"></param>
	public RequestBody(Dictionary<string, MediaType> content)
	{
		Content = content;
	}
#pragma warning disable CS8618
	private protected RequestBody(){}
#pragma warning restore CS8618

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		if (keys[0] == "server")
		{
			if (keys.Length == 1) return null;
			var target = Content.GetFromMap(keys[1]);
			return target?.Resolve(keys.Slice(2));
		}

		return ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		return Content.Values.SelectMany(x => x.FindSchemas());
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (this is RequestBodyRef rbRef)
			yield return rbRef;

		var theRest = Content.Values.SelectMany(x => x.FindRefs());

		foreach (var reference in theRest)
		{
			yield return reference;
		}
	}
}

/// <summary>
/// Models a `$ref` to a request body.
/// </summary>
public class RequestBodyRef : RequestBody, IComponentRef
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
	/// Creates a new <see cref="RequestBodyRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public RequestBodyRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	/// <summary>
	/// Creates a new <see cref="RequestBodyRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public RequestBodyRef(string reference)
	{
		Ref = new Uri(reference ?? throw new ArgumentNullException(nameof(reference)), UriKind.RelativeOrAbsolute);
	}

	async Task IComponentRef.Resolve(OpenApiDocument root, JsonSerializerOptions? options)
	{

		void copy(RequestBody other)
		{
			Content = other.Content;
			base.Description = other.Description;
			Required = other.Required;
			ExtensionData = other.ExtensionData;
		}

		IsResolved = await OpenApi.Ref.Resolve(root, Ref, copy, ApiSerializerContext.Default.RequestBody, options);
	}
}

internal class RequestBodyJsonConverter : JsonConverter<RequestBody>
{
	private const string _objectType = "request body";

	public override RequestBody Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? reference = null;
		string? summary = null;
		string? description = null;
		Dictionary<string, MediaType>? content = null;
		bool? required = null;
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
				case "content":
					content = reader.ReadMap(propertyName, _objectType, options, ApiSerializerContext.Default.MediaType);
					break;
				case "required":
					required = reader.ReadBool(propertyName, _objectType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		if (reference is not null)
			return new RequestBodyRef(reference)
			{
				Summary = summary,
				Description = description
			};

		return new RequestBody(ConverterHelpers.ExpectPresent(content, "content", _objectType))
		{
			Description = description,
			Required = required,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, RequestBody value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value is RequestBodyRef reference)
		{
			writer.WriteString("$ref", reference.Ref.ToString());
			writer.MaybeWrite("summary", reference.Summary);
			writer.MaybeWrite("description", reference.Description);
		}
		else
		{
			writer.MaybeWrite("description", value.Description);
			writer.MaybeWriteMap("content", value.Content, options, ApiSerializerContext.Default.MediaType);
			writer.MaybeWrite("required", value.Required);
			writer.WriteExtensions(value.ExtensionData, value.UnknownData);
		}

		writer.WriteEndObject();
	}
}