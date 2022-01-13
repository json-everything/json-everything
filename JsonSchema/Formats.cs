using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema
{
	/// <summary>
	/// The formats supported by JSON Schema base specifications.
	/// </summary>
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
			"HH':'mm':'ss'.'fffffffK",
			"HH':'mm':'ss'.'ffffffK",
			"HH':'mm':'ss'.'fffffK",
			"HH':'mm':'ss'.'ffffK",
			"HH':'mm':'ss'.'fffK",
			"HH':'mm':'ss'.'ffK",
			"HH':'mm':'ss'.'fK",
			"HH':'mm':'ssK",
			"HH':'mm':'ss"
		};

		/// <summary>
		/// Defines the `date` format.
		/// </summary>
		public static readonly Format Date = new PredicateFormat("date", CheckDate);
		/// <summary>
		/// Defines the `date-time` format.
		/// </summary>
		public static readonly Format DateTime = new PredicateFormat("date-time", CheckDateTime);
		/// <summary>
		/// Defines the `duration` format.
		/// </summary>
		public static readonly Format Duration = new PredicateFormat("duration", CheckDuration);
		/// <summary>
		/// Defines the `email` format.
		/// </summary>
		public static readonly Format Email = new PredicateFormat("email", CheckEmail);
		/// <summary>
		/// Defines the `hostname` format.
		/// </summary>
		public static readonly Format Hostname = new RegexFormat("hostname", "^[a-zA-Z][-.a-zA-Z0-9]{0,22}[a-zA-Z0-9]$");
		/// <summary>
		/// Defines the `idn-email` format.
		/// </summary>
		public static readonly Format IdnEmail = new PredicateFormat("idn-email", CheckEmail);
		/// <summary>
		/// Defines the `idn-hostname` format.
		/// </summary>
		public static readonly Format IdnHostname = new PredicateFormat("idn-hostname", CheckHostName);
		/// <summary>
		/// Defines the `ipv4` format.
		/// </summary>
		public static readonly Format Ipv4 = new PredicateFormat("ipv4", CheckIpv4);
		/// <summary>
		/// Defines the `ipv6` format.
		/// </summary>
		public static readonly Format Ipv6 = new PredicateFormat("ipv6", CheckIpv6);
		/// <summary>
		/// Defines the `iri` format.
		/// </summary>
		public static readonly Format Iri = new PredicateFormat("iri", CheckAbsoluteUri);
		/// <summary>
		/// Defines the `iri-reference` format.
		/// </summary>
		public static readonly Format IriReference = new PredicateFormat("iri-reference", CheckUri);
		/// <summary>
		/// Defines the `json-pointer` format.
		/// </summary>
		public static readonly Format JsonPointer = new PredicateFormat("json-pointer", CheckJsonPointer);
		/// <summary>
		/// Defines the `regex` format.
		/// </summary>
		public static readonly Format Regex = new PredicateFormat("regex", CheckRegex);
		/// <summary>
		/// Defines the `relative-json-pointer` format.
		/// </summary>
		public static readonly Format RelativeJsonPointer = new PredicateFormat("relative-json-pointer", CheckRelativeJsonPointer);
		/// <summary>
		/// Defines the `time` format.
		/// </summary>
		public static readonly Format Time = new PredicateFormat("time", CheckTime);
		/// <summary>
		/// Defines the `uri` format.
		/// </summary>
		public static readonly Format Uri = new PredicateFormat("uri", CheckAbsoluteUri);
		/// <summary>
		/// Defines the `uri-reference` format.
		/// </summary>
		public static readonly Format UriReference = new PredicateFormat("uri-reference", CheckUri);
		/// <summary>
		/// Defines the `uri-template` format.
		/// </summary>
		/// <remarks>
		/// This is currently the same check as `uri`.  The infrastructure to check URI templates
		/// [does not yet exist in .Net Standard/Core](https://github.com/dotnet/runtime/issues/41587).
		/// </remarks>
		public static readonly Format UriTemplate = new PredicateFormat("uri-template", CheckUriTemplate);
		/// <summary>
		/// Defines the `uuid` format.
		/// </summary>
		public static readonly Format Uuid = new PredicateFormat("uuid", CheckUuid);

		static Formats()
		{
			_registry = new ConcurrentDictionary<string, Format>(
				typeof(Formats)
					.GetFields(BindingFlags.Static | BindingFlags.Public)
					.Select(f => (Format) f.GetValue(null))
					.ToDictionary(f => f.Key));
		}

		/// <summary>
		/// Gets a format by its key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The specified format, if known; otherwise null.</returns>
		public static Format Get(string key)
		{
			return _registry.TryGetValue(key, out var format) ? format : CreateUnknown(key);
		}

		/// <summary>
		/// Registers a new format.
		/// </summary>
		/// <param name="format">The format.</param>
		public static void Register(Format format)
		{
			if (format == null)
				throw new ArgumentNullException(nameof(format));

			_registry[format.Key] = format;
		}

		/// <summary>
		/// Creates an unknown format.
		/// </summary>
		/// <param name="name">The format key.</param>
		/// <returns>A <see cref="Format"/> instance.</returns>
		public static Format CreateUnknown(string name)
		{
			return new UnknownFormat(name);
		}

		private static bool CheckAbsoluteUri(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

			return System.Uri.TryCreate(element.GetString(), UriKind.Absolute, out _);
		}

		private static bool CheckUri(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

			return System.Uri.TryCreate(element.GetString(), UriKind.RelativeOrAbsolute, out _);
		}

		private static bool CheckUriTemplate(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

			throw new NotSupportedException("The UriTemplate class has not been ported to .Net Standard/Core yet.");
			//return System.UriTemplate.Match(element.GetString());
		}

		private static bool CheckJsonPointer(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

			return Pointer.JsonPointer.TryParse(element.GetString(), out var p) && !p!.IsUriEncoded;
		}

		private static bool CheckRelativeJsonPointer(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

			return Pointer.RelativeJsonPointer.TryParse(element.GetString(), out _);
		}

		// source: https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
		private static bool CheckEmail(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

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
			if (element.ValueKind != JsonValueKind.String) return true;

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
			if (element.ValueKind != JsonValueKind.String) return true;

			return System.DateTime.TryParseExact(element.GetString().ToUpperInvariant(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
		}

		private static bool CheckHostName(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

			var type = System.Uri.CheckHostName(element.GetString());

			return type != UriHostNameType.Unknown;
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
			if (element.ValueKind != JsonValueKind.String) return true;

			var actualType = System.Uri.CheckHostName(element.GetString());

			return actualType == type;
		}

		private static bool CheckDuration(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

			return Schema.Duration.TryParse(element.GetString(), out _);
		}

		private static bool CheckRegex(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String) return true;

			try
			{
				var _ = new Regex(element.GetString());
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}