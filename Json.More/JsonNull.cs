using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.More;

/// <summary>
/// Provides a <see cref="JsonNode"/> instance to use as null.
/// </summary>
/// <remarks>
/// .Net has decided to unify JSON null with .Net null.  This decision has a number
/// of consequences.  This value is provided to get around some of these issues.
///
/// This class *should not be used as a property value*, but rather as a signal to indicate
/// that null was explicitly passed rather than a property missing.
///
/// See https://github.com/dotnet/runtime/issues/68128 for more information.
/// </remarks>
[JsonConverter(typeof(JsonNullConverter))]
public class JsonNull
{
	/// <summary>
	/// Provides a static instance of this class wrapped in a <see cref="JsonNode"/>.
	/// </summary>
	public static JsonNode SignalNode { get; } = JsonValue.Create(new JsonNull(), JsonNullSerializationContext.Default.JsonNull)!;

	private JsonNull() { }
}

internal class JsonNullConverter : JsonConverter<JsonNull>
{
	public override JsonNull Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException("This should never be deserialized directly.");
	}

	public override void Write(Utf8JsonWriter writer, JsonNull value, JsonSerializerOptions options)
	{
		writer.WriteNullValue();
	}
}

/// <summary>
/// Provides a serialization context for <see cref="JsonNull"/>.
/// </summary>
[JsonSerializable(typeof(JsonNull))]
internal partial class JsonNullSerializationContext : JsonSerializerContext
{

}