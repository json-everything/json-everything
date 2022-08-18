using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Data;

public class UriIdentifier : IDataResourceIdentifier
{
	public Uri Target { get; }

	public UriIdentifier(Uri target)
	{
		Target = target;
	}

	public bool TryResolve(ValidationContext context, out JsonNode? node)
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
				var uriFolder = context.CurrentUri.OriginalString.EndsWith("/")
					? context.CurrentUri
					: context.CurrentUri.GetParentUri();
				var newBaseUri = new Uri(uriFolder, baseUri);
				wasResolved = Download(newBaseUri, out data);
			}

			if (!wasResolved)
			{
				context.LocalResult.Fail(ErrorMessages.BaseUriResolution, ("uri", baseUri));
				node = null;
				return false;
			}
		}
		else
			data = JsonSerializer.SerializeToNode(context.SchemaRoot);

		if (!string.IsNullOrEmpty(fragment))
		{
			fragment = $"#{fragment}";
			if (!JsonPointer.TryParse(fragment, out var pointer))
			{
				context.LocalResult.Fail(ErrorMessages.PointerParse, ("fragment", fragment));
				node = null;
				return false;
			}

			if (!pointer!.TryEvaluate(data, out var resolved))
			{
				context.LocalResult.Fail(ErrorMessages.RefResolution, ("uri", fragment));
				node = null;
				return false;
			}
			data = resolved;
		}

		node = data;
		return true;
	}

	private static bool Download(Uri uri, out JsonNode? node)
	{
		if (DataKeyword.Fetch == null)
		{
			node = null;
			return false;
		}

		node = DataKeyword.Fetch(uri);
		return true;
	}

	public override string ToString()
	{
		return Target.ToString();
	}
}