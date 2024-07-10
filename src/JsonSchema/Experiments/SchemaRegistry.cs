using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;

namespace Json.Schema.Experiments;

public class SchemaRegistry
{
	private class Registration
	{
		public required JsonObject Root { get; init; }
		public Dictionary<string, JsonObject> Anchors { get; } = [];
		public Dictionary<string, JsonObject> LegacyAnchors { get; } = [];
		public Dictionary<string, JsonObject> DynamicAnchors { get; } = [];
		public JsonObject? RecursiveAnchor { get; set; }
	}

	internal static readonly Regex AnchorPattern201909 = new("^[A-Za-z][-A-Za-z0-9.:_]*$");
	internal static readonly Regex AnchorPattern202012 = new("^[A-Za-z_][-A-Za-z0-9._]*$");

	private readonly Dictionary<Uri, Registration> _registry;
	private readonly Dictionary<JsonObject, Uri> _reverseLookup;

	public static SchemaRegistry Global { get; } = new();

	internal SchemaRegistry()
	{
		_registry = [];
		_reverseLookup = [];
	}

	public Uri Register(JsonObject schema)
	{
		var idText = (schema["$id"] as JsonValue)?.GetString();

		var id = idText is null ? GenerateId() : new Uri(JsonSchema.DefaultBaseUri, idText);

		Register(id, schema);

		return id;
	}

	public void Register(Uri baseUri, JsonObject schema)
	{
		var registrations = Scan(baseUri, schema);

		foreach (var registration in registrations)
		{
			_registry[registration.Key] = registration.Value;
			_reverseLookup[registration.Value.Root] = registration.Key;
		}

		if (!_registry.ContainsKey(baseUri))
		{
			// schema contains different $id
			var registration = _registry.First(x => ReferenceEquals(x.Value.Root, schema)).Value;
			_registry[baseUri] = registration;
		}
	}

	internal JsonObject Get(Uri baseUri, string? anchor = null, bool allowLegacy = false)
	{
		return GetAnchor(baseUri, anchor, false, allowLegacy) ?? throw new RefResolutionException(baseUri, anchor);
	}

	internal Uri? GetUri(JsonObject schema)
	{
		return _reverseLookup.GetValueOrDefault(schema) ??
		       Global._reverseLookup.GetValueOrDefault(schema);
	}

	internal (JsonObject, Uri) Get(DynamicScope scope, Uri baseUri, string anchor, bool requireLocalAnchor)
	{
		if (requireLocalAnchor)
		{
			var registration = _registry.GetValueOrDefault(baseUri) ?? Global._registry.GetValueOrDefault(baseUri);
			if (registration == null)
				throw new InvalidOperationException($"Could not find '{baseUri}'. This shouldn't happen.");
			if (!registration.DynamicAnchors.ContainsKey(anchor))
			{
				var target = GetAnchor(baseUri, anchor, false, false) ?? throw new RefResolutionException(baseUri, anchor);
				return (target, baseUri);
			}
		}

		foreach (var uri in scope.Reverse())
		{
			var target = GetAnchor(uri, anchor, true, false);
			if (target is not null) return (target, uri);
		}

		throw new RefResolutionException(scope.LocalScope, anchor, true);
	}

	internal (JsonObject, Uri) GetRecursive(DynamicScope scope)
	{
		(JsonObject, Uri)? resolved = null;
		foreach (var uri in scope)
		{
			var registration = _registry.GetValueOrDefault(uri) ?? Global._registry.GetValueOrDefault(uri);
			if (registration == null)
				throw new InvalidOperationException($"Could not find '{uri}'. This shouldn't happen.");
			if (registration.RecursiveAnchor is null)
				return resolved ?? (registration.Root, uri);

			resolved = (registration.RecursiveAnchor, uri);
		}

		// resolved should always be set
		return resolved ?? throw new NotImplementedException();
	}

	private JsonObject? GetAnchor(Uri baseUri, string? anchor, bool isDynamic, bool allowLegacy)
	{
		return GetAnchorFromRegistry(_registry, baseUri, anchor, isDynamic, allowLegacy) ??
		       GetAnchorFromRegistry(Global._registry, baseUri, anchor, isDynamic, allowLegacy);
	}

	private static JsonObject? GetAnchorFromRegistry(Dictionary<Uri, Registration> registry, Uri baseUri, string? anchor, bool isDynamic, bool allowLegacy)
	{
		if (!registry.TryGetValue(baseUri, out var registration)) return null;

		if (anchor is null) return registration.Root;

		var anchorList = isDynamic ? registration.DynamicAnchors : registration.Anchors;

		return anchorList.GetValueOrDefault(anchor) ?? (allowLegacy ? registration.LegacyAnchors.GetValueOrDefault(anchor) : null);
	}

	private static Uri GenerateId() => new(JsonSchema.DefaultBaseUri, Guid.NewGuid().ToString("N")[..10]);

	private Dictionary<Uri, Registration> Scan(Uri baseUri, JsonObject schema)
	{
		var toCheck = new Queue<(Uri, JsonObject)>();
		toCheck.Enqueue((baseUri, schema));

		var registrations = new Dictionary<Uri, Registration>();

		while (toCheck.Any())
		{
			var (currentUri, currentSchema) = toCheck.Dequeue();

			_reverseLookup[currentSchema] = currentUri;

			var idText = (currentSchema["$id"] as JsonValue)?.GetString();
			if (idText is not null) currentUri = new Uri(currentUri, idText);

			if (!registrations.TryGetValue(currentUri, out var registration))
				registrations[currentUri] = registration = new Registration
				{
					Root = currentSchema
				};

			if (!string.IsNullOrEmpty(idText) && idText[0] == '#' && AnchorPattern201909.IsMatch(idText[1..])) 
				registration.LegacyAnchors[idText[1..]] = currentSchema;

			var dynamicAnchor = (currentSchema["$dynamicAnchor"] as JsonValue)?.GetString();
			if (dynamicAnchor is not null)
			{
				registration.Anchors[dynamicAnchor] = currentSchema;
				registration.DynamicAnchors[dynamicAnchor] = currentSchema;
			}

			var recursiveAnchor = (currentSchema["$recursiveAnchor"] as JsonValue)?.GetBool() == true;
			if (recursiveAnchor) 
				registration.RecursiveAnchor = currentSchema;

			var anchor = (currentSchema["$anchor"] as JsonValue)?.GetString();
			if (anchor is not null)
				registration.Anchors[anchor] = currentSchema;

			foreach (var kvp in currentSchema)
			{
				var handler = KeywordRegistry.Get(kvp.Key);
				if (handler is null) continue;

				foreach (var subschema in handler.GetSubschemas(kvp.Value))
				{
					if (subschema is not JsonObject objSubschema) continue;

					toCheck.Enqueue((currentUri, objSubschema));
				}
			}
		}

		return registrations;
	}
}
