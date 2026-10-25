using System.Text.Json;

namespace Json.Schema.Keywords.Draft07;

/// <summary>
/// Formats whose validation differs for draft 6 through draft 2020-12.
/// </summary>
internal static class Formats
{
	/// <summary>
	/// Defines the `time` format, which allows leap seconds.
	/// </summary>
	public static readonly Format Time = new PredicateFormat("time", CheckTime);

	private static bool CheckTime(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Json.Schema.Formats.CheckTimeString(node.GetString()!, allowLeapSecond: true);
	}
}
