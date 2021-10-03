using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Json.Pointer;

namespace Json.Path
{
	public static class ReferenceHandler
	{
		//private readonly ConcurrentDictionary<string, Lazy<JsonDocument>> _includes;

		//public static Dictionary<Func<PathMatch, bool>, Func<PathMatch, int, List<PathMatch>?>> Resolvers = new Dictionary<Func<PathMatch, bool>, Func<PathMatch, int, List<PathMatch>?>>();
		//public static Dictionary<Func<object, bool>, Func<object, JsonDocument>> Retrievers = new Dictionary<Func<object, bool>, Func<object, JsonDocument>>();

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
			var baseUri = new Uri(uri.OriginalString.Replace(fragment, string.Empty));

			var document = await options.ExperimentalFeatures.DataReferenceDownload(baseUri);
			if (document == null) return null;
			if (string.IsNullOrWhiteSpace(fragment)) return document.RootElement;
			if (!JsonPointer.TryParse(fragment, out var pointer)) return null;
			return pointer.Evaluate(document.RootElement);
		}

		//public static List<PathMatch>? ResolveIncludes(PathMatch match, int idx = 0)
		//{
		//	JsonElement elm;
		//	List<PathMatch>? matches = new List<PathMatch>();

		//	foreach (var (pred, rslvr) in Resolvers)
		//	{
		//		if (pred(match)) return rslvr(match, idx);
		//	}
		//	matches.Add(match);
		//	return matches;
		//}

		//public static JsonDocument Retrieve(object location)
		//{
		//	foreach (var (pred, rtrvr) in Retrievers)
		//	{
		//		if (pred(location)) return rtrvr(location);
		//	}
		//	return JsonDocument.Parse("{}");
		//}

		//public static List<PathMatch>? StringRootResolver(PathMatch match, int idx = 0)
		//{
		//	JsonDocument remoteInstance = RetrieveAndCache(match.Value.GetString()[2..]);
		//	return RootResolver(remoteInstance, match.Location, idx);
		//}

		//public static List<PathMatch>? StringJsonPointerResolver(PathMatch match, int idx = 0)
		//{
		//	string[] parts = match.Value.GetString()[2..].Split("#");
		//	JsonDocument remoteInstance = RetrieveAndCache(parts[0]);
		//	if (parts.Length == 1 || ((parts.Length == 2) && string.IsNullOrWhiteSpace(parts[1])))
		//	{
		//		return RootResolver(remoteInstance, match.Location, idx);
		//	}
		//	else if (parts.Length == 2)
		//	{
		//		var ptr = JsonPointer.Parse(parts[1]);
		//		return JsonPointerResolver(remoteInstance, ptr, match.Location, idx);
		//	}
		//	return null;
		//}

		//public static List<PathMatch>? StringJsonPathResolver(PathMatch match, int idx = 0)
		//{
		//	string[] parts = match.Value.GetString()[2..].Split("#$");
		//	JsonDocument remoteInstance = RetrieveAndCache(parts[0]);
		//	if (parts.Length == 1 || ((parts.Length == 2) && string.IsNullOrWhiteSpace(parts[1])))
		//	{
		//		return RootResolver(remoteInstance, match.Location, idx);
		//	}
		//	else if (parts.Length == 2)
		//	{
		//		var path = JsonPath.Parse("$" + parts[1]);
		//		return JsonPathResolver(remoteInstance, path, match.Location, idx);
		//	}
		//	return null;
		//}

		//public static List<PathMatch>? RefRootResolver(PathMatch match, int idx = 0)
		//{
		//	JsonDocument remoteInstance = RetrieveAndCache(match.Value.GetProperty("ref$").GetString());
		//	return RootResolver(remoteInstance, match.Location, idx);
		//}

		//public static List<PathMatch>? RefJsonPointerResolver(PathMatch match, int idx = 0)
		//{
		//	string[] parts = match.Value.GetProperty("ref$").GetString().Split("#");
		//	JsonDocument remoteInstance = RetrieveAndCache(parts[0]);
		//	if (parts.Length == 1 || ((parts.Length == 2) && string.IsNullOrWhiteSpace(parts[1])))
		//	{
		//		return RootResolver(remoteInstance, match.Location, idx);
		//	}
		//	else if (parts.Length == 2)
		//	{
		//		var ptr = JsonPointer.Parse(parts[1]);
		//		return JsonPointerResolver(remoteInstance, ptr, match.Location, idx);
		//	}
		//	return null;
		//}

		//public static List<PathMatch>? RefJsonPathResolver(PathMatch match, int idx = 0)
		//{
		//	string[] parts = match.Value.GetProperty("ref$").GetString().Split("#$");
		//	JsonDocument remoteInstance = RetrieveAndCache(parts[0]);
		//	if (parts.Length == 1 || ((parts.Length == 2) && string.IsNullOrWhiteSpace(parts[1])))
		//	{
		//		return RootResolver(remoteInstance, match.Location, idx);
		//	}
		//	else if (parts.Length == 2)
		//	{
		//		var path = JsonPath.Parse("$" + parts[1]);
		//		return JsonPathResolver(remoteInstance, path, match.Location, idx);
		//	}
		//	return null;
		//}

		//public static List<PathMatch>? RootResolver(JsonDocument remoteInstance, JsonPointer matchLocation, int idx = 0)
		//{
		//	List<PathMatch>? matches = new List<PathMatch>();
		//	matches.Add(new PathMatch(remoteInstance.RootElement, matchLocation));
		//	return matches;
		//}

		//public static List<PathMatch>? JsonPointerResolver(JsonDocument remoteInstance, JsonPointer jsonPointer, JsonPointer matchLocation, int idx = 0)
		//{
		//	List<PathMatch>? matches = new List<PathMatch>();

		//	if (jsonPointer == null)
		//	{
		//		matches.Add(new PathMatch(remoteInstance.RootElement, matchLocation));
		//	}
		//	else
		//	{
		//		var root = jsonPointer.Evaluate(remoteInstance.RootElement);
		//		matches.Add(new PathMatch(root.Value, matchLocation));
		//	}

		//	return matches;
		//}

		//public static List<PathMatch>? JsonPathResolver(JsonDocument remoteInstance, JsonPath jsonPath, JsonPointer matchLocation, int idx = 0)
		//{
		//	List<PathMatch>? matches = new List<PathMatch>();
			
		//	if (jsonPath == null)
		//	{
		//		matches.Add(new PathMatch(remoteInstance.RootElement, matchLocation));
		//	}
		//	else
		//	{
		//		var results = jsonPath.Evaluate(remoteInstance.RootElement);
		//		foreach (var pm in results.Matches.ToList())
		//		{
		//			var pmNew = new PathMatch(pm.Value, matchLocation.Combine(PointerSegment.Create(idx.ToString())));
		//			List<PathMatch>? rincludes = ResolveIncludes(pmNew, idx);
		//			if (rincludes != null)
		//			{
		//				matches.AddRange(rincludes);
		//				idx += rincludes?.Count ?? 0;
		//			}
		//			else
		//			{
		//				matches.Add(new PathMatch(pmNew.Value, matchLocation.Combine(PointerSegment.Create(idx.ToString()))));
		//				idx++;
		//			}
		//		}
		//	}

		//	return matches;
		//}

		//public static JsonDocument RetrieveAndCache(object location, bool update = false)
		//{
		//	Lazy<JsonDocument> valueFactory(string __) => new Lazy<JsonDocument>(() => Retrieve(location));
		//	return _includes.AddOrUpdate((string)location, valueFactory, (__, lazy) => update ? valueFactory(__) : lazy).Value;
		//}

		internal static async Task<JsonDocument?> DefaultDownload(Uri uri)
		{
			using var httpClient = new HttpClient();
			try
			{
				return await JsonDocument.ParseAsync(await httpClient.GetStreamAsync(uri));
			}
			catch (Exception e)
			{
				return null;
			}
		}
	}
}
