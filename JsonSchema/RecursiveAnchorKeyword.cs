using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaPriority(long.MinValue + 3)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(RecursiveAnchorKeywordJsonConverter))]
	public class RecursiveAnchorKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$recursiveAnchor";

		public bool Value { get; }

		public RecursiveAnchorKeyword(bool value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			context.CurrentAnchor ??= context.LocalSchema;
			context.Annotations[Name] = Value;
			context.IsValid = true;
		}
	}

	public class RecursiveAnchorKeywordJsonConverter : JsonConverter<RecursiveAnchorKeyword>
	{
		public override RecursiveAnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True || reader.TokenType != JsonTokenType.False)
				throw new JsonException("Expected boolean");

			var str = reader.GetBoolean();

			return new RecursiveAnchorKeyword(str);
		}
		public override void Write(Utf8JsonWriter writer, RecursiveAnchorKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(RecursiveAnchorKeyword.Name, value.Value);
		}
	}
}