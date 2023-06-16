using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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
	/// <returns>true if resolution is successful; false otherwise.</returns>
	public async Task<(bool, JsonNode?)> TryResolve(EvaluationContext context)
	{
		var parts = Target.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
		var baseUri = parts[0];
		var fragment = parts.Length > 1 ? parts[1] : null;

		JsonNode? data;
		if (!string.IsNullOrEmpty(baseUri))
		{
			bool wasResolved;
			if (Uri.TryCreate(baseUri, UriKind.Absolute, out var newUri))
				(wasResolved, data) = await Download(newUri);
			else
			{
				var uriFolder = context.Scope.LocalScope.OriginalString.EndsWith("/")
					? context.Scope.LocalScope
					: context.Scope.LocalScope.GetParentUri();
				var newBaseUri = new Uri(uriFolder, baseUri);
				(wasResolved, data) = await Download(newBaseUri);
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

		return (true, data);
	}

	private static async Task<(bool, JsonNode?)> Download(Uri uri)
	{
		if (DataKeyword.ExternalDataRegistry.TryGetValue(uri, out var node))
			// protect against the off-hand that someone registered a null.
			return (node != null, node);

		if (DataKeyword.Fetch == null)
			return (false, null);

		node = await DataKeyword.Fetch(uri);
		return (true, node);
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return Target.ToString();
	}
}