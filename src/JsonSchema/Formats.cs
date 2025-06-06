﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// The formats supported by JSON Schema base specifications.
/// </summary>
public static partial class Formats
{
	private static readonly ConcurrentDictionary<string, Format> _registry;
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

#if NET7_0_OR_GREATER
	[GeneratedRegex(@"^((?:(\d{4}-\d{2}-\d{2})([Tt_]| )(\d{2}:\d{2}:\d{2}(?:\.\d+)?))([Zz]|[\+-]\d{2}:\d{2}))$", RegexOptions.Compiled, 250)]
	private static partial Regex DateTimeRegex();
#else
//from built from https://regex101.com/r/qH0sU7/1, edited to support all date+time examples in https://ijmacd.github.io/rfc3339-iso8601/
	private static readonly Regex _dateTimeRegex = new(@"^((?:(\d{4}-\d{2}-\d{2})([Tt_]| )(\d{2}:\d{2}:\d{2}(?:\.\d+)?))([Zz]|[\+-]\d{2}:\d{2}))$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

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
	[GeneratedRegex("^([a-zA-Z0-9]([a-zA-Z0-9\\-]{0,61}[a-zA-Z0-9])?\\.)+[a-zA-Z]{2,6}$", RegexOptions.Compiled, 250)]
	private static partial Regex HostnameRegex();
#else
	private static readonly Regex _hostnameRegex = new("^([a-zA-Z0-9]([a-zA-Z0-9\\-]{0,61}[a-zA-Z0-9])?\\.)+[a-zA-Z]{2,6}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

	private static Regex HostnameRegex() => _hostnameRegex;
#endif


	/// <summary>
	/// Defines the `hostname` format.
	/// </summary>
	public static readonly Format Hostname = new PredicateFormat("hostname", CheckHostName);
	/// <summary>
	/// Defines the `idn-email` format.
	/// </summary>
	public static readonly Format IdnEmail = new PredicateFormat("idn-email", CheckEmail);
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
	public static readonly Format Iri = new PredicateFormat("iri", CheckAbsoluteUri);
	/// <summary>
	/// Defines the `iri-reference` format.
	/// </summary>
	public static readonly Format IriReference = new PredicateFormat("iri-reference", CheckUri);
	/// <summary>
	/// Defines the `json-pointerOld` format.
	/// </summary>
	public static readonly Format JsonPointer = new PredicateFormat("json-pointerOld", CheckJsonPointer);
	/// <summary>
	/// Defines the `regex` format.
	/// </summary>
	public static readonly Format Regex = new PredicateFormat("regex", CheckRegex);
	/// <summary>
	/// Defines the `relative-json-pointerOld` format.
	/// </summary>
	public static readonly Format RelativeJsonPointer = new PredicateFormat("relative-json-pointerOld", CheckRelativeJsonPointer);
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
		_registry = new ConcurrentDictionary<string, Format>(new Dictionary<string, Format>() {
			{ Date.Key, Date },
			{ DateTime.Key, DateTime },
			{ Duration.Key, Duration },
			{ Email.Key, Email },
			{ Hostname.Key, Hostname },
			{ IdnEmail.Key, IdnEmail },
			{ IdnHostname.Key, IdnHostname },
			{ Ipv4.Key, Ipv4 },
			{ Ipv6.Key, Ipv6 },
			{ Iri.Key, Iri },
			{ IriReference.Key, IriReference },
			{ JsonPointer.Key, JsonPointer },
			{ Regex.Key, Regex },
			{ RelativeJsonPointer.Key, RelativeJsonPointer },
			{ Time.Key, Time },
			{ Uri.Key, Uri },
			{ UriReference.Key, UriReference },
			{ UriTemplate.Key, UriTemplate },
			{ Uuid.Key, Uuid }
		});
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

	private static bool CheckAbsoluteUri(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return System.Uri.TryCreate(node!.GetValue<string>(), UriKind.Absolute, out _);
	}

	private static bool CheckUri(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return System.Uri.TryCreate(node!.GetValue<string>(), UriKind.RelativeOrAbsolute, out _);
	}

	private static bool CheckUriTemplate(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		// The UriTemplate class has not been ported to .Net Standard/Core yet.
		return false;
		//return System.UriTemplate.Match(node.GetValue<string>());
	}

	private static bool CheckJsonPointer(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var str = node!.GetValue<string>();
		return string.IsNullOrEmpty(str) || (str[0] != '#' && Pointer.JsonPointer.TryParse(str, out _));
	}

	private static bool CheckRelativeJsonPointer(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Pointer.RelativeJsonPointer.TryParse(node!.GetValue<string>(), out _);
	}

#if NET7_0_OR_GREATER
	[GeneratedRegex(@"(@)(.+)$", RegexOptions.Compiled, 250)]
	private static partial Regex NormalizeDomainRegex();

	[GeneratedRegex("^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.Compiled| RegexOptions.IgnoreCase, 250)]
	private static partial Regex EmailFormatRegex();
#else
	private static readonly Regex _normalizeDomainRegex = new(@"(@)(.+)$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

	private static Regex NormalizeDomainRegex() => _normalizeDomainRegex;

	private static readonly Regex _emailFormatRegex =
		new("^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

	private static Regex EmailFormatRegex() => _emailFormatRegex;
#endif


	// source: https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
	private static bool CheckEmail(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var email = node!.GetValue<string>();

		if (string.IsNullOrWhiteSpace(email)) return false;

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

		try
		{
			return EmailFormatRegex().IsMatch(email);
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

	private static bool CheckUuid(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Guid.TryParseExact(node!.GetValue<string>(), "D", out _);
	}

	private static bool CheckDate(JsonNode? node)
	{
		return CheckDateFormat(node, "yyyy-MM-dd");
	}

	private static bool CheckTime(JsonNode? node)
	{
		return CheckDateFormat(node, _timeFormats);
	}

	private static bool CheckDateTime(JsonNode? node)
	{
		if (!CheckDateFormat(node))
		{
			return CheckDateTimePrecisionFormat(node);
		}

		return true;
	}

	private static bool CheckDateFormat(JsonNode? node, params string[] formats)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var dateString = node!.GetValue<string>().ToUpperInvariant();
		if (formats.Length != 0)
		{
			var canParseExact = System.DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
			if (canParseExact) return true;
		}

		return false;
	}

	private static bool CheckDateTimePrecisionFormat(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var dateString = node!.GetValue<string>().ToUpperInvariant();

		// date-times with very high precision don't get matched by TryParseExact but are still actually parsable.
		// We use a fallback to catch these cases
		var match = DateTimeRegex().Match(dateString);
		return match.Success;
	}

	private static bool CheckHostName(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node!.GetValue<string>();

		return HostnameRegex().IsMatch(value) && value.Length < 253;
	}

	private static bool CheckIdnHostName(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var type = System.Uri.CheckHostName(node!.GetValue<string>());

		return type != UriHostNameType.Unknown;
	}

	private static bool CheckIpv4(JsonNode? node)
	{
		return CheckHostName(node, UriHostNameType.IPv4);
	}

	private static bool CheckIpv6(JsonNode? node)
	{
		return CheckHostName(node, UriHostNameType.IPv6);
	}

	private static bool CheckHostName(JsonNode? node, UriHostNameType type)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var actualType = System.Uri.CheckHostName(node!.GetValue<string>());

		return actualType == type;
	}

	private static bool CheckDuration(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Schema.Duration.TryParse(node!.GetValue<string>(), out _);
	}

	private static bool CheckRegex(JsonNode? node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		try
		{
			_ = new Regex(node!.GetValue<string>(), RegexOptions.ECMAScript);
			return true;
		}
		catch
		{
			return false;
		}
	}
}