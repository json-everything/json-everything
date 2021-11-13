using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Json.Pointer;

namespace Json.Path
{
	internal static class ReferenceHandler
	{
		internal static void Handle(EvaluationContext context)
		{
			if (!context.Options.ExperimentalFeatures.ProcessDataReferences) return;

			foreach (var match in context.Current.ToList())
			{
				if (!IsReference(match.Value, out var reference)) continue;

				var newData = ResolveReference(reference, context.Options).GetAwaiter().GetResult();
				if (newData == null) continue;

				var newMatch = new PathMatch(newData.Value, match.Location);
				var index = context.Current.IndexOf(match);
				context.Current.RemoveAt(index);
				context.Current.Insert(index, newMatch);
			}
		}

		private static bool IsReference(JsonElement element, [NotNullWhen(true)] out Uri? uri)
		{
			uri = null;
			if (element.ValueKind != JsonValueKind.Object) return false;

			var props = element.EnumerateObject().ToList();
			if (props.Count != 1 ||
			    props[0].Name != "$ref" || 
			    props[0].Value.ValueKind != JsonValueKind.String)
				return false;

			var value = props[0].Value.GetString();
			if (!Uri.IsWellFormedUriString(value, UriKind.Absolute)) return false;

			uri = new Uri(value, UriKind.Absolute);
			return true;
		}

		private static async Task<JsonElement?> ResolveReference(Uri uri, PathEvaluationOptions options)
		{
			var fragment = uri.Fragment;
			var baseUri = string.IsNullOrWhiteSpace(fragment)
				? uri
				: new Uri(uri.OriginalString.Replace(fragment, string.Empty));

			var document = await options.ExperimentalFeatures.DataReferenceDownload(baseUri);
			if (document == null) return null;
			if (string.IsNullOrWhiteSpace(fragment)) return document.RootElement;
			if (!JsonPointer.TryParse(fragment, out var pointer)) return null;
			return pointer!.Evaluate(document.RootElement);
		}

		internal static async Task<JsonDocument?> DefaultDownload(Uri uri)
		{
			using var httpClient = new HttpClient();
			try
			{
				return await JsonDocument.ParseAsync(await httpClient.GetStreamAsync(uri));
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
