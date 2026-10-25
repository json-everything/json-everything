using System.Text.Json;

namespace Json.Schema.Keywords.Draft06;

/// <summary>
/// Formats whose validation differs for draft 6 through draft 2020-12.
/// </summary>
internal static class Formats
{
	/// <summary>
	/// Defines the `date-time` format, which doesn't allow RFC 9557 suffixes.
	/// </summary>
	public static readonly Format DateTime = new PredicateFormat("date-time", CheckDateTime);

	private static bool CheckDateTime(JsonElement node)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String) return true;

		return Json.Schema.Formats.CheckDateTimeString(node.GetString()!);
	}
}
