using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema
{
	public static class Formats
	{
		private static readonly ConcurrentDictionary<string, Format> _registry;
		private static readonly string[] _dateTimeFormats =
		{
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ssK",
			"yyyy'-'MM'-'dd'T'HH':'mm':'ss"
		};
		private static readonly string[] _timeFormats =
		{
			"'HH':'mm':'ss'.'fffffffK",
			"'HH':'mm':'ss'.'ffffffK",
			"'HH':'mm':'ss'.'fffffK",
			"'HH':'mm':'ss'.'ffffK",
			"'HH':'mm':'ss'.'fffK",
			"'HH':'mm':'ss'.'ffK",
			"'HH':'mm':'ss'.'fK",
			"'HH':'mm':'ssK",
			"'HH':'mm':'ss"
		};

		public static readonly Format Date = new PredicateFormat("date", CheckDate);
		public static readonly Format DateTime = new PredicateFormat("date-time", CheckDateTime);
		public static readonly Format Duration = new PredicateFormat("duration", CheckDuration);
		public static readonly Format Email = new PredicateFormat("email", CheckEmail);
		public static readonly Format Hostname = new PredicateFormat("hostname", CheckHostName);
		public static readonly Format IdnEmail = new PredicateFormat("idn-email", CheckEmail);
		public static readonly Format IdnHostname = new PredicateFormat("idn-hostname", CheckHostName);
		public static readonly Format Ipv4 = new PredicateFormat("ipv4", CheckIpv4);
		public static readonly Format Ipv6 = new PredicateFormat("ipv6", CheckIpv6);
		public static readonly Format Iri = new PredicateFormat("iri", CheckUriFormatting);
		public static readonly Format IriReference = new PredicateFormat("iri-reference", CheckUriFormatting);
		public static readonly Format JsonPointer = new PredicateFormat("json-pointer", CheckJsonPointer);
		public static readonly Format Regex = new Format("regex");
		public static readonly Format RelativeJsonPointer = new PredicateFormat("relative-json-pointer", CheckRelativeJsonPointer);
		public static readonly Format Time = new PredicateFormat("time", CheckTime);
		public static readonly Format Uri = new PredicateFormat("uri", CheckUriFormatting);
		public static readonly Format UriReference = new PredicateFormat("uri-reference", CheckUriFormatting);
		public static readonly Format UriTemplate = new PredicateFormat("uri-template", CheckUriFormatting);
		public static readonly Format Uuid = new PredicateFormat("uuid", CheckUuid);

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

		private static bool CheckUriFormatting(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			return System.Uri.TryCreate(element.GetString(), UriKind.RelativeOrAbsolute, out _);
		}

		private static bool CheckJsonPointer(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			return Pointer.JsonPointer.TryParse(element.GetString(), out _);
		}

		private static bool CheckRelativeJsonPointer(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			return Pointer.RelativeJsonPointer.TryParse(element.GetString(), out _);
		}

		// source: https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
		private static bool CheckEmail(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			var email = element.GetString();

			if (string.IsNullOrWhiteSpace(email)) return false;

			try
			{
				// Normalize the domain
				email = System.Text.RegularExpressions.Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
			catch (ArgumentException)
			{
				return false;
			}

			try
			{
				return System.Text.RegularExpressions.Regex.IsMatch(email,
					"^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$",
					RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}

		// source: part of above
		// Examines the domain part of the email and normalizes it.
		private static string DomainMapper(Match match)
		{
			// Use IdnMapping class to convert Unicode domain names.
			var idn = new IdnMapping();

			// Pull out and process domain name (throws ArgumentException on invalid)
			var domainName = idn.GetAscii(match.Groups[2].Value);

			return match.Groups[1].Value + domainName;
		}

		private static bool CheckUuid(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			return Guid.TryParseExact(element.GetString(), "D", out _);
		}

		private static bool CheckDate(JsonElement element)
		{
			return CheckDateFormat(element, "yyyy-MM-dd");
		}

		private static bool CheckTime(JsonElement element)
		{
			return CheckDateFormat(element, _timeFormats);
		}

		private static bool CheckDateTime(JsonElement element)
		{
			return CheckDateFormat(element, _dateTimeFormats);
		}

		private static bool CheckDateFormat(JsonElement element, params string[] formats)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			return System.DateTime.TryParseExact(element.GetString().ToUpperInvariant(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
		}

		private static bool CheckHostName(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			return System.Uri.CheckHostName(element.GetString()) != UriHostNameType.Unknown;
		}

		private static bool CheckIpv4(JsonElement element)
		{
			return CheckHostName(element, UriHostNameType.IPv4);
		}

		private static bool CheckIpv6(JsonElement element)
		{
			return CheckHostName(element, UriHostNameType.IPv6);
		}

		private static bool CheckHostName(JsonElement element, UriHostNameType type)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			return System.Uri.CheckHostName(element.GetString()) == type;
		}

		private static bool CheckDuration(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return false;

			return Schema.Duration.TryParse(element.GetString(), out _);
		}
	}
}