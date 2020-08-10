using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(DefaultKeywordJsonConverter))]
	public class DefaultKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "default";

		public JsonElement Value { get; }

		public DefaultKeyword(JsonElement value)
		{
			Value = value.Clone();
		}

		public ValidationResults Validate(ValidationContext context)
		{
			return ValidationResults.Annotation(context, Value);
		}
	}

	public class DefaultKeywordJsonConverter : JsonConverter<DefaultKeyword>
	{
		public override DefaultKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var element = JsonDocument.ParseValue(ref reader).RootElement;

			return new DefaultKeyword(element);
		}
		public override void Write(Utf8JsonWriter writer, DefaultKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DefaultKeyword.Name);
			value.Value.WriteTo(writer);
		}
	}
}