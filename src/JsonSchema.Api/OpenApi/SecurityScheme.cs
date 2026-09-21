using System;
using Json.More;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a security scheme.
/// </summary>
[JsonConverter(typeof(SecuritySchemeJsonConverter))]
public class SecurityScheme : IRefTargetContainer
{
	/// <summary>
	/// Gets the type of security scheme.
	/// </summary>
	public string Type { get; private protected set; }
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	public string? Name { get; set; }
	/// <summary>
	/// Gets or sets the location of the API key.
	/// </summary>
	public SecuritySchemeLocation? In { get; set; }
	/// <summary>
	/// Gets or sets the scheme.
	/// </summary>
	public string? Scheme { get; set; }
	/// <summary>
	/// Gets or sets the bearer token format.
	/// </summary>
	public string? BearerFormat { get; set; }
	/// <summary>
	/// Gets or sets the collection of OAuth flows.
	/// </summary>
	public OAuthFlowCollection? Flows { get; set; }
	/// <summary>
	/// Gets the OpenID Connect URL.
	/// </summary>
	public Uri? OpenIdConnectUrl { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="SecurityScheme"/>
	/// </summary>
	/// <param name="type">The security scheme type</param>
	public SecurityScheme(string type)
	{
		Type = type;
	}
#pragma warning disable CS8618
	private protected SecurityScheme(){}
#pragma warning restore CS8618

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		if (keys[0] == "flows")
		{
			if (keys.Length == 1) return Flows;
			return Flows?.Resolve(keys.Slice(1));
		}

		return ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		if (this is SecuritySchemeRef ssRef)
			yield return ssRef;
	}
}

/// <summary>
/// Models a `$ref` to a security scheme.
/// </summary>
public class SecuritySchemeRef : SecurityScheme, IComponentRef
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
	/// Creates a new <see cref="SecuritySchemeRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public SecuritySchemeRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	/// <summary>
	/// Creates a new <see cref="SecuritySchemeRef"/>
	/// </summary>
	/// <param name="reference">The reference URI</param>
	public SecuritySchemeRef(string reference)
	{
		Ref = new Uri(reference ?? throw new ArgumentNullException(nameof(reference)), UriKind.RelativeOrAbsolute);
	}

	async Task IComponentRef.Resolve(OpenApiDocument root, JsonSerializerOptions? options)
	{
		void copy(SecurityScheme other)
		{
			Type = other.Type;
			base.Description = other.Description;
			Name = other.Name;
			In = other.In;
			Scheme = other.Scheme;
			BearerFormat = other.BearerFormat;
			Flows = other.Flows;
			OpenIdConnectUrl = other.OpenIdConnectUrl;
			ExtensionData = other.ExtensionData;
		}

		IsResolved = await OpenApi.Ref.Resolve(root, Ref, copy, ApiSerializerContext.Default.SecurityScheme, options);
	}
}

internal class SecuritySchemeJsonConverter : JsonConverter<SecurityScheme>
{
	private const string _objectType = "security scheme";

	public override SecurityScheme Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? reference = null;
		string? summary = null;
		string? type = null;
		string? description = null;
		string? name = null;
		SecuritySchemeLocation? location = null;
		string? scheme = null;
		string? bearerFormat = null;
		OAuthFlowCollection? flows = null;
		Uri? openIdConnectUrl = null;
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
				case "type":
					type = reader.ReadString(propertyName, _objectType);
					break;
				case "description":
					description = reader.ReadString(propertyName, _objectType);
					break;
				case "name":
					name = reader.ReadString(propertyName, _objectType);
					break;
				case "in":
					location = reader.ReadEnum(propertyName, _objectType, options, ApiSerializerContext.Default.SecuritySchemeLocation);
					break;
				case "scheme":
					scheme = reader.ReadString(propertyName, _objectType);
					break;
				case "bearerFormat":
					bearerFormat = reader.ReadString(propertyName, _objectType);
					break;
				case "flows":
					flows = options.Read(ref reader, ApiSerializerContext.Default.OAuthFlowCollection);
					break;
				case "openIdConnectUrl":
					openIdConnectUrl = reader.ReadUri(propertyName, _objectType);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		if (reference is not null)
			return new SecuritySchemeRef(reference)
			{
				Summary = summary,
				Description = description
			};

		return new SecurityScheme(ConverterHelpers.ExpectPresent(type, "type", _objectType))
		{
			Description = description,
			Name = name,
			In = location,
			Scheme = scheme,
			BearerFormat = bearerFormat,
			Flows = flows,
			OpenIdConnectUrl = openIdConnectUrl,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, SecurityScheme value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value is SecuritySchemeRef reference)
		{
			writer.WriteString("$ref", reference.Ref.ToString());
			writer.MaybeWrite("summary", reference.Summary);
			writer.MaybeWrite("description", reference.Description);
		}
		else
		{
			writer.MaybeWrite("type", value.Type);
			writer.MaybeWrite("description", value.Description);
			writer.MaybeWrite("name", value.Name);
			writer.MaybeWriteEnum("in", value.In, options, ApiSerializerContext.Default.SecuritySchemeLocation);
			writer.MaybeWrite("scheme", value.Scheme);
			writer.MaybeWrite("bearerFormat", value.BearerFormat);
			writer.MaybeWrite("flows", value.Flows, options, ApiSerializerContext.Default.OAuthFlowCollection);
			writer.MaybeWrite("openIdConnectUrl", value.OpenIdConnectUrl);
			writer.WriteExtensions(value.ExtensionData, value.UnknownData);
		}

		writer.WriteEndObject();
	}
}