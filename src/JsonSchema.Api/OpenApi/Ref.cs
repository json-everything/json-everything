using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System;
﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.Pointer;
using Yaml2JsonNode;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Allows customization of `$ref` resolutions.
/// </summary>
public static class Ref
{
	/// <summary>
	/// Provides factory methods for creating references to objects in the components collection.
	/// </summary>
	public static class To
	{
		/// <summary>
		/// Creates a reference to a callback.
		/// </summary>
		/// <param name="componentName">The key that identifies the callback.</param>
		/// <returns>The reference.</returns>
		public static CallbackRef Callback(string componentName) => new($"#/components/callbacks/{componentName}");

		/// <summary>
		/// Creates a reference to a example.
		/// </summary>
		/// <param name="componentName">The key that identifies the example.</param>
		/// <returns>The reference.</returns>
		public static ExampleRef Example(string componentName) => new($"#/components/examples/{componentName}");

		/// <summary>
		/// Creates a reference to a header.
		/// </summary>
		/// <param name="componentName">The key that identifies the header.</param>
		/// <returns>The reference.</returns>
		public static HeaderRef Header(string componentName) => new($"#/components/headers/{componentName}");

		/// <summary>
		/// Creates a reference to a link.
		/// </summary>
		/// <param name="componentName">The key that identifies the link.</param>
		/// <returns>The reference.</returns>
		public static LinkRef Link(string componentName) => new($"#/components/links/{componentName}");

		/// <summary>
		/// Creates a reference to a parameter.
		/// </summary>
		/// <param name="componentName">The key that identifies the parameter.</param>
		/// <returns>The reference.</returns>
		public static ParameterRef Parameter(string componentName) => new($"#/components/parameters/{componentName}");

		/// <summary>
		/// Creates a reference to a path item.
		/// </summary>
		/// <param name="componentName">The key that identifies the path item.</param>
		/// <returns>The reference.</returns>
		public static PathItemRef PathItem(string componentName) => new($"#/components/pathItems/{componentName}");

		/// <summary>
		/// Creates a reference to a request body.
		/// </summary>
		/// <param name="componentName">The key that identifies the request body.</param>
		/// <returns>The reference.</returns>
		public static RequestBodyRef RequestBody(string componentName) => new($"#/components/requestBodies/{componentName}");

		/// <summary>
		/// Creates a reference to a response.
		/// </summary>
		/// <param name="componentName">The key that identifies the response.</param>
		/// <returns>The reference.</returns>
		public static ResponseRef Response(string componentName) => new($"#/components/responses/{componentName}");

		/// <summary>
		/// Creates a reference to a schema.
		/// </summary>
		/// <param name="componentName">The key that identifies the schema.</param>
		/// <returns>The reference.</returns>
		public static JsonSchema Schema(string componentName) => new JsonSchemaBuilder().Ref($"#/components/schemas/{componentName}");

		/// <summary>
		/// Creates a reference to a security scheme.
		/// </summary>
		/// <param name="componentName">The key that identifies the security scheme.</param>
		/// <returns>The reference.</returns>
		public static SecuritySchemeRef SecurityScheme(string componentName) => new($"#/components/securitySchemes/{componentName}");
	}

	/// <summary>
	/// Retrieves the document identified by <paramref name="uri"/>.
	/// </summary>
	/// <param name="uri">The resource URI</param>
	/// <returns>The document content, or null if it could not be retrieved.</returns>
	/// <remarks>
	/// The fetched value is the entire external document.  The `$ref`'s URI fragment is
	/// then evaluated against it as a JSON Pointer to locate the referenced component,
	/// which may be any component type.
	/// </remarks>
	public delegate Task<JsonElement?> FetchFunc(Uri uri);

	/// <summary>
	/// Gets or sets the `$ref` fetching function.  Set to null to disable automatic fetching.
	/// </summary>
	public static FetchFunc? Fetch { get; set; } = FetchJson;

	/// <summary>
	/// Defines a default basic fetching function that uses an
	/// <see cref="HttpClient"/> and supports YAML and JSON content.
	/// </summary>
	/// <param name="uri">The resource URI</param>
	/// <returns>The document content</returns>
	public static async Task<JsonElement?> FetchJson(Uri uri)
	{
		// This is inefficient, but it gets the job done.
		using var client = new HttpClient();
		var content = await client.GetStringAsync(uri);
		var yaml = YamlSerializer.Parse(content);
		var json = yaml.First().ToJsonNode();

		if (json is null) return null;

		using var doc = JsonDocument.Parse(json.ToJsonString());

		return doc.RootElement.Clone();
	}

	internal static T? GetFromArray<T>(this IEnumerable<T>? array, string key)
		where T : class
	{
		if (!int.TryParse(key, out var index)) return null;
		return index < 1
			? array?.Reverse().ElementAtOrDefault(-index)
			: array?.ElementAtOrDefault(index);
	}

	internal static TValue? GetFromMap<TKey, TValue>(this Dictionary<TKey, TValue>? map, string key)
		where TKey : IEquatable<string>
		where TValue : class
	{
		return map?.FirstOrDefault(x => x.Key.Equals(key)).Value;
	}

	internal static object? GetFromNode(this JsonNode? node, ReadOnlySpan<string> keys)
	{
		return keys.ToPointer().TryEvaluate(node, out var target)
			? target
			: null;
	}

	internal static JsonPointer ToPointer(this ReadOnlySpan<string> segments)
	{
		if (segments.Length == 0) return JsonPointer.Empty;

		var builder = new StringBuilder();
		foreach (var segment in segments)
		{
			builder.Append('/').Append(segment.Replace("~", "~0").Replace("/", "~1"));
		}

		return JsonPointer.Parse(builder.ToString());
	}

	internal static async Task<bool> Resolve<T>(OpenApiDocument root, Uri targetUri, Action<T> copy, JsonTypeInfo<T> typeInfo, JsonSerializerOptions? options = null)
		where T : class
	{
		var baseUri = ((IBaseDocument)root).BaseUri;
		var newUri = new Uri(baseUri, targetUri);
		var fragment = newUri.Fragment;

		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		if (newBaseUri == baseUri)
		{
			var target = root.Find<T>(JsonPointer.Parse(fragment));
			if (target == null) return false;

			copy(target);
			return true;
		}

		if (Fetch == null)
			throw new RefResolutionException("Automatic fetching of referenced documents has been disabled.");

		var targetBase = await Fetch(newBaseUri) ??
		                 throw new RefResolutionException($"Cannot resolve base document from `{newUri}`");

		if (!JsonPointer.TryParse(fragment, out var pointerFragment))
			throw new RefResolutionException("URI fragments for $ref must be JSON Pointers.");

		var targetContent = pointerFragment.Evaluate(targetBase);
		if (targetContent is null) return false;

		var fetched = targetContent.Value.Deserialize(typeInfo);
		if (fetched is null) return false;

		copy(fetched);
		return true;
	}
}