using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Path;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Provides a fluent interface for <see cref="JsonSchemaBuilder"/>.
/// </summary>
public static class JsonSchemaBuilderExtensions
{
	/// <summary>
	/// Adds a `data` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="data">The collection of keywords and references.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, string> data)
	{
		builder.Add(new DataKeyword(data.ToDictionary(x => x.Key, x => CreateResourceIdentifier(x.Value))));
		return builder;
	}

	/// <summary>
	/// Adds a `data` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="data">The collection of keywords and references.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Data(this JsonSchemaBuilder builder, params (string name, string reference)[] data)
	{
		builder.Add(new DataKeyword(data.ToDictionary(x => x.name, x => CreateResourceIdentifier(x.reference))));
		return builder;
	}

	/// <summary>
	/// Adds a `data` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="data">The collection of keywords and references.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder OptionalData(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, string> data)
	{
		builder.Add(new OptionalDataKeyword(data.ToDictionary(x => x.Key, x => CreateResourceIdentifier(x.Value))));
		return builder;
	}

	/// <summary>
	/// Adds a `data` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="data">The collection of keywords and references.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder OptionalData(this JsonSchemaBuilder builder, params (string name, string reference)[] data)
	{
		builder.Add(new OptionalDataKeyword(data.ToDictionary(x => x.name, x => CreateResourceIdentifier(x.reference))));
		return builder;
	}

	internal static IDataResourceIdentifier CreateResourceIdentifier(string identifier)
	{
		if (identifier[0] != '#' && JsonPointer.TryParse(identifier, out var jp)) return new JsonPointerIdentifier(jp!);
		if (RelativeJsonPointer.TryParse(identifier, out var rjp)) return new RelativeJsonPointerIdentifier(rjp!);
		if (JsonPath.TryParse(identifier, out var path)) return new JsonPathIdentifier(path);
		if (Uri.TryCreate(identifier, UriKind.RelativeOrAbsolute, out var uri)) return new UriIdentifier(uri);

		throw new JsonException($"Cannot identify type of resource identifier for '{identifier}'");
	}
}