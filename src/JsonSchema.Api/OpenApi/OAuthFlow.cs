using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models an OAuth flow.
/// </summary>
[JsonConverter(typeof(OAuthFlowJsonConverter))]
public class OAuthFlow : IRefTargetContainer
{
	/// <summary>
	/// Gets the authorization URL.
	/// </summary>
	public Uri? AuthorizationUrl { get; set; }
	/// <summary>
	/// Gets the token URL.
	/// </summary>
	public Uri? TokenUrl { get; set; }
	/// <summary>
	/// Gets or sets the refresh token URL.
	/// </summary>
	public Uri? RefreshUrl { get; set; }
	/// <summary>
	/// Gets the scopes.
	/// </summary>
	public Dictionary<string, string> Scopes { get; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }
	/// <summary>
	/// Gets or sets properties which are not recognized by OpenAPI.
	/// </summary>
	public UnknownData? UnknownData { get; set; }

	/// <summary>
	/// Creates a new <see cref="OAuthFlow"/>
	/// </summary>
	/// <param name="scopes">The scopes</param>
	public OAuthFlow(Dictionary<string, string> scopes)
	{
		Scopes = scopes;
	}

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		return ExtensionData?.Resolve(keys);
	}
}

internal class OAuthFlowJsonConverter : JsonConverter<OAuthFlow>
{
	private const string _objectType = "oauth flow";

	public override OAuthFlow Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		Uri? authorizationUrl = null;
		Uri? tokenUrl = null;
		Uri? refreshUrl = null;
		Dictionary<string, string>? scopes = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "authorizationUrl":
					authorizationUrl = reader.ReadUri(propertyName, _objectType);
					break;
				case "tokenUrl":
					tokenUrl = reader.ReadUri(propertyName, _objectType);
					break;
				case "refreshUrl":
					refreshUrl = reader.ReadUri(propertyName, _objectType);
					break;
				case "scopes":
					scopes = reader.ReadStringMap(propertyName, _objectType, options);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		return new OAuthFlow(ConverterHelpers.ExpectPresent(scopes, "scopes", _objectType))
		{
			AuthorizationUrl = authorizationUrl,
			TokenUrl = tokenUrl,
			RefreshUrl = refreshUrl,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, OAuthFlow value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.MaybeWrite("authorizationUrl", value.AuthorizationUrl);
		writer.MaybeWrite("tokenUrl", value.TokenUrl);
		writer.MaybeWrite("refreshUrl", value.RefreshUrl);
		writer.WriteStringMap("scopes", value.Scopes, options);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
