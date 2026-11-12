using System.Text.Json;

namespace Json.Schema.Keywords.Draft07;

/// <summary>
/// Formats whose validation differs for draft 6 through draft 2020-12.
/// </summary>
public static class Formats
{
	/// <summary>
	/// Defines the `time` format, which allows leap seconds.
	/// </summary>
	public static readonly Format Time = new PredicateFormat("time", CheckTime);

	/// <summary>
	/// Defines the `hostname` format, which also enforces the IDNA A-label rules of RFC 5891.
	/// </summary>
	public static readonly Format Hostname = new PredicateFormat("hostname", CheckHostName);

	private static bool CheckTime(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Json.Schema.Formats.CheckTimeString(node.GetString()!, allowLeapSecond: true);
	}

	private static bool CheckHostName(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Json.Schema.Formats.CheckHostNameString(node.GetString()!, enforceIdnaRules: true);
	}
}
