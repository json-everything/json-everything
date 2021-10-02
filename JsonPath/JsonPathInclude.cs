using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Json.Pointer;

namespace Json.Path
{
	public static class JsonPathInclude
	{
		public static ConcurrentDictionary<string, Lazy<JsonDocument>> Includes = new ConcurrentDictionary<string, Lazy<JsonDocument>>();

		public static Dictionary<Func<PathMatch, bool>, Func<PathMatch, int, List<PathMatch>?>> Resolvers = new Dictionary<Func<PathMatch, bool>, Func<PathMatch, int, List<PathMatch>?>>();
		public static Dictionary<Func<object, bool>, Func<object, JsonDocument>> Retrievers = new Dictionary<Func<object, bool>, Func<object, JsonDocument>>();

		public static bool HasProperty(this JsonElement jsonElement, string propertyName)
		{
			JsonElement elm;
			if (jsonElement.TryGetProperty("ref$", out elm)) return true;
			else return false;
		}

		public static List<PathMatch>? ResolveIncludes(PathMatch match, int idx = 0)
		{
			JsonElement elm;
			List<PathMatch>? matches = new List<PathMatch>();

			foreach (var (pred, rslvr) in Resolvers)
			{
				if (pred(match)) return rslvr(match, idx);
			}
			matches.Add(match);
			return matches;
		}

		public static JsonDocument Retrieve(object location)
		{
			foreach (var (pred, rtrvr) in Retrievers)
			{
				if (pred(location)) return rtrvr(location);
			}
			return JsonDocument.Parse("{}");
		}

		public static List<PathMatch>? StringRootResolver(PathMatch match, int idx = 0)
		{
			JsonDocument remoteInstance = RetrieveAndCache(match.Value.GetString()[2..]);
			return RootResolver(remoteInstance, match.Location, idx);
		}

		public static List<PathMatch>? StringJsonPointerResolver(PathMatch match, int idx = 0)
		{
			string[] parts = match.Value.GetString()[2..].Split("#");
			JsonDocument remoteInstance = RetrieveAndCache(parts[0]);
			if (parts.Length == 1 || ((parts.Length == 2) && string.IsNullOrWhiteSpace(parts[1])))
			{
				return RootResolver(remoteInstance, match.Location, idx);
			}
			else if (parts.Length == 2)
			{
				var ptr = JsonPointer.Parse(parts[1]);
				return JsonPointerResolver(remoteInstance, ptr, match.Location, idx);
			}
			return null;
		}

		public static List<PathMatch>? StringJsonPathResolver(PathMatch match, int idx = 0)
		{
			string[] parts = match.Value.GetString()[2..].Split("#$");
			JsonDocument remoteInstance = RetrieveAndCache(parts[0]);
			if (parts.Length == 1 || ((parts.Length == 2) && string.IsNullOrWhiteSpace(parts[1])))
			{
				return RootResolver(remoteInstance, match.Location, idx);
			}
			else if (parts.Length == 2)
			{
				var path = JsonPath.Parse("$" + parts[1]);
				return JsonPathResolver(remoteInstance, path, match.Location, idx);
			}
			return null;
		}

		public static List<PathMatch>? RefRootResolver(PathMatch match, int idx = 0)
		{
			JsonDocument remoteInstance = RetrieveAndCache(match.Value.GetProperty("ref$").GetString());
			return RootResolver(remoteInstance, match.Location, idx);
		}

		public static List<PathMatch>? RefJsonPointerResolver(PathMatch match, int idx = 0)
		{
			string[] parts = match.Value.GetProperty("ref$").GetString().Split("#");
			JsonDocument remoteInstance = RetrieveAndCache(parts[0]);
			if (parts.Length == 1 || ((parts.Length == 2) && string.IsNullOrWhiteSpace(parts[1])))
			{
				return RootResolver(remoteInstance, match.Location, idx);
			}
			else if (parts.Length == 2)
			{
				var ptr = JsonPointer.Parse(parts[1]);
				return JsonPointerResolver(remoteInstance, ptr, match.Location, idx);
			}
			return null;
		}

		public static List<PathMatch>? RefJsonPathResolver(PathMatch match, int idx = 0)
		{
			string[] parts = match.Value.GetProperty("ref$").GetString().Split("#$");
			JsonDocument remoteInstance = RetrieveAndCache(parts[0]);
			if (parts.Length == 1 || ((parts.Length == 2) && string.IsNullOrWhiteSpace(parts[1])))
			{
				return RootResolver(remoteInstance, match.Location, idx);
			}
			else if (parts.Length == 2)
			{
				var path = JsonPath.Parse("$" + parts[1]);
				return JsonPathResolver(remoteInstance, path, match.Location, idx);
			}
			return null;
		}

		public static List<PathMatch>? RootResolver(JsonDocument remoteInstance, JsonPointer matchLocation, int idx = 0)
		{
			List<PathMatch>? matches = new List<PathMatch>();
			matches.Add(new PathMatch(remoteInstance.RootElement, matchLocation));
			return matches;
		}

		public static List<PathMatch>? JsonPointerResolver(JsonDocument remoteInstance, JsonPointer jsonPointer, JsonPointer matchLocation, int idx = 0)
		{
			List<PathMatch>? matches = new List<PathMatch>();

			if (jsonPointer == null)
			{
				matches.Add(new PathMatch(remoteInstance.RootElement, matchLocation));
			}
			else
			{
				var root = jsonPointer.Evaluate(remoteInstance.RootElement);
				matches.Add(new PathMatch(root.Value, matchLocation));
			}

			return matches;
		}

		public static List<PathMatch>? JsonPathResolver(JsonDocument remoteInstance, JsonPath jsonPath, JsonPointer matchLocation, int idx = 0)
		{
			List<PathMatch>? matches = new List<PathMatch>();
			
			if (jsonPath == null)
			{
				matches.Add(new PathMatch(remoteInstance.RootElement, matchLocation));
			}
			else
			{
				var results = jsonPath.Evaluate(remoteInstance.RootElement);
				foreach (var pm in results.Matches.ToList())
				{
					var pmNew = new PathMatch(pm.Value, matchLocation.Combine(PointerSegment.Create(idx.ToString())));
					List<PathMatch>? rincludes = ResolveIncludes(pmNew, idx);
					if (rincludes != null)
					{
						matches.AddRange(rincludes);
						idx += rincludes?.Count ?? 0;
					}
					else
					{
						matches.Add(new PathMatch(pmNew.Value, matchLocation.Combine(PointerSegment.Create(idx.ToString()))));
						idx++;
					}
				}
			}

			return matches;
		}

		public static JsonDocument RetrieveAndCache(object location, bool update = false)
		{
			Lazy<JsonDocument> valueFactory(string __) => new Lazy<JsonDocument>(() => Retrieve(location));
			return Includes.AddOrUpdate((string)location, valueFactory, (__, lazy) => update ? valueFactory(__) : lazy).Value;
		}

		public static JsonDocument UrlRetriever(object location)
		{
			string url = (string) location;
			using var httpClient = new HttpClient();
			return JsonDocument.Parse(httpClient.GetStreamAsync(url).GetAwaiter().GetResult());
		}
	}
}
