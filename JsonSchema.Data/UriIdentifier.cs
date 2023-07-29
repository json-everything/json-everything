using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Handles data references that are URIs.
/// </summary>
public class UriIdentifier : IDataResourceIdentifier
{
	/// <summary>
	/// The URI target.
	/// </summary>
	public Uri Target { get; }

	/// <summary>
	/// Creates a new instance of <see cref="UriIdentifier"/>.
	/// </summary>
	/// <param name="target">The target.</param>
	public UriIdentifier(Uri target)
	{
		Target = target;
	}

	/// <summary>
	/// Attempts to resolve the reference.
	/// </summary>
	/// <param name="context">The schema evaluation context.</param>
	/// <param name="value">If return is true, the value at the indicated location.</param>
	/// <returns>true if resolution is successful; false otherwise.</returns>
	public bool TryResolve(EvaluationContext context, out JsonNode? value)
	{
		var parts = Target.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
		var baseUri = parts[0];
		var fragment = parts.Length > 1 ? parts[1] : null;

		JsonNode? data;
		if (!string.IsNullOrEmpty(baseUri))
		{
			bool wasResolved;
			if (Uri.TryCreate(baseUri, UriKind.Absolute, out var newUri))
				wasResolved = Download(newUri, out data);
			else
			{
				var uriFolder = context.Scope.LocalScope.OriginalString.EndsWith("/")
					? context.Scope.LocalScope
					: context.Scope.LocalScope.GetParentUri();
				var newBaseUri = new Uri(uriFolder, baseUri);
				wasResolved = Download(newBaseUri, out data);
			}

			if (!wasResolved)
				throw new JsonSchemaException($"Cannot resolve value at `{Target}`");
		}
		else
			data = JsonSerializer.SerializeToNode(context.SchemaRoot);

		if (!string.IsNullOrEmpty(fragment))
		{
			fragment = $"#{fragment}";
			if (!JsonPointer.TryParse(fragment, out var pointer))
				throw new JsonSchemaException($"Unrecognized fragment type `{Target}`");

			if (!pointer!.TryEvaluate(data, out var resolved))
				throw new JsonSchemaException($"Cannot resolve value at `{Target}`");
			data = resolved;
		}

		value = data;
		return true;
	}

	public bool TryResolve(KeywordEvaluation evaluation, SchemaRegistry registry, out JsonNode? value)
	{
		var parts = Target.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
		var baseUri = parts[0];
		var fragment = parts.Length > 1 ? parts[1] : null;

		JsonNode? data;
		if (!string.IsNullOrEmpty(baseUri))
		{
			bool wasResolved;
			if (Uri.TryCreate(baseUri, UriKind.Absolute, out var newUri))
				wasResolved = Download(newUri, out data);
			else
			{
				var localScope = new Uri(evaluation.Results.SchemaLocation.OriginalString.Split(new[] { '#' }, StringSplitOptions.None)[0]);
				var uriFolder = localScope.OriginalString.EndsWith("/")
					? localScope
					: localScope.GetParentUri();
				var newBaseUri = new Uri(uriFolder, baseUri);
				wasResolved = Download(newBaseUri, out data);
			}

			if (!wasResolved)
				throw new JsonSchemaException($"Cannot resolve value at `{Target}`");
		}
		else
		{
			var root = evaluation.Results;
			while (root.Parent != null)
			{
				root = root.Parent;
			}

			var rootSchema = (JsonSchema?) registry.Get(root.SchemaLocation);
			data = JsonSerializer.SerializeToNode(rootSchema);
		}

		if (!string.IsNullOrEmpty(fragment))
		{
			fragment = $"#{fragment}";
			if (!JsonPointer.TryParse(fragment, out var pointer))
				throw new JsonSchemaException($"Unrecognized fragment type `{Target}`");

			if (!pointer!.TryEvaluate(data, out var resolved))
				throw new JsonSchemaException($"Cannot resolve value at `{Target}`");
			data = resolved;
		}

		value = data;
		return true;
	}

	private static bool Download(Uri uri, out JsonNode? node)
	{
		if (DataKeyword.ExternalDataRegistry.TryGetValue(uri, out node))
			// protect against the off-hand that someone registered a null.
			return node != null;

		if (DataKeyword.Fetch == null)
		{
			node = null;
			return false;
		}

		node = DataKeyword.Fetch(uri);
		return true;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}