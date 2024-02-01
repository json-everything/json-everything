using System;
using System.Diagnostics.CodeAnalysis;
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
	/// Resolves a resource.
	/// </summary>
	/// <param name="evaluation">The evaluation being process.  This will help identify.</param>
	/// <param name="registry">The schema registry.</param>
	/// <param name="value">The value, if <paramref name="evaluation"/> was resolvable.</param>
	/// <returns>True if resolution was successful; false otherwise.</returns>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
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
			data = JsonSerializer.SerializeToNode(rootSchema, JsonSchemaSerializerContext.Default.JsonSchema!);
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
			return node != null!;

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
