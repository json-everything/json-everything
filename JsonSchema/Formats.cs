using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Json.Schema
{
	public static class Formats
	{
		private static readonly ConcurrentDictionary<string, Format> _registry;

		public static readonly Format Date = new Format("date");
		public static readonly Format DateTime = new Format("date-time");
		public static readonly Format Duration = new Format("duration");
		public static readonly Format Email = new Format("email");
		public static readonly Format Hostname = new Format("hostname");
		public static readonly Format IdnEmail = new Format("idn-email");
		public static readonly Format IdnHostname = new Format("idn-hostname");
		public static readonly Format Ipv4 = new Format("ipv4");
		public static readonly Format Ipv6 = new Format("ipv6");
		public static readonly Format Iri = new Format("iri");
		public static readonly Format IriReference = new Format("iri-reference");
		public static readonly Format JsonPointer = new Format("json-pointer");
		public static readonly Format Regex = new Format("regex");
		public static readonly Format RelativeJsonPointer = new Format("relative-json-pointer");
		public static readonly Format Time = new Format("time");
		public static readonly Format Uri = new Format("uri");
		public static readonly Format UriReference = new Format("uri-reference");
		public static readonly Format UriTemplate = new Format("uri-template");
		public static readonly Format Uuid = new Format("uuid");

		public static readonly Format Unknown = new Format();

		static Formats()
		{
			_registry = new ConcurrentDictionary<string, Format>(
				typeof(Formats)
					.GetFields(BindingFlags.Static | BindingFlags.Public)
					.Select(f => (Format) f.GetValue(null))
					.Where(f => !ReferenceEquals(f, Unknown))
					.ToDictionary(f => f.Key));
		}

		public static Format Get(string key)
		{
			return _registry.TryGetValue(key, out var format) ? format : Unknown;
		}

		public static void Register(Format format)
		{
			if (format == null)
				throw new ArgumentNullException(nameof(format));

			_registry[format.Key] = format;
		}
	}
}