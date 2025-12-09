using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Json.Schema.OpenApi;

/// <summary>
/// Provides a fluent interface for <see cref="JsonSchemaBuilder"/>.
/// </summary>
public static class JsonSchemaBuilderExtensions
{
	/// <summary>
	/// Adds a `discriminator` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="propertyName">The name of the property in the payload that will hold the discriminator value.</param>
	/// <param name="mapping">An object to hold mappings between payload values and schema names or references.</param>
	/// <param name="extensions">
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Discriminator(this JsonSchemaBuilder builder,
		string propertyName,
		IReadOnlyDictionary<string, string>? mapping = null,
		IReadOnlyDictionary<string, JsonNode?>? extensions = null)
	{
		var obj = new JsonObject
		{
			["propertyName"] = propertyName
		};

		if (mapping is not null)
			obj["mapping"] = new JsonObject(mapping.ToDictionary(x => x.Key, x => (JsonNode?)x.Value));

		if (extensions is not null)
		{
			foreach (var extension in extensions)
				obj[extension.Key] = extension.Value;
		}

		builder.Add("discriminator", obj);
		return builder;
	}

	/// <summary>
	/// Adds an `example` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The example value.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Example(this JsonSchemaBuilder builder,
		JsonNode? value)
	{
		builder.Add("example", value);
		return builder;
	}

	/// <summary>
	/// Adds an `externalDocs` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="url">The URL for the target documentation. This MUST be in the form of a URL.</param>
	/// <param name="description">A description of the target documentation. CommonMark syntax MAY be used for rich text representation.</param>
	/// <param name="extensions">
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder ExternalDocs(this JsonSchemaBuilder builder,
		Uri url,
		string? description = null,
		IReadOnlyDictionary<string, JsonNode?>? extensions = null)
	{
		var obj = new JsonObject
		{
			["url"] = url.OriginalString
		};

		if (description is not null)
			obj["description"] = description;

		if (extensions is not null)
		{
			foreach (var extension in extensions)
				obj[extension.Key] = extension.Value;
		}

		builder.Add("externalDocs", obj);
		return builder;
	}

	/// <summary>
	/// Adds an `xml` keyword to the schema.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="namespace">The URI of the namespace definition. This MUST be in the form of an absolute URI.</param>
	/// <param name="name">Replaces the name of the element/attribute used for the described schema property.
	/// When defined within `items`, it will affect the name of the individual XML elements within the list.
	/// When defined alongside `type` being `array` (outside the `items`), it will affect the wrapping element and
	/// only if `wrapped` is `true`. If `wrapped` is `false`, it will be ignored.</param>
	/// <param name="prefix">The prefix to be used for the name.</param>
	/// <param name="attribute">Declares whether the property definition translates to an attribute instead of an
	/// element. Default value is `false`.</param>
	/// <param name="wrapped">
	/// MAY be used only for an array definition. Signifies whether the array is wrapped (for example, `<books><book/><book/></books>`)
	/// or unwrapped (`<book/><book/>`). Default value is `false`. The definition takes effect only when defined alongside `type`
	/// being `array` (outside the `items`).</param>
	/// <param name="extensions">
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Xml(this JsonSchemaBuilder builder,
		Uri? @namespace = null,
		string? name = null,
		string? prefix = null,
		bool? attribute = null,
		bool? wrapped = null,
		IReadOnlyDictionary<string, JsonNode?>? extensions = null)
	{
		var obj = new JsonObject();

		if (@namespace is not null)
			obj["namespace"] = @namespace.OriginalString;

		if (name is not null)
			obj["name"] = name;

		if (prefix is not null)
			obj["prefix"] = prefix;

		if (attribute.HasValue)
			obj["attribute"] = attribute.Value;

		if (wrapped.HasValue)
			obj["wrapped"] = wrapped.Value;

		if (extensions is not null)
		{
			foreach (var extension in extensions)
				obj[extension.Key] = extension.Value;
		}

		builder.Add("xml", obj);
		return builder;
	}
}