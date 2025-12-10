using System;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Json.Schema;

/// <summary>
/// The formats supported by JSON Schema base specifications.
/// </summary>
public static partial class Formats
{
#if NET7_0_OR_GREATER
	[GeneratedRegex(@"^((?:([0-9]{4}-[0-9]{2}-[0-9]{2})([Tt_]| )([0-9]{2}:[0-9]{2}:[0-9]{2}(?:\.[0-9]+)?))([Zz]|[\+-][0-9]{2}:[0-9]{2}))$", RegexOptions.Compiled, 250)]
	private static partial Regex DateTimeRegex();
#else
// from https://regex101.com/r/qH0sU7/1, adjusted to enforce ASCII digits only
	private static readonly Regex _dateTimeRegex = new(@"^((?:([0-9]{4}-[0-9]{2}-[0-9]{2})([Tt_]| )([0-9]{2}:[0-9]{2}:[0-9]{2}(?:\.[0-9]+)?))([Zz]|[\+-][0-9]{2}:[0-9]{2}))$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

	private static Regex DateTimeRegex() => _dateTimeRegex;
#endif


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


#if NET7_0_OR_GREATER
	[GeneratedRegex(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.Compiled, 250)]
	private static partial Regex HostnameRegex();
#else
	private static readonly Regex _hostnameRegex = new(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

	private static Regex HostnameRegex() => _hostnameRegex;
#endif


	/// <summary>
	/// Defines the `hostname` format.
	/// </summary>
	public static readonly Format Hostname = new PredicateFormat("hostname", CheckHostName);
	/// <summary>
	/// Defines the `idn-email` format.
	/// </summary>
	public static readonly Format IdnEmail = new PredicateFormat("idn-email", CheckIdnEmail);
	/// <summary>
	/// Defines the `idn-hostname` format.
	/// </summary>
	public static readonly Format IdnHostname = new PredicateFormat("idn-hostname", CheckIdnHostName);
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
	public static readonly Format Iri = new PredicateFormat("iri", CheckAbsoluteIri);
	/// <summary>
	/// Defines the `iri-reference` format.
	/// </summary>
	public static readonly Format IriReference = new PredicateFormat("iri-reference", CheckIriReference);
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

	/// <summary>
	/// Gets a format by its key.
	/// </summary>
	/// <param name="key">The key.</param>
	/// <returns>The specified format, if known; otherwise null.</returns>
	[Obsolete("This just calls FormatRegistry.Global.Get().  Use that instead.")]
	public static Format Get(string key)
	{
		return FormatRegistry.Global.Get(key);
	}

	/// <summary>
	/// Registers a new format.
	/// </summary>
	/// <param name="format">The format.</param>
	[Obsolete("This just calls FormatRegistry.Global.Register().  Use that instead.")]
	public static void Register(Format format)
	{
		FormatRegistry.Global.Register(format);
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

	private static bool CheckAbsoluteIri(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Reject invalid characters per RFC3987 (IRI)
		// Must not contain unescaped: space, <, >, {, }, |, \, ^, `, "
		foreach (var ch in value)
		{
			if (ch == ' ' || ch == '<' || ch == '>' || ch == '{' || ch == '}' || 
			    ch == '|' || ch == '\\' || ch == '^' || ch == '`' || ch == '"')
				return false;
		}
		
		if (!System.Uri.TryCreate(value, UriKind.Absolute, out var uri))
			return false;

		// Must have a proper scheme (reject UNC paths)
		return !string.IsNullOrEmpty(uri.Scheme) && value.IndexOf(':') > 0;
	}

	private static bool CheckAbsoluteUri(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Reject invalid characters per RFC3986 (URI)
		// Must not contain unescaped: space, <, >, {, }, |, \, ^, `, "
		foreach (var ch in value)
		{
			if (ch == ' ' || ch == '<' || ch == '>' || ch == '{' || ch == '}' || 
			    ch == '|' || ch == '\\' || ch == '^' || ch == '`' || ch == '"')
				return false;
			
			// URI must be ASCII-only (non-ASCII must be percent-encoded)
			if (ch > 127) return false;
		}

		// Check for invalid [ or ] in userinfo (before @)
		var atIndex = value.IndexOf('@');
		if (atIndex > 0)
		{
			var schemeEnd = value.IndexOf("://");
			if (schemeEnd >= 0)
			{
				var userinfo = value.Substring(schemeEnd + 3, atIndex - schemeEnd - 3);
				if (userinfo.IndexOf('[') >= 0 || userinfo.IndexOf(']') >= 0)
					return false;
			}
		}
		
		if (!System.Uri.TryCreate(value, UriKind.Absolute, out var uri))
			return false;

		// Must have a proper scheme (reject UNC paths)
		return !string.IsNullOrEmpty(uri.Scheme) && value.IndexOf(':') > 0;
	}

	private static bool CheckIriReference(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Reject invalid characters per RFC3987 (IRI)
		// Must not contain: backslash, space, <, >, {, }, |, ^, `, "
		foreach (var ch in value)
		{
			if (ch == '\\' || ch == ' ' || ch == '<' || ch == '>' || ch == '{' || 
			    ch == '}' || ch == '|' || ch == '^' || ch == '`' || ch == '"')
				return false;
		}

		return System.Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out _);
	}

	private static bool CheckUri(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Reject invalid characters per RFC3986
		// Must not contain: backslash, space, <, >, {, }, |, ^, `, "
		foreach (var ch in value)
		{
			if (ch == '\\' || ch == ' ' || ch == '<' || ch == '>' || ch == '{' || 
			    ch == '}' || ch == '|' || ch == '^' || ch == '`' || ch == '"')
				return false;
			
			// URI-reference must be ASCII-only (non-ASCII must be percent-encoded)
			if (ch > 127) return false;
		}

		return System.Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out _);
	}

	private static bool CheckUriTemplate(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Basic RFC 6570 URI Template validation
		// Check that all { have matching } and vice versa
		int depth = 0;
		bool inExpression = false;

		for (int i = 0; i < value.Length; i++)
		{
			var ch = value[i];

			if (ch == '{')
			{
				if (inExpression) return false; // nested braces not allowed
				inExpression = true;
				depth++;
			}
			else if (ch == '}')
			{
				if (!inExpression) return false; // closing without opening
				inExpression = false;
				depth--;
			}
		}

		// All braces must be closed
		return depth == 0;
	}

	private static bool CheckJsonPointer(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var str = node.GetString()!;
		return string.IsNullOrEmpty(str) || (str[0] != '#' && Pointer.JsonPointer.TryParse(str, out _));
	}

	private static bool CheckRelativeJsonPointer(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Pointer.RelativeJsonPointer.TryParse(node.GetString()!, out _);
	}

#if NET7_0_OR_GREATER
	[GeneratedRegex(@"(@)(.+)$", RegexOptions.Compiled, 250)]
	private static partial Regex NormalizeDomainRegex();

	[GeneratedRegex(@"^(?("")("".+?(?<!\\)""@)|([-!#$%&'*+/=?^`{}|~A-Za-z0-9_]+(\.((?!\.)[-!#$%&'*+/=?^`{}|~A-Za-z0-9_]+))*@))(\[([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}|(?i:IPv6:)[0-9A-Fa-f:.]+)\]|([A-Za-z0-9]([A-Za-z0-9\-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,63})$", RegexOptions.Compiled, 250)]
	private static partial Regex EmailFormatRegex();
#else
	private static readonly Regex _normalizeDomainRegex = new(@"(@)(.+)$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

	private static Regex NormalizeDomainRegex() => _normalizeDomainRegex;

	private static readonly Regex _emailFormatRegex =
		new(@"^(?("")("".+?(?<!\\)""@)|([-!#$%&'*+/=?^`{}|~A-Za-z0-9_]+(\.((?!\.)[-!#$%&'*+/=?^`{}|~A-Za-z0-9_]+))*@))(\[([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}|(?i:IPv6:)[0-9A-Fa-f:.]+)\]|([A-Za-z0-9]([A-Za-z0-9\-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,63})$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

	private static Regex EmailFormatRegex() => _emailFormatRegex;
#endif


	// source: https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
	private static bool CheckEmail(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var email = node.GetString()!;

		if (string.IsNullOrWhiteSpace(email)) return false;

		// Skip IDN normalization for domain-literals
		var atIndex = email.LastIndexOf('@');
		var isDomainLiteral = atIndex >= 0 && atIndex + 1 < email.Length && email[atIndex + 1] == '[';

		if (!isDomainLiteral)
		{
			try
			{
				// Normalize the domain
				email = NormalizeDomainRegex().Replace(email, DomainMapper);
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}

		try
		{
			if (!EmailFormatRegex().IsMatch(email)) return false;

			// Validate IP address domain-literal if present
			if (isDomainLiteral)
			{
				var domain = email.Substring(atIndex + 1);
				if (!domain.StartsWith("[") || !domain.EndsWith("]"))
					return false;

				var addrContent = domain.Substring(1, domain.Length - 2);
				
				if (addrContent.StartsWith("IPv6:", StringComparison.OrdinalIgnoreCase))
				{
					// IPv6 literal
					var addr = addrContent.Substring(5);
					if (!System.Net.IPAddress.TryParse(addr, out var ip)) return false;
					if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6) return false;
				}
				else
				{
					// IPv4 literal
					if (!System.Net.IPAddress.TryParse(addrContent, out var ip)) return false;
					if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
				}
			}

			return true;
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

	private static bool CheckIdnEmail(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var email = node.GetString()!;

		if (string.IsNullOrWhiteSpace(email)) return false;

		var atIndex = email.LastIndexOf('@');
		if (atIndex <= 0 || atIndex == email.Length - 1) return false;

		var local = email.Substring(0, atIndex);
		var domain = email.Substring(atIndex + 1);

		// Local-part: allow Unicode or quoted strings, basic validation only
		if (local.Length == 0) return false;
		if (local[0] == '"')
		{
			// Must end with quote
			if (local[local.Length - 1] != '"') return false;
		}
		else
		{
			// Unquoted: disallow leading/trailing dots and consecutive dots
			if (local[0] == '.' || local[local.Length - 1] == '.') return false;
			if (local.IndexOf("..") >= 0) return false;
		}

		// Domain: normalize and validate using CheckIdnHostName
		if (domain.Length == 0) return false;
		
		// Handle domain literals (IPv4/IPv6)
		if (domain[0] == '[')
		{
			if (!domain.EndsWith("]")) return false;
			var addrContent = domain.Substring(1, domain.Length - 2);
			
			if (addrContent.StartsWith("IPv6:", StringComparison.OrdinalIgnoreCase))
			{
				var addr = addrContent.Substring(5);
				if (!System.Net.IPAddress.TryParse(addr, out var ip)) return false;
				if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6) return false;
			}
			else
			{
				if (!System.Net.IPAddress.TryParse(addrContent, out var ip)) return false;
				if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;
			}
			return true;
		}

		// Use idn-hostname validation for domain
		var testNode = JsonDocument.Parse($"\"{domain}\"").RootElement;
		return CheckIdnHostName(testNode);
	}

	private static bool CheckUuid(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Guid.TryParseExact(node.GetString()!, "D", out _);
	}

	private static bool CheckDate(JsonElement node)
	{
		return CheckDateFormat(node, "yyyy-MM-dd");
	}

	private static bool CheckTime(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var timeString = node.GetString()!;

		// Must contain offset (Z or +/-HH:MM)
		if (!timeString.EndsWith("Z", StringComparison.OrdinalIgnoreCase) && 
		    timeString.IndexOf('+') < 0 && timeString.IndexOf('-') < 0)
			return false;

		// Parse the time and offset parts
		string timePart;
		string offsetPart;

		if (timeString.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
		{
			timePart = timeString.Substring(0, timeString.Length - 1);
			offsetPart = timeString.Substring(timeString.Length - 1);
		}
		else
		{
			var lastPlus = timeString.LastIndexOf('+');
			var lastMinus = timeString.LastIndexOf('-');
			var offsetIndex = Math.Max(lastPlus, lastMinus);
			if (offsetIndex <= 0) return false;

			timePart = timeString.Substring(0, offsetIndex);
			offsetPart = timeString.Substring(offsetIndex);
		}

		// Enforce ASCII digits only
		static bool IsAsciiDigits(string s)
		{
			foreach (var ch in s)
			{
				if (ch < '0' || ch > '9') return false;
			}
			return true;
		}

		// Parse time HH:MM:SS[.fraction]
		var fracSplit = timePart.Split('.');
		var hms = fracSplit[0].Split(':');
		if (hms.Length != 3) return false;

		// Each component must be exactly 2 digits
		if (hms[0].Length != 2 || hms[1].Length != 2 || hms[2].Length != 2) return false;
		if (!IsAsciiDigits(hms[0]) || !IsAsciiDigits(hms[1]) || !IsAsciiDigits(hms[2])) return false;

		if (!int.TryParse(hms[0], NumberStyles.None, CultureInfo.InvariantCulture, out var hour)) return false;
		if (!int.TryParse(hms[1], NumberStyles.None, CultureInfo.InvariantCulture, out var minute)) return false;
		if (!int.TryParse(hms[2], NumberStyles.None, CultureInfo.InvariantCulture, out var second)) return false;

		if (hour < 0 || hour > 23) return false;
		if (minute < 0 || minute > 59) return false;

		var isLeapSecond = second == 60;
		if (!isLeapSecond)
		{
			if (second < 0 || second > 59) return false;
		}

		// Validate fraction if present
		if (fracSplit.Length > 1)
		{
			var frac = fracSplit[1];
			if (frac.Length == 0) return false;
			if (!IsAsciiDigits(frac)) return false;
		}

		// Validate offset
		if (offsetPart.Equals("Z", StringComparison.Ordinal) || offsetPart.Equals("z", StringComparison.Ordinal))
		{
			if (isLeapSecond && !(hour == 23 && minute == 59)) return false;
		}
		else
		{
			var sign = offsetPart[0];
			if (sign != '+' && sign != '-') return false;
			
			var off = offsetPart.Substring(1).Split(':');
			if (off.Length != 2) return false;
			
			// Each offset component must be exactly 2 digits
			if (off[0].Length != 2 || off[1].Length != 2) return false;
			if (!IsAsciiDigits(off[0]) || !IsAsciiDigits(off[1])) return false;
			
			if (!int.TryParse(off[0], NumberStyles.None, CultureInfo.InvariantCulture, out var offHour)) return false;
			if (!int.TryParse(off[1], NumberStyles.None, CultureInfo.InvariantCulture, out var offMinute)) return false;
			if (offHour < 0 || offHour > 23) return false;
			if (offMinute < 0 || offMinute > 59) return false;

			// Validate leap second against UTC
			if (isLeapSecond)
			{
				var utcHour = hour;
				var utcMinute = minute;
				if (sign == '+')
				{
					utcHour -= offHour;
					utcMinute -= offMinute;
				}
				else
				{
					utcHour += offHour;
					utcMinute += offMinute;
				}

				if (utcMinute < 0)
				{
					utcMinute += 60;
					utcHour -= 1;
				}
				else if (utcMinute >= 60)
				{
					utcMinute -= 60;
					utcHour += 1;
				}

				utcHour %= 24;
				if (utcHour < 0) utcHour += 24;

				if (!(utcHour == 23 && utcMinute == 59)) return false;
			}
		}

		return true;
	}

	private static bool CheckDateTime(JsonElement node)
	{
		if (!CheckDateFormat(node))
		{
			return CheckDateTimePrecisionFormat(node);
		}

		return true;
	}

	private static bool CheckDateFormat(JsonElement node, params string[] formats)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var dateString = node.GetString()!.ToUpperInvariant();
		if (formats.Length != 0)
		{
			var canParseExact = System.DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
			if (canParseExact) return true;
		}

		return false;
	}

	private static bool CheckDateTimePrecisionFormat(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var dateString = node.GetString()!;

		var match = DateTimeRegex().Match(dateString);
		if (!match.Success) return false;

		// Extract groups: 2=date, 3=separator, 4=time, 5=offset
		var datePart = match.Groups[2].Value; // YYYY-MM-DD
		var timePart = match.Groups[4].Value; // HH:MM:SS[.fraction]
		var offsetPart = match.Groups[5].Value; // Z or ±HH:MM

		// Enforce ASCII digits only within numeric segments
		static bool IsAsciiDigits(string s)
		{
			foreach (var ch in s)
			{
				if (ch < '0' || ch > '9') return false;
			}
			return true;
		}

		var ymd = datePart.Split('-');
		if (ymd.Length != 3) return false;
		if (!IsAsciiDigits(ymd[0]) || !IsAsciiDigits(ymd[1]) || !IsAsciiDigits(ymd[2])) return false;
		if (!int.TryParse(ymd[0], NumberStyles.None, CultureInfo.InvariantCulture, out var year)) return false;
		if (!int.TryParse(ymd[1], NumberStyles.None, CultureInfo.InvariantCulture, out var month)) return false;
		if (!int.TryParse(ymd[2], NumberStyles.None, CultureInfo.InvariantCulture, out var day)) return false;
		if (month < 1 || month > 12) return false;
		var daysInMonth = System.DateTime.DaysInMonth(year, month);
		if (day < 1 || day > daysInMonth) return false;

		var fracSplit = timePart.Split('.');
		var hms = fracSplit[0].Split(':');
		if (hms.Length != 3) return false;
		if (!IsAsciiDigits(hms[0]) || !IsAsciiDigits(hms[1]) || !IsAsciiDigits(hms[2])) return false;
		if (!int.TryParse(hms[0], NumberStyles.None, CultureInfo.InvariantCulture, out var hour)) return false;
		if (!int.TryParse(hms[1], NumberStyles.None, CultureInfo.InvariantCulture, out var minute)) return false;
		if (!int.TryParse(hms[2], NumberStyles.None, CultureInfo.InvariantCulture, out var second)) return false;
		if (hour < 0 || hour > 23) return false;
		if (minute < 0 || minute > 59) return false;
		// RFC3339 allows leap second 60 only at 23:59:60 UTC.
		// Validate by converting local time to UTC using the offset and
		// ensuring the resulting time is 23:59 when seconds==60.
		var isLeapSecond = second == 60;
		if (!isLeapSecond)
		{
			if (second < 0 || second > 59) return false;
		}

		if (fracSplit.Length > 1)
		{
			var frac = fracSplit[1];
			if (frac.Length == 0) return false;
			if (!IsAsciiDigits(frac)) return false;
		}

		// Offset validation
		if (offsetPart.Equals("Z", StringComparison.Ordinal) || offsetPart.Equals("z", StringComparison.Ordinal))
		{
			// Zulu time; for leap second, must be 23:59
			if (isLeapSecond && !(hour == 23 && minute == 59)) return false;
		}
		else
		{
			// ±HH:MM
			var sign = offsetPart[0];
			if (sign != '+' && sign != '-') return false;
			var off = offsetPart.Substring(1).Split(':');
			if (off.Length != 2) return false;
			if (!IsAsciiDigits(off[0]) || !IsAsciiDigits(off[1])) return false;
			if (!int.TryParse(off[0], NumberStyles.None, CultureInfo.InvariantCulture, out var offHour)) return false;
			if (!int.TryParse(off[1], NumberStyles.None, CultureInfo.InvariantCulture, out var offMinute)) return false;
			if (offHour < 0 || offHour > 23) return false;
			if (offMinute < 0 || offMinute > 59) return false;

			if (isLeapSecond)
			{
				// Compute UTC time-of-day: local time minus offset sign
				// RFC3339 offset meaning: local time = UTC + offset
				// Therefore UTC = local time - offset
				var utcHour = hour;
				var utcMinute = minute;
				if (sign == '+')
				{
					utcHour -= offHour;
					utcMinute -= offMinute;
				}
				else // sign == '-'
				{
					utcHour += offHour;
					utcMinute += offMinute;
				}

				// Normalize minutes
				if (utcMinute < 0)
				{
					utcMinute += 60;
					utcHour -= 1;
				}
				else if (utcMinute >= 60)
				{
					utcMinute -= 60;
					utcHour += 1;
				}

				// Normalize hours to 0..23 (date rollover ignored for validation)
				utcHour %= 24;
				if (utcHour < 0) utcHour += 24;

				if (!(utcHour == 23 && utcMinute == 59)) return false;
			}
		}

		return true;
	}

	private static bool CheckHostName(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Empty, too long, or contains underscore
		if (string.IsNullOrEmpty(value) || value.Length > 255 || value.IndexOf('_') >= 0) return false;

		// Leading or trailing dot
		if (value[0] == '.' || value[value.Length - 1] == '.') return false;

		// Check basic pattern
		if (!HostnameRegex().IsMatch(value)) return false;

		// Split into labels and validate each
		var labels = value.Split('.');
		foreach (var label in labels)
		{
			// Each label max 63 chars
			if (label.Length > 63) return false;

			// Cannot have -- in positions 3-4 (index 2-3)
			// Exception: xn-- is allowed as punycode prefix (positions 1-2, not 3-4)
			if (label.Length >= 4 && label[2] == '-' && label[3] == '-')
			{
				return false;
			}
		}

		return true;
	}

	private static bool CheckIdnHostName(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Empty, too long, or contains underscore
		if (string.IsNullOrEmpty(value) || value.Length > 255 || value.IndexOf('_') >= 0) return false;

		// Reject Unicode dot separators (fullwidth, halfwidth ideographic)
		// U+FF0E fullwidth full stop, U+FF61 halfwidth ideographic full stop, U+3002 ideographic full stop
		if (value.IndexOf('\uFF0E') >= 0 || value.IndexOf('\uFF61') >= 0 || value.IndexOf('\u3002') >= 0)
			return false;

		// Leading or trailing dot
		if (value[0] == '.' || value[value.Length - 1] == '.') return false;

		// Split into labels and validate each
		var labels = value.Split('.');
		foreach (var label in labels)
		{
			if (label.Length == 0) return false;
			
			// Each label max 63 chars
			if (label.Length > 63) return false;

			// Cannot start or end with hyphen
			if (label[0] == '-' || label[label.Length - 1] == '-') return false;

			// Cannot have -- in positions 3-4 (index 2-3)
			if (label.Length >= 4 && label[2] == '-' && label[3] == '-')
			{
				return false;
			}

			// Check for disallowed starting characters (combining marks)
			if (label.Length > 0)
			{
				var firstChar = label[0];
				var category = char.GetUnicodeCategory(firstChar);
				
				// Cannot start with combining marks
				if (category == System.Globalization.UnicodeCategory.SpacingCombiningMark ||
				    category == System.Globalization.UnicodeCategory.NonSpacingMark ||
				    category == System.Globalization.UnicodeCategory.EnclosingMark)
				{
					return false;
				}
			}

			// IDNA2008 contextual rules for specific characters
			for (int i = 0; i < label.Length; i++)
			{
				var ch = label[i];
				
				// U+00B7 MIDDLE DOT: must be surrounded by 'l' (U+006C)
				if (ch == '\u00B7')
				{
					if (i == 0 || i == label.Length - 1) return false;
					if (label[i - 1] != 'l' || label[i + 1] != 'l') return false;
				}
				
				// U+0375 Greek KERAIA: must be followed by Greek letter
				if (ch == '\u0375')
				{
					if (i == label.Length - 1) return false;
					var next = label[i + 1];
					// Check if next char is Greek (U+0370-U+03FF range)
					if (next < '\u0370' || next > '\u03FF') return false;
				}
				
				// U+05F3 Hebrew GERESH: must be preceded by Hebrew letter
				if (ch == '\u05F3')
				{
					if (i == 0) return false;
					var prev = label[i - 1];
					// Check if prev char is Hebrew (U+0590-U+05FF range)
					if (prev < '\u0590' || prev > '\u05FF') return false;
				}
				
				// U+05F4 Hebrew GERSHAYIM: must be preceded by Hebrew letter
				if (ch == '\u05F4')
				{
					if (i == 0) return false;
					var prev = label[i - 1];
					// Check if prev char is Hebrew (U+0590-U+05FF range)
					if (prev < '\u0590' || prev > '\u05FF') return false;
				}
				
				// U+30FB KATAKANA MIDDLE DOT: must have Hiragana, Katakana, or Han in the label
				if (ch == '\u30FB')
				{
					bool hasRequiredScript = false;
					foreach (var c in label)
					{
						// Hiragana: U+3040-U+309F, Katakana: U+30A0-U+30FF, Han: U+4E00-U+9FFF
						if ((c >= '\u3040' && c <= '\u309F') || 
						    (c >= '\u30A0' && c <= '\u30FF') || 
						    (c >= '\u4E00' && c <= '\u9FFF'))
						{
							hasRequiredScript = true;
							break;
						}
					}
					if (!hasRequiredScript) return false;
				}
				
				// U+0660-U+0669 Arabic-Indic digits cannot mix with U+06F0-U+06F9 Extended Arabic-Indic
				if (ch >= '\u0660' && ch <= '\u0669')
				{
					foreach (var c in label)
					{
						if (c >= '\u06F0' && c <= '\u06F9') return false;
					}
				}
				if (ch >= '\u06F0' && ch <= '\u06F9')
				{
					foreach (var c in label)
					{
						if (c >= '\u0660' && c <= '\u0669') return false;
					}
				}
				
				// U+200D ZERO WIDTH JOINER: must be preceded by Virama
				if (ch == '\u200D')
				{
					if (i == 0) return false;
					var prev = label[i - 1];
					// Virama combining class is 9
					if (char.GetUnicodeCategory(prev) != System.Globalization.UnicodeCategory.NonSpacingMark ||
					    !IsVirama(prev))
					{
						return false;
					}
				}
			}
		}

		return true;
	}

	private static bool IsVirama(char ch)
	{
		// Common Virama characters (combining class 9)
		// This is a simplified check for the most common Viramas
		return ch == '\u094D' || // Devanagari
		       ch == '\u09CD' || // Bengali
		       ch == '\u0A4D' || // Gurmukhi
		       ch == '\u0ACD' || // Gujarati
		       ch == '\u0B4D' || // Oriya
		       ch == '\u0BCD' || // Tamil
		       ch == '\u0C4D' || // Telugu
		       ch == '\u0CCD' || // Kannada
		       ch == '\u0D4D' || // Malayalam
		       ch == '\u0DCA';   // Sinhala
	}

	private static bool CheckIpv4(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Must be exactly 4 decimal octets separated by dots
		var parts = value.Split('.');
		if (parts.Length != 4) return false;

		foreach (var part in parts)
		{
			// Empty part
			if (part.Length == 0) return false;

			// Must be 1-3 digits
			if (part.Length > 3) return false;

			// Check all characters are ASCII digits
			foreach (var ch in part)
			{
				if (ch < '0' || ch > '9') return false;
			}

			// No leading zeros (except "0" itself)
			if (part.Length > 1 && part[0] == '0') return false;

			// Parse and check range 0-255
			if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var octet))
				return false;
			if (octet < 0 || octet > 255) return false;
		}

		return true;
	}

	private static bool CheckIpv6(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// Reject zone ID (%) and netmask (/)
		if (value.IndexOf('%') >= 0 || value.IndexOf('/') >= 0) return false;

		var actualType = System.Uri.CheckHostName(value);

		return actualType == UriHostNameType.IPv6;
	}

	private static bool CheckHostName(JsonElement node, UriHostNameType type)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var actualType = System.Uri.CheckHostName(node.GetString()!);

		return actualType == type;
	}

	private static bool CheckDuration(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Schema.Duration.TryParse(node.GetString()!, out _);
	}

	private static bool CheckRegex(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var pattern = node.GetString()!;

		// Check for .NET-specific escapes that are not valid in ECMA-262
		// ECMA-262 valid escapes: b, f, n, r, t, v, 0 (null), x (hex), u (unicode),
		// c (control), and special regex chars like ., *, +, ?, [, ], {, }, (, ), |, ^, $, \, /
		// .NET-specific (invalid in ECMA-262): a (alert), e (escape), A, G, z, Z (anchors)
		for (int i = 0; i < pattern.Length - 1; i++)
		{
			if (pattern[i] == '\\')
			{
				var next = pattern[i + 1];
				// .NET-specific escapes not in ECMA-262
				if (next == 'a' || // alert/bell
				    next == 'e' || // escape
				    next == 'A' || // start of string (use ^ in ECMA-262)
				    next == 'G' || // end of previous match
				    next == 'z' || // end of string before newline (use $ in ECMA-262)  
				    next == 'Z')   // end of string (use $ in ECMA-262)
				{
					return false;
				}
				// \p and \P (Unicode property escapes) - these are ES2018+ but .NET ECMAScript mode rejects them
				// \k (named backreference) - ECMA-262 supports this, .NET ECMAScript mode should too
				// Lookbehinds (?<=...) and (?<!...) - ES2018+ but .NET doesn't support in ECMAScript mode
				// Named groups (?<name>...) - ES2018+ but .NET doesn't support in ECMAScript mode
			}
		}

		try
		{
			_ = new Regex(pattern, RegexOptions.ECMAScript);
			return true;
		}
		catch
		{
			return false;
		}
	}
}