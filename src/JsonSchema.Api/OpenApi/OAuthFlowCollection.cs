using System;
using Json.More;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models the OAuth flow collection.
/// </summary>
[JsonConverter(typeof(OAuthFlowCollectionJsonConverter))]
public class OAuthFlowCollection : IRefTargetContainer
{

	/// <summary>
	/// Gets or sets the implicit flow.
	/// </summary>
	public OAuthFlow? Implicit { get; set; }
	/// <summary>
	/// Gets or sets the password flow.
	/// </summary>
	public OAuthFlow? Password { get; set; }
	/// <summary>
	/// Gets or sets the client-credentials flow.
	/// </summary>
	public OAuthFlow? ClientCredentials { get; set; }
	/// <summary>
	/// Gets or sets the authorization-code flow.
	/// </summary>
	public OAuthFlow? AuthorizationCode { get; set; }
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
			case "implicit":
				target = Implicit;
				break;
			case "password":
				target = Password;
				break;
			case "clientCredentials":
				target = ClientCredentials;
				break;
			case "authorizationCode":
				target = AuthorizationCode;
				break;
		}

		return target != null
			? target.Resolve(keys.Slice(keysConsumed))
			: ExtensionData?.Resolve(keys);
	}
}

internal class OAuthFlowCollectionJsonConverter : JsonConverter<OAuthFlowCollection>
{
	private const string _objectType = "oauth flow collection";

	public override OAuthFlowCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		reader.ExpectObjectStart(_objectType);

		OAuthFlow? implicitFlow = null;
		OAuthFlow? password = null;
		OAuthFlow? clientCredentials = null;
		OAuthFlow? authorizationCode = null;
		ExtensionData? extensionData = null;
		UnknownData? unknownData = null;

		while (reader.ReadPropertyName() is { } propertyName)
		{
			switch (propertyName)
			{
				case "implicit":
					implicitFlow = options.Read(ref reader, ApiSerializerContext.Default.OAuthFlow);
					break;
				case "password":
					password = options.Read(ref reader, ApiSerializerContext.Default.OAuthFlow);
					break;
				case "clientCredentials":
					clientCredentials = options.Read(ref reader, ApiSerializerContext.Default.OAuthFlow);
					break;
				case "authorizationCode":
					authorizationCode = options.Read(ref reader, ApiSerializerContext.Default.OAuthFlow);
					break;
				default:
					reader.ReadUnknown(propertyName, ref extensionData, ref unknownData);
					break;
			}
		}

		if (implicitFlow is not null && implicitFlow.AuthorizationUrl is null)
			throw new JsonException("`authorizationUrl` is required for implicit oauth flow object");
		if (password is not null && password.TokenUrl is null)
			throw new JsonException("`tokenUrl` is required for password oauth flow object");
		if (clientCredentials is not null && clientCredentials.TokenUrl is null)
			throw new JsonException("`tokenUrl` is required for clientCredentials oauth flow object");
		if (authorizationCode is not null)
		{
			if (authorizationCode.AuthorizationUrl is null)
				throw new JsonException("`authorizationUrl` is required for authorizationCode oauth flow object");
			if (authorizationCode.TokenUrl is null)
				throw new JsonException("`tokenUrl` is required for authorizationCode oauth flow object");
		}

		return new OAuthFlowCollection
		{
			Implicit = implicitFlow,
			Password = password,
			ClientCredentials = clientCredentials,
			AuthorizationCode = authorizationCode,
			ExtensionData = extensionData,
			UnknownData = unknownData
		};
	}

	public override void Write(Utf8JsonWriter writer, OAuthFlowCollection value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.MaybeWrite("implicit", value.Implicit, options, ApiSerializerContext.Default.OAuthFlow);
		writer.MaybeWrite("password", value.Password, options, ApiSerializerContext.Default.OAuthFlow);
		writer.MaybeWrite("clientCredentials", value.ClientCredentials, options, ApiSerializerContext.Default.OAuthFlow);
		writer.MaybeWrite("authorizationCode", value.AuthorizationCode, options, ApiSerializerContext.Default.OAuthFlow);
		writer.WriteExtensions(value.ExtensionData, value.UnknownData);

		writer.WriteEndObject();
	}
}
