using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace Json.Schema;

/// <summary>
/// The formats supported by JSON Schema base specifications.
/// </summary>
public static partial class Formats
{
#if NET7_0_OR_GREATER
	[GeneratedRegex(@"^((?:([0-9]{4}-[0-9]{2}-[0-9]{2})([Tt_]| )([0-9]{2}:[0-9]{2}:[0-9]{2}(?:\.[0-9]+)?))([Zz]|[\+-][0-9]{2}:[0-9]{2}))\z", RegexOptions.Compiled, 250)]
	private static partial Regex DateTimeRegex();

	// RFC 9557 section 4.1: suffix = [time-zone] *suffix-tag
	[GeneratedRegex(@"^(?:\[!?(?:(?!\.{1,2}[/\]])[A-Za-z._][A-Za-z0-9._+\-]{0,13}(?:/(?!\.{1,2}[/\]])[A-Za-z._][A-Za-z0-9._+\-]{0,13})*|[+\-](?:[01][0-9]|2[0-3]):[0-5][0-9])\])?(?:\[!?[a-z_][a-z0-9_\-]*=[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*\])*\z", RegexOptions.Compiled, 250)]
	private static partial Regex DateTimeSuffixRegex();
#else
// from https://regex101.com/r/qH0sU7/1, adjusted to enforce ASCII digits only
	private static readonly Regex _dateTimeRegex = new(@"^((?:([0-9]{4}-[0-9]{2}-[0-9]{2})([Tt_]| )([0-9]{2}:[0-9]{2}:[0-9]{2}(?:\.[0-9]+)?))([Zz]|[\+-][0-9]{2}:[0-9]{2}))\z", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

	private static Regex DateTimeRegex() => _dateTimeRegex;

	// RFC 9557 section 4.1: suffix = [time-zone] *suffix-tag
	private static readonly Regex _dateTimeSuffixRegex = new(@"^(?:\[!?(?:(?!\.{1,2}[/\]])[A-Za-z._][A-Za-z0-9._+\-]{0,13}(?:/(?!\.{1,2}[/\]])[A-Za-z._][A-Za-z0-9._+\-]{0,13})*|[+\-](?:[01][0-9]|2[0-3]):[0-5][0-9])\])?(?:\[!?[a-z_][a-z0-9_\-]*=[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*\])*\z", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

	private static Regex DateTimeSuffixRegex() => _dateTimeSuffixRegex;
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
	[GeneratedRegex(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*\z", RegexOptions.Compiled, 250)]
	private static partial Regex HostnameRegex();
#else
	private static readonly Regex _hostnameRegex = new(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*\z", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

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
		int i = 0;
		return TryParseUriOrIriRfc(value, ref i, true) && i == value.Length;
	}

	private static bool CheckAbsoluteUri(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;
		var value = node.GetString()!;
		int i = 0;
		return TryParseUriOrIriRfc(value, ref i, false) && i == value.Length;
	}

	private static bool CheckIriReference(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;
		return TryParseUriOrIriReferenceRfc(node.GetString()!, true);
	}

	private static bool CheckUri(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;
		return TryParseUriOrIriReferenceRfc(node.GetString()!, false);
	}

	// RFC 3986/3987 character classification helpers
	private static bool IsAlphaChar(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
	private static bool IsDigitChar(char c) => c >= '0' && c <= '9';
	private static bool IsHexDigitChar(char c) => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
	private static bool IsUnreservedChar(char c) => IsAlphaChar(c) || IsDigitChar(c) || c == '-' || c == '.' || c == '_' || c == '~';
	private static bool IsSubDelimChar(char c) => c is '!' or '$' or '&' or '\'' or '(' or ')' or '*' or '+' or ',' or ';' or '=';

	// ucschar (RFC 3987) — BMP ranges; non-BMP handled via surrogate pairs in ConsumeSegmentRfc
	private static bool IsUcscharBmp(char c)
		=> (c >= '\xA0' && c <= '\uD7FF') ||
		   (c >= '\uF900' && c <= '\uFDCF') ||
		   (c >= '\uFDD0' && c <= '\uFDEF') ||
		   (c >= '\uFFF0' && c <= '\uFFFD');

	// iprivate BMP range (RFC 3987): %xE000-F8FF
	private static bool IsIprivateBmp(char c) => c >= '\uE000' && c <= '\uF8FF';

	// URI / IRI = scheme ":" hier-part [ "?" query ] [ "#" fragment ]
	private static bool TryParseUriOrIriRfc(string s, ref int i, bool isIri)
	{
		if (!TryParseSchemeRfc(s, ref i)) return false;
		if (i >= s.Length || s[i] != ':') return false;
		i++;
		if (!TryParseHierPartRfc(s, ref i, isIri)) return false;
		if (i < s.Length && s[i] == '?') { i++; ConsumeQueryOrFragmentRfc(s, ref i, isIri, true); }
		if (i < s.Length && s[i] == '#') { i++; ConsumeQueryOrFragmentRfc(s, ref i, isIri, false); }
		return true;
	}

	// URI-reference / IRI-reference = URI / relative-ref
	private static bool TryParseUriOrIriReferenceRfc(string s, bool isIri)
	{
		var i = 0;
		if (TryParseUriOrIriRfc(s, ref i, isIri) && i == s.Length) return true;
		i = 0;
		if (!TryParseRelativePartRfc(s, ref i, isIri)) return false;
		if (i < s.Length && s[i] == '?') { i++; ConsumeQueryOrFragmentRfc(s, ref i, isIri, true); }
		if (i < s.Length && s[i] == '#') { i++; ConsumeQueryOrFragmentRfc(s, ref i, isIri, false); }
		return i == s.Length;
	}

	// scheme = ALPHA *( ALPHA / DIGIT / "+" / "-" / "." )
	private static bool TryParseSchemeRfc(string s, ref int i)
	{
		if (i >= s.Length || !IsAlphaChar(s[i])) return false;
		i++;
		while (i < s.Length && (IsAlphaChar(s[i]) || IsDigitChar(s[i]) || s[i] == '+' || s[i] == '-' || s[i] == '.'))
			i++;
		return true;
	}

	// hier-part = "//" authority path-abempty / path-absolute / path-rootless / path-empty
	private static bool TryParseHierPartRfc(string s, ref int i, bool isIri)
	{
		if (i + 1 < s.Length && s[i] == '/' && s[i + 1] == '/')
		{
			i += 2;
			if (!TryParseAuthorityRfc(s, ref i, isIri)) return false;
			return TryParsePathAbemptyRfc(s, ref i, isIri);
		}
		if (i < s.Length && s[i] == '/') return TryParsePathAbsoluteRfc(s, ref i, isIri);
		return TryParsePathRootlessOrEmptyRfc(s, ref i, isIri);
	}

	// relative-part = "//" authority path-abempty / path-absolute / path-noscheme / path-empty
	private static bool TryParseRelativePartRfc(string s, ref int i, bool isIri)
	{
		if (i + 1 < s.Length && s[i] == '/' && s[i + 1] == '/')
		{
			i += 2;
			if (!TryParseAuthorityRfc(s, ref i, isIri)) return false;
			return TryParsePathAbemptyRfc(s, ref i, isIri);
		}
		if (i < s.Length && s[i] == '/') return TryParsePathAbsoluteRfc(s, ref i, isIri);
		return TryParsePathNoschemeOrEmptyRfc(s, ref i, isIri);
	}

	// authority = [ userinfo "@" ] host [ ":" port ]
	private static bool TryParseAuthorityRfc(string s, ref int i, bool isIri)
	{
		var authorityStart = i;
		while (i < s.Length && s[i] != '/' && s[i] != '?' && s[i] != '#') i++;
		var authorityEnd = i;

		// Locate the first "@" to split userinfo from host
		var atPos = -1;
		for (var j = authorityStart; j < authorityEnd; j++)
		{
			if (s[j] == '@') { atPos = j; break; }
		}

		var hostStart = authorityStart;
		if (atPos >= 0)
		{
			// Validate userinfo: *( unreserved / pct-encoded / sub-delims / ":" )
			var k = authorityStart;
			while (k < atPos)
			{
				var c = s[k];
				if (IsUnreservedChar(c) || IsSubDelimChar(c) || c == ':') { k++; continue; }
				if (c == '%')
				{
					if (k + 2 < atPos && IsHexDigitChar(s[k + 1]) && IsHexDigitChar(s[k + 2])) { k += 3; continue; }
					return false;
				}
				if (isIri && IsUcscharBmp(c)) { k++; continue; }
				if (isIri && char.IsHighSurrogate(c) && k + 1 < atPos && char.IsLowSurrogate(s[k + 1])) { k += 2; continue; }
				return false;
			}
			hostStart = atPos + 1;
		}

		// Find port separator; IP-literals "[...]" may contain ":" internally
		var portColon = -1;
		if (hostStart < authorityEnd && s[hostStart] == '[')
		{
			var closeBracket = -1;
			for (var j = hostStart + 1; j < authorityEnd; j++)
			{
				if (s[j] == ']') { closeBracket = j; break; }
			}
			if (closeBracket < 0) return false;
			var afterBracket = closeBracket + 1;
			if (afterBracket < authorityEnd)
			{
				if (s[afterBracket] == ':') portColon = afterBracket;
				else return false;
			}
		}
		else
		{
			for (var j = hostStart; j < authorityEnd; j++)
			{
				if (s[j] == ':') { portColon = j; break; }
			}
		}

		var hostEnd = portColon >= 0 ? portColon : authorityEnd;
		if (!TryParseHostRfc(s, hostStart, hostEnd, isIri)) return false;

		// port = *DIGIT
		if (portColon >= 0)
		{
			for (var j = portColon + 1; j < authorityEnd; j++)
			{
				if (!IsDigitChar(s[j])) return false;
			}
		}

		return true;
	}

	// host = IP-literal / reg-name  (IPv4address is syntactically a subset of reg-name)
	private static bool TryParseHostRfc(string s, int start, int end, bool isIri)
	{
		if (start == end) return true; // empty host is valid

		if (s[start] == '[')
		{
			// IP-literal = "[" ( IPv6address / IPvFuture ) "]"
			if (s[end - 1] != ']') return false;
			var inner = s.Substring(start + 1, end - start - 2);
			if (inner.Length == 0) return false;
			if (inner[0] == 'v' || inner[0] == 'V') return TryParseIPvFutureRfc(inner);
			return IsValidIpv6(inner);
		}

		// reg-name = *( unreserved / pct-encoded / sub-delims )
		for (var i = start; i < end;)
		{
			var c = s[i];
			if (IsUnreservedChar(c) || IsSubDelimChar(c)) { i++; continue; }
			if (c == '%')
			{
				if (i + 2 < end && IsHexDigitChar(s[i + 1]) && IsHexDigitChar(s[i + 2])) { i += 3; continue; }
				return false;
			}
			if (isIri && IsUcscharBmp(c)) { i++; continue; }
			if (isIri && char.IsHighSurrogate(c) && i + 1 < end && char.IsLowSurrogate(s[i + 1])) { i += 2; continue; }
			return false;
		}
		return true;
	}

	// IPvFuture = "v" 1*HEXDIG "." 1*( unreserved / sub-delims / ":" )
	private static bool TryParseIPvFutureRfc(string s)
	{
		var i = 1; // s[0] is 'v'/'V'
		if (i >= s.Length || !IsHexDigitChar(s[i])) return false;
		while (i < s.Length && IsHexDigitChar(s[i])) i++;
		if (i >= s.Length || s[i] != '.') return false;
		i++;
		if (i >= s.Length) return false;
		while (i < s.Length)
		{
			var c = s[i];
			if (IsUnreservedChar(c) || IsSubDelimChar(c) || c == ':') { i++; continue; }
			return false;
		}
		return true;
	}

	// path-abempty = *( "/" segment )
	private static bool TryParsePathAbemptyRfc(string s, ref int i, bool isIri)
	{
		while (i < s.Length && s[i] == '/')
		{
			i++;
			ConsumeSegmentRfc(s, ref i, isIri, true);
		}
		return true;
	}

	// path-absolute = "/" [ segment-nz *( "/" segment ) ]
	private static bool TryParsePathAbsoluteRfc(string s, ref int i, bool isIri)
	{
		i++; // consume leading '/'
		if (i < s.Length && s[i] != '/' && s[i] != '?' && s[i] != '#')
		{
			var before = i;
			ConsumeSegmentRfc(s, ref i, isIri, true);
			if (i == before) return false; // segment-nz needs at least one pchar
			_ = TryParsePathAbemptyRfc(s, ref i, isIri);
		}
		return true;
	}

	// path-rootless = segment-nz *( "/" segment )  or  path-empty
	private static bool TryParsePathRootlessOrEmptyRfc(string s, ref int i, bool isIri)
	{
		if (i >= s.Length || s[i] == '?' || s[i] == '#') return true;
		var before = i;
		ConsumeSegmentRfc(s, ref i, isIri, true);
		if (i == before) return false;
		_ = TryParsePathAbemptyRfc(s, ref i, isIri);
		return true;
	}

	// path-noscheme = segment-nz-nc *( "/" segment )  or  path-empty
	// segment-nz-nc: 1*( unreserved / pct-encoded / sub-delims / "@" ) — no ":"
	private static bool TryParsePathNoschemeOrEmptyRfc(string s, ref int i, bool isIri)
	{
		if (i >= s.Length || s[i] == '?' || s[i] == '#') return true;
		var before = i;
		ConsumeSegmentRfc(s, ref i, isIri, false);
		if (i == before) return false;
		_ = TryParsePathAbemptyRfc(s, ref i, isIri);
		return true;
	}

	// segment = *pchar;  pchar = unreserved / pct-encoded / sub-delims / ":" / "@"
	private static void ConsumeSegmentRfc(string s, ref int i, bool isIri, bool allowColon)
	{
		while (i < s.Length)
		{
			var c = s[i];
			if (IsUnreservedChar(c) || IsSubDelimChar(c) || c == '@') { i++; continue; }
			if (allowColon && c == ':') { i++; continue; }
			if (c == '%')
			{
				if (i + 2 < s.Length && IsHexDigitChar(s[i + 1]) && IsHexDigitChar(s[i + 2])) { i += 3; continue; }
				break;
			}
			if (isIri && IsUcscharBmp(c)) { i++; continue; }
			if (isIri && char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1])) { i += 2; continue; }
			break;
		}
	}

	// query = *( pchar / "/" / "?" )  ;  fragment = *( pchar / "/" / "?" )
	// For IRI query, iprivate characters are also allowed.
	private static void ConsumeQueryOrFragmentRfc(string s, ref int i, bool isIri, bool isQuery)
	{
		while (i < s.Length)
		{
			var c = s[i];
			if (c == '/' || c == '?') { i++; continue; }
			if (isQuery && isIri && IsIprivateBmp(c)) { i++; continue; }
			if (isQuery && isIri && char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1])) { i += 2; continue; }
			var prev = i;
			ConsumeSegmentRfc(s, ref i, isIri, true);
			if (i == prev) break;
		}
	}

	private static bool CheckUriTemplate(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;

		// URI-Template = *( literals / expression )
		var i = 0;
		while (i < value.Length)
		{
			var c = value[i];
			if (c == '{')
			{
				if (!TryParseUriTemplateExpression(value, ref i)) return false;
				continue;
			}
			if (c == '%')
			{
				if (i + 2 < value.Length && IsHexDigitChar(value[i + 1]) && IsHexDigitChar(value[i + 2])) { i += 3; continue; }
				return false;
			}
			if (IsUriTemplateLiteralChar(c)) { i++; continue; }
			if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1])) { i += 2; continue; }
			return false;
		}

		return true;
	}

	// literals = any Unicode character except: CTL, SP, DQUOTE, "%" (aside from pct-encoded), "<", ">", "\", "^", "`", "{", "|", "}"
	private static bool IsUriTemplateLiteralChar(char c)
	{
		if (c >= '\x80') return IsUcscharBmp(c) || IsIprivateBmp(c);
		if (c <= '\x20' || c == '\x7F') return false;
		return c is not ('"' or '%' or '<' or '>' or '\\' or '^' or '`' or '{' or '|' or '}');
	}

	// expression = "{" [ operator ] variable-list "}"
	private static bool TryParseUriTemplateExpression(string s, ref int i)
	{
		i++; // consume '{'
		if (i < s.Length && s[i] is '+' or '#' or '.' or '/' or ';' or '?' or '&' or '=' or ',' or '!' or '@' or '|') i++;

		// variable-list = varspec *( "," varspec )
		while (true)
		{
			if (!TryParseUriTemplateVarname(s, ref i)) return false;

			// modifier-level4 = prefix / explode
			if (i < s.Length && s[i] == ':')
			{
				i++;
				// max-length = %x31-39 0*3DIGIT
				if (i >= s.Length || s[i] < '1' || s[i] > '9') return false;
				var start = i;
				i++;
				while (i < s.Length && IsDigitChar(s[i]) && i - start < 4) i++;
			}
			else if (i < s.Length && s[i] == '*') i++;

			if (i >= s.Length) return false;
			if (s[i] == ',') { i++; continue; }
			if (s[i] != '}') return false;
			i++;
			return true;
		}
	}

	// varname = varchar *( ["."] varchar )
	private static bool TryParseUriTemplateVarname(string s, ref int i)
	{
		if (!TryParseUriTemplateVarchar(s, ref i)) return false;
		while (i < s.Length)
		{
			if (s[i] == '.')
			{
				i++;
				if (!TryParseUriTemplateVarchar(s, ref i)) return false;
				continue;
			}
			if (!TryParseUriTemplateVarchar(s, ref i)) break;
		}
		return true;
	}

	// varchar = ALPHA / DIGIT / "_" / pct-encoded
	private static bool TryParseUriTemplateVarchar(string s, ref int i)
	{
		if (i >= s.Length) return false;
		var c = s[i];
		if (IsAlphaChar(c) || IsDigitChar(c) || c == '_') { i++; return true; }
		if (c == '%' && i + 2 < s.Length && IsHexDigitChar(s[i + 1]) && IsHexDigitChar(s[i + 2])) { i += 3; return true; }
		return false;
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

	// RFC 5321 section 4.1.2: Mailbox = Local-part "@" ( Domain / address-literal )
	private static bool CheckEmail(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var email = node.GetString()!;

		var i = 0;
		if (i < email.Length && email[i] == '"')
		{
			if (!TryParseSmtpQuotedString(email, ref i)) return false;
		}
		else if (!TryParseSmtpDotString(email, ref i)) return false;

		if (i >= email.Length || email[i] != '@') return false;
		i++;

		var domain = email[i..];
		if (domain.Length > 0 && domain[0] == '[') return IsValidAddressLiteral(domain);
		return IsValidSmtpDomain(domain);
	}

	// Quoted-string = DQUOTE *QcontentSMTP DQUOTE
	private static bool TryParseSmtpQuotedString(string s, ref int i)
	{
		i++; // consume opening quote
		while (i < s.Length)
		{
			var c = s[i];
			if (c == '"') { i++; return true; }
			if (c == '\\')
			{
				// quoted-pairSMTP = %d92 %d32-126
				if (i + 1 >= s.Length || s[i + 1] < '\x20' || s[i + 1] > '\x7E') return false;
				i += 2;
				continue;
			}
			// qtextSMTP = %d32-33 / %d35-91 / %d93-126
			if (c < '\x20' || c > '\x7E') return false;
			i++;
		}
		return false;
	}

	// Dot-string = Atom *("." Atom)
	private static bool TryParseSmtpDotString(string s, ref int i)
	{
		while (true)
		{
			var start = i;
			while (i < s.Length && IsAtextChar(s[i])) i++;
			if (i == start) return false;
			if (i >= s.Length || s[i] != '.') return true;
			i++;
		}
	}

	// atext = ALPHA / DIGIT / "!" / "#" / "$" / "%" / "&" / "'" / "*" / "+" / "-" / "/" / "=" / "?" / "^" / "_" / "`" / "{" / "|" / "}" / "~"
	private static bool IsAtextChar(char c) =>
		IsAlphaChar(c) || IsDigitChar(c) ||
		c is '!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '/' or '=' or '?' or '^' or '_' or '`' or '{' or '|' or '}' or '~';

	// Domain = sub-domain *("." sub-domain);  sub-domain = Let-dig [Ldh-str]
	private static bool IsValidSmtpDomain(string domain)
	{
		if (domain.Length == 0) return false;

		foreach (var label in domain.Split('.'))
		{
			if (label.Length == 0 || label.Length > 63) return false;
			if (label[0] == '-' || label[^1] == '-') return false;
			foreach (var c in label)
			{
				if (!IsAlphaChar(c) && !IsDigitChar(c) && c != '-') return false;
			}
		}

		return true;
	}

	// address-literal = "[" ( IPv4-address-literal / IPv6-address-literal ) "]"
	private static bool IsValidAddressLiteral(string domain)
	{
		if (domain.Length < 2 || domain[0] != '[' || domain[^1] != ']') return false;

		var content = domain.Substring(1, domain.Length - 2);
		if (content.StartsWith("IPv6:", StringComparison.OrdinalIgnoreCase)) return IsValidIpv6(content[5..]);
		return IsValidIpv4(content);
	}

	private static bool CheckIdnEmail(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var email = node.GetString()!;

		if (string.IsNullOrWhiteSpace(email)) return false;

		var atIndex = email.LastIndexOf('@');
		if (atIndex <= 0 || atIndex == email.Length - 1) return false;

		var local = email[..atIndex];
		var domain = email[(atIndex + 1)..];

		// Local-part: allow Unicode or quoted strings, basic validation only
		if (local.Length == 0) return false;
		if (local[0] == '"')
		{
			// Must end with quote
			if (local[^1] != '"') return false;
		}
		else
		{
			// Unquoted: disallow leading/trailing dots and consecutive dots
			if (local[0] == '.' || local[^1] == '.') return false;
			if (local.Contains("..")) return false;
		}

		// Domain: normalize and validate using CheckIdnHostName
		if (domain.Length == 0) return false;
		
		if (domain[0] == '[') return IsValidAddressLiteral(domain);

		return IsValidIdnHostName(domain);
	}

	private static bool CheckUuid(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;
		// Guid parsing trims surrounding whitespace
		return value.Length == 36 && Guid.TryParseExact(value, "D", out _);
	}

	private static bool CheckDate(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var dateString = node.GetString()!;
		return TryParseRfc3339FullDate(dateString);
	}

	private static bool IsAsciiDigits(string s)
	{
		foreach (var ch in s)
		{
			if (ch < '0' || ch > '9') return false;
		}
		return true;
	}

	private static bool IsLeapYearProlepticGregorian(int year)
	{
		return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
	}

	private static int GetDaysInMonthProlepticGregorian(int year, int month)
	{
		return month switch
		{
			1 => 31,
			2 => IsLeapYearProlepticGregorian(year) ? 29 : 28,
			3 => 31,
			4 => 30,
			5 => 31,
			6 => 30,
			7 => 31,
			8 => 31,
			9 => 30,
			10 => 31,
			11 => 30,
			12 => 31,
			_ => 0
		};
	}

	private static bool TryParseRfc3339FullDate(string datePart)
	{
		if (datePart.Length != 10) return false;
		if (datePart[4] != '-' || datePart[7] != '-') return false;

		var yearPart = datePart[..4];
		var monthPart = datePart.Substring(5, 2);
		var dayPart = datePart.Substring(8, 2);

		if (!IsAsciiDigits(yearPart) || !IsAsciiDigits(monthPart) || !IsAsciiDigits(dayPart)) return false;
		if (!int.TryParse(yearPart, NumberStyles.None, CultureInfo.InvariantCulture, out var year)) return false;
		if (!int.TryParse(monthPart, NumberStyles.None, CultureInfo.InvariantCulture, out var month)) return false;
		if (!int.TryParse(dayPart, NumberStyles.None, CultureInfo.InvariantCulture, out var day)) return false;

		if (month < 1 || month > 12) return false;
		var daysInMonth = GetDaysInMonthProlepticGregorian(year, month);
		if (day < 1 || day > daysInMonth) return false;

		return true;
	}

	// Without a date, a leap second can't be known to exist, so one isn't allowed.
	private static bool CheckTime(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return CheckTimeString(node.GetString()!, false);
	}

	internal static bool CheckTimeString(string timeString, bool allowLeapSecond)
	{
		// Must contain offset (Z or +/-HH:MM)
		if (!timeString.EndsWith("Z", StringComparison.OrdinalIgnoreCase) && 
		    timeString.IndexOf('+') < 0 && timeString.IndexOf('-') < 0)
			return false;

		// Parse the time and offset parts
		string timePart;
		string offsetPart;

		if (timeString.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
		{
			timePart = timeString[..^1];
			offsetPart = timeString[^1..];
		}
		else
		{
			var lastPlus = timeString.LastIndexOf('+');
			var lastMinus = timeString.LastIndexOf('-');
			var offsetIndex = Math.Max(lastPlus, lastMinus);
			if (offsetIndex <= 0) return false;

			timePart = timeString[..offsetIndex];
			offsetPart = timeString[offsetIndex..];
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
		if (isLeapSecond && !allowLeapSecond) return false;
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
			
			var off = offsetPart[1..].Split(':');
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

	// RFC 9557 section 4.1: date-time-ext = date-time suffix
	private static bool CheckDateTime(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		var value = node.GetString()!;
		var suffixStart = value.IndexOf('[');
		if (suffixStart < 0) return CheckDateTimeString(value);

		return CheckDateTimeString(value[..suffixStart]) && DateTimeSuffixRegex().IsMatch(value[suffixStart..]);
	}

	internal static bool CheckDateTimeString(string dateString)
	{
		var match = DateTimeRegex().Match(dateString);
		if (!match.Success) return false;

		// Extract groups: 2=date, 3=separator, 4=time, 5=offset
		var datePart = match.Groups[2].Value; // YYYY-MM-DD
		var timePart = match.Groups[4].Value; // HH:MM:SS[.fraction]
		var offsetPart = match.Groups[5].Value; // Z or ±HH:MM

		if (!TryParseRfc3339FullDate(datePart)) return false;

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
			var off = offsetPart[1..].Split(':');
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
		if (value[0] == '.' || value[^1] == '.') return false;

		// Check basic pattern
		if (!HostnameRegex().IsMatch(value)) return false;

		// Split into labels and validate each
		var labels = value.Split('.');
		var hasALabel = false;
		for (var i = 0; i < labels.Length; i++)
		{
			var label = labels[i];

			// Each label max 63 chars
			if (label.Length > 63) return false;

			// RFC 5891 section 4.2.3.1: -- in positions 3-4 (index 2-3) is reserved for A-labels
			if (label.Length < 4 || label[2] != '-' || label[3] != '-') continue;
			if (!label.StartsWith("xn--", StringComparison.OrdinalIgnoreCase)) return false;

			if (!TryGetULabel(label, out var uLabel) || !IsValidULabel(uLabel)) return false;
			labels[i] = uLabel;
			hasALabel = true;
		}

		return !hasALabel || SatisfiesBidiRule(labels);
	}

	private static bool CheckIdnHostName(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return IsValidIdnHostName(node.GetString()!);
	}

	private static bool IsValidIdnHostName(string value)
	{
		// Empty or contains underscore
		if (string.IsNullOrEmpty(value) || value.IndexOf('_') >= 0) return false;

		// RFC 3490 section 3.1 (and UTS 46): ideographic, fullwidth, and halfwidth ideographic full stops separate labels
		value = value.Replace('\u3002', '.').Replace('\uFF0E', '.').Replace('\uFF61', '.');

		// Leading or trailing dot
		if (value[0] == '.' || value[^1] == '.') return false;

		// Split into labels and validate each
		var labels = value.Split('.');
		var asciiLength = labels.Length - 1;
		for (var i = 0; i < labels.Length; i++)
		{
			if (!TryGetULabel(labels[i], out var uLabel)) return false;
			if (!IsValidULabel(uLabel)) return false;

			// RFC 5890 section 2.3.2.1: length limits apply to the A-label form
			var aLabelLength = GetALabelLength(uLabel);
			if (aLabelLength > 63) return false;
			asciiLength += aLabelLength;

			labels[i] = uLabel;
		}

		// RFC 1034 section 3.1: 255 octets on the wire leaves 253 characters in text form
		if (asciiLength > 253) return false;

		return SatisfiesBidiRule(labels);
	}

	private static int GetALabelLength(string uLabel)
	{
		var mapped = uLabel.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
		foreach (var ch in mapped)
		{
			if (ch >= '\x80') return 4 + GetPunycodeLength(mapped);
		}

		return mapped.Length;
	}

	// RFC 3492 section 6.3, counting the output rather than producing it
	private static int GetPunycodeLength(string input)
	{
		const int punycodeBase = 36, tMin = 1, tMax = 26;

		var codePoints = new int[input.Length];
		var count = 0;
		for (var i = 0; i < input.Length; i++)
		{
			if (char.IsHighSurrogate(input[i]) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
			{
				codePoints[count++] = char.ConvertToUtf32(input[i], input[i + 1]);
				i++;
			}
			else codePoints[count++] = input[i];
		}

		var basicCount = 0;
		for (var i = 0; i < count; i++)
		{
			if (codePoints[i] < 0x80) basicCount++;
		}

		var length = basicCount > 0 ? basicCount + 1 : 0;
		long n = 0x80, delta = 0;
		var bias = 72;
		var handled = basicCount;
		while (handled < count)
		{
			var m = long.MaxValue;
			for (var i = 0; i < count; i++)
			{
				if (codePoints[i] >= n && codePoints[i] < m) m = codePoints[i];
			}

			delta += (m - n) * (handled + 1);
			n = m;
			for (var i = 0; i < count; i++)
			{
				if (codePoints[i] < n) delta++;
				if (codePoints[i] != n) continue;

				var q = delta;
				for (var k = punycodeBase; ; k += punycodeBase)
				{
					var t = k <= bias ? tMin : k >= bias + tMax ? tMax : k - bias;
					if (q < t) break;
					length++;
					q = (q - t) / (punycodeBase - t);
				}
				length++;

				bias = AdaptPunycodeBias(delta, handled + 1, handled == basicCount);
				delta = 0;
				handled++;
			}

			delta++;
			n++;
		}

		return length;
	}

	private static int AdaptPunycodeBias(long delta, int pointCount, bool isFirst)
	{
		delta = isFirst ? delta / 700 : delta / 2;
		delta += delta / pointCount;

		var k = 0;
		while (delta > 35 * 26 / 2)
		{
			delta /= 35;
			k += 36;
		}

		return (int)(k + 36 * delta / (delta + 38));
	}

	// RFC 5891 sections 5.3-5.4: a label that carries the ACE prefix once mapped (UTS 46) is an A-label.
	// It must decode as Punycode, and the decoded U-label must encode back to the same (lowercased) A-label.
	private static bool TryGetULabel(string label, out string uLabel)
	{
		uLabel = label;
		try
		{
			var mapped = label.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
			if (!mapped.StartsWith("xn--", StringComparison.Ordinal)) return true;
			if (mapped.Length > 63) return false;

			var idn = new IdnMapping { UseStd3AsciiRules = true };
			uLabel = idn.GetUnicode(mapped);
			return string.Equals(idn.GetAscii(uLabel), mapped, StringComparison.Ordinal);
		}
		catch (ArgumentException)
		{
			return false;
		}
	}

	private static bool IsValidULabel(string label)
	{
		if (label.Length == 0) return false;

		// Each label max 63 chars
		if (label.Length > 63) return false;

		// Cannot start or end with hyphen
		if (label[0] == '-' || label[^1] == '-') return false;

		// Cannot have -- in positions 3-4 (index 2-3)
		if (label.Length >= 4 && label[2] == '-' && label[3] == '-')
		{
			return false;
		}

		// The only ASCII allowed is letters, digits, and hyphen; non-ASCII must be letters, marks, or digits
		foreach (var ch in label)
		{
			if (ch < '\x80')
			{
				if (!IsAlphaChar(ch) && !IsDigitChar(ch) && ch != '-') return false;
			}
			else if (IsDisallowedIdnCategory(ch)) return false;
		}

		// Check for disallowed starting characters (combining marks)
		if (label.Length > 0)
		{
			var firstChar = label[0];
			var category = char.GetUnicodeCategory(firstChar);
			
			// Cannot start with combining marks
			if (category == UnicodeCategory.SpacingCombiningMark ||
			    category == UnicodeCategory.NonSpacingMark ||
			    category == UnicodeCategory.EnclosingMark)
			{
				return false;
			}
		}

		// IDNA2008 contextual rules for specific characters
		for (var i = 0; i < label.Length; i++)
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
				var hasRequiredScript = false;
				foreach (var c in label)
				{
					// Hiragana: U+3040-U+309F, Katakana: U+30A0-U+30FF (aside from U+30FB itself), Han: U+4E00-U+9FFF
					if (c == '\u30FB') continue;
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
				if (char.GetUnicodeCategory(prev) != UnicodeCategory.NonSpacingMark || !IsVirama(prev))
				{
					return false;
				}
			}

			// U+200C ZERO WIDTH NON-JOINER: must be preceded by Virama or sit between joining letters
			if (ch == '‌')
			{
				if (i > 0 && IsVirama(label[i - 1])) continue;
				if (!HasJoiningNeighbors(label, i)) return false;
			}
		}

		return true;
	}

	// RFC 5892 appendix A.1: (Joining_Type:{L,D})(Joining_Type:T)* ZWNJ (Joining_Type:T)*(Joining_Type:{R,D})
	private static bool HasJoiningNeighbors(string label, int index)
	{
		var before = index - 1;
		while (before >= 0 && GetJoiningType(label[before]) == 'T') before--;
		if (before < 0 || GetJoiningType(label[before]) is not ('L' or 'D')) return false;

		var after = index + 1;
		while (after < label.Length && GetJoiningType(label[after]) == 'T') after++;
		return after < label.Length && GetJoiningType(label[after]) is 'R' or 'D';
	}

	// An approximation of the Unicode Joining_Type property for the Arabic, Syriac, NKo, and Mongolian scripts
	private static char GetJoiningType(char ch)
	{
		if (ch is '‌' or '‍' or 'ء' or 'ٴ') return 'U';
		if (char.GetUnicodeCategory(ch) is UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark or UnicodeCategory.Format) return 'T';
		if (!char.IsLetter(ch)) return 'U';
		if (IsRightJoining(ch)) return 'R';
		if (ch is (>= 'ؠ' and <= 'ۿ') or (>= 'ܐ' and <= 'ݿ') or (>= 'ߊ' and <= 'ߪ') or
		    (>= 'ࡠ' and <= 'ࣉ') or (>= 'ᠠ' and <= 'ᡸ'))
			return 'D';

		return 'U';
	}

	private static bool IsRightJoining(char ch) =>
		ch is (>= 'آ' and <= 'إ') or 'ا' or 'ة' or (>= 'د' and <= 'ز') or 'و' or
			(>= 'ٱ' and <= 'ٳ') or (>= 'ٵ' and <= 'ٷ') or (>= 'ڈ' and <= 'ڙ') or
			'ۀ' or (>= 'ۃ' and <= 'ۋ') or 'ۍ' or 'ۏ' or 'ے' or 'ۓ' or 'ە' or
			'ۮ' or 'ۯ' or (>= 'ݙ' and <= 'ݛ') or 'ݫ' or 'ݬ' or 'ݱ' or 'ݳ' or
			'ݴ' or 'ݸ' or 'ݹ' or 'ܐ' or (>= 'ܕ' and <= 'ܙ') or 'ܞ' or 'ܨ' or
			'ܪ' or 'ܬ' or 'ܯ' or 'ݍ';

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

	// RFC 5892: punctuation, symbols, spaces, and controls are DISALLOWED, and section 2.6 lists exceptions both ways
	private static bool IsDisallowedIdnCategory(char ch)
	{
		if (ch is '\u00B7' or '\u0375' or '\u05F3' or '\u05F4' or '\u06FD' or '\u06FE' or '\u0F0B' or '\u3007' or '\u30FB') return false;
		if (ch is '\u0640' or '\u07FA' or '\u302E' or '\u302F' or (>= '\u3031' and <= '\u3035') or '\u303B') return true;

		return char.GetUnicodeCategory(ch) switch
		{
			UnicodeCategory.ConnectorPunctuation or
			UnicodeCategory.DashPunctuation or
			UnicodeCategory.OpenPunctuation or
			UnicodeCategory.ClosePunctuation or
			UnicodeCategory.InitialQuotePunctuation or
			UnicodeCategory.FinalQuotePunctuation or
			UnicodeCategory.OtherPunctuation or
			UnicodeCategory.MathSymbol or
			UnicodeCategory.CurrencySymbol or
			UnicodeCategory.ModifierSymbol or
			UnicodeCategory.OtherSymbol or
			UnicodeCategory.SpaceSeparator or
			UnicodeCategory.LineSeparator or
			UnicodeCategory.ParagraphSeparator or
			UnicodeCategory.Control or
			UnicodeCategory.PrivateUse => true,
			_ => false
		};
	}

	// RFC 5893 section 2, using BMP code point ranges as an approximation of the Bidi_Class property
	private static bool SatisfiesBidiRule(string[] labels)
	{
		var isBidiDomain = false;
		foreach (var label in labels)
		{
			foreach (var ch in label)
			{
				if (!IsRtlLetter(ch) && !IsArabicNumber(ch)) continue;
				isBidiDomain = true;
				break;
			}
		}
		if (!isBidiDomain) return true;

		foreach (var label in labels)
		{
			// Rule 1: the first character must be L, R, or AL
			var isRtl = IsRtlLetter(label[0]);
			if (!isRtl && !IsLtrLetter(label[0])) return false;

			var hasEuropeanNumber = false;
			var hasArabicNumber = false;
			foreach (var ch in label)
			{
				// Rule 2: no L in an RTL label; rule 5: no R, AL, or AN in an LTR label
				if (isRtl ? IsLtrLetter(ch) : IsRtlLetter(ch) || IsArabicNumber(ch)) return false;
				if (IsEuropeanNumber(ch)) hasEuropeanNumber = true;
				if (IsArabicNumber(ch)) hasArabicNumber = true;
			}

			// Rule 4: an RTL label can't mix EN and AN
			if (isRtl && hasEuropeanNumber && hasArabicNumber) return false;

			// Rules 3 and 6: the last character, ignoring trailing NSMs
			var end = label.Length - 1;
			while (end > 0 && char.GetUnicodeCategory(label[end]) is UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark) end--;
			var last = label[end];
			if (isRtl
				    ? !IsRtlLetter(last) && !IsEuropeanNumber(last) && !IsArabicNumber(last)
				    : !IsLtrLetter(last) && !IsEuropeanNumber(last))
				return false;
		}

		return true;
	}

	// Hebrew, Arabic, Syriac, Thaana, NKo, Samaritan, Mandaic, and the RTL presentation forms
	private static bool IsRtlLetter(char ch) =>
		char.IsLetter(ch) &&
		((ch >= '\u0590' && ch <= '\u08FF') || (ch >= '\uFB1D' && ch <= '\uFDFF') || (ch >= '\uFE70' && ch <= '\uFEFF'));

	private static bool IsLtrLetter(char ch) => char.IsLetter(ch) && !IsRtlLetter(ch);

	private static bool IsArabicNumber(char ch) =>
		(ch >= '\u0600' && ch <= '\u0605') || (ch >= '\u0660' && ch <= '\u0669') || ch == '\u066B' || ch == '\u066C' || ch == '\u06DD';

	private static bool IsEuropeanNumber(char ch) =>
		(ch >= '0' && ch <= '9') || (ch >= '\u06F0' && ch <= '\u06F9') || (ch >= '\uFF10' && ch <= '\uFF19');

	private static bool CheckIpv4(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return IsValidIpv4(node.GetString()!);
	}

	private static bool IsValidIpv4(string value)
	{
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

		return IsValidIpv6(node.GetString()!);
	}

	// RFC 4291 section 2.2 (as given by the IPv6address rule in RFC 3986 section 3.2.2)
	private static bool IsValidIpv6(string value)
	{
		var compression = value.IndexOf("::", StringComparison.Ordinal);
		if (compression < 0)
			return TryCountIpv6Groups(value, true, out var count) && count == 8;

		if (value.IndexOf("::", compression + 1, StringComparison.Ordinal) >= 0) return false;

		return TryCountIpv6Groups(value[..compression], false, out var headCount) &&
		       TryCountIpv6Groups(value[(compression + 2)..], true, out var tailCount) &&
		       headCount + tailCount <= 7;
	}

	// h16 = 1*4HEXDIG;  ls32 = ( h16 ":" h16 ) / IPv4address
	private static bool TryCountIpv6Groups(string value, bool allowTrailingIpv4, out int count)
	{
		count = 0;
		if (value.Length == 0) return true;

		var groups = value.Split(':');
		for (var i = 0; i < groups.Length; i++)
		{
			var group = groups[i];
			if (allowTrailingIpv4 && i == groups.Length - 1 && group.IndexOf('.') >= 0)
			{
				if (!IsValidIpv4(group)) return false;
				count += 2;
				continue;
			}

			if (group.Length is 0 or > 4) return false;
			foreach (var ch in group)
			{
				if (!IsHexDigitChar(ch)) return false;
			}
			count++;
		}

		return true;
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
		for (var i = 0; i < pattern.Length - 1; i++)
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

		if (!TryTranslateEcmaPattern(pattern, out var translated)) return false;

		try
		{
			_ = RegexOrPattern.CreateRegex(translated, false);
			return true;
		}
		catch
		{
			return false;
		}
	}

	// Rejects group syntax that .NET accepts but ECMA-262 doesn't, and rewrites the ECMA-262 empty classes
	// `[]` and `[^]`, which .NET can't parse, into equivalents it can.
	private static bool TryTranslateEcmaPattern(string pattern, out string translated)
	{
		var builder = new StringBuilder(pattern.Length);
		var inClass = false;
		for (var i = 0; i < pattern.Length; i++)
		{
			var c = pattern[i];
			if (c == '\\')
			{
				builder.Append(c);
				if (i + 1 < pattern.Length) builder.Append(pattern[++i]);
				continue;
			}

			if (inClass)
			{
				if (c == ']') inClass = false;
				builder.Append(c);
				continue;
			}

			if (c == '[')
			{
				if (i + 1 < pattern.Length && pattern[i + 1] == ']')
				{
					builder.Append("(?!)");
					i++;
					continue;
				}
				if (i + 2 < pattern.Length && pattern[i + 1] == '^' && pattern[i + 2] == ']')
				{
					builder.Append(@"[\s\S]");
					i += 2;
					continue;
				}
				inClass = true;
			}
			else if (c == '(' && i + 1 < pattern.Length && pattern[i + 1] == '?' && !IsEcmaGroupPrefix(pattern, i + 2))
			{
				translated = string.Empty;
				return false;
			}

			builder.Append(c);
		}

		translated = builder.ToString();
		return true;
	}

	// After "(?", ECMA-262 allows ":", "=", "!", "<=", "<!", "<name>", and modifiers such as "i:" or "i-ms:".
	private static bool IsEcmaGroupPrefix(string pattern, int i)
	{
		if (i >= pattern.Length) return false;
		if (pattern[i] is ':' or '=' or '!' or '<') return true;

		var start = i;
		while (i < pattern.Length && pattern[i] is 'i' or 'm' or 's') i++;
		var hasAdded = i > start;
		if (i < pattern.Length && pattern[i] == '-')
		{
			i++;
			start = i;
			while (i < pattern.Length && pattern[i] is 'i' or 'm' or 's') i++;
			if (i == start) return false;
		}
		else if (!hasAdded) return false;

		return i < pattern.Length && pattern[i] == ':';
	}
}