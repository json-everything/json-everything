using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Represents a single ordering characteristic.
/// </summary>
public class OrderingSpecifier
{
	/// <summary>
	/// Gets a pointer to the value.
	/// </summary>
	[JsonPropertyName("by")]
	public JsonPointer By { get; }
	/// <summary>
	/// Gets the direction of the ordering.
	/// </summary>
	[JsonPropertyName("direction")]
	public Direction Direction { get; }
	/// <summary>
	/// For strings, gets the culture to use.
	/// </summary>
	[JsonPropertyName("culture")]
	[JsonConverter(typeof(CustomCultureInfoConverter))]
	public CultureInfo Culture { get; }
	/// <summary>
	/// For strings, gets whether to consider case sensitivity.
	/// </summary>
	[JsonPropertyName("ignoreCase")]
	public bool IgnoreCase { get; }

	/// <summary>
	/// Creates a new <see cref="OrderingSpecifier"/>.
	/// </summary>
	/// <param name="by">A pointer to the value</param>
	/// <param name="direction">The direction of the ordering</param>
	/// <param name="culture">For strings, gets the culture to use</param>
	/// <param name="ignoreCase">For strings, gets whether to consider case sensitivity</param>
	public OrderingSpecifier(JsonPointer by, Direction direction = Direction.Ascending, CultureInfo? culture = null, bool ignoreCase = false)
	{
		By = by;
		Direction = direction;
		Culture = culture ?? CultureInfo.InvariantCulture;
		IgnoreCase = ignoreCase;
	}
}

internal class CustomCultureInfoConverter : Json.More.AotCompatibleJsonConverter<CultureInfo>
{
	public override CultureInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var text = reader.GetString();
		if (text == "none") return CultureInfo.InvariantCulture;
		
		return CultureInfo.GetCultureInfo(text!);
	}

	public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}