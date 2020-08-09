using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword("$id")]
	[SchemaPriority(long.MinValue)]
	[JsonConverter(typeof(IdKeywordJsonConverter))]
	public class IdKeyword : IJsonSchemaKeyword
	{
		public Uri Id { get; }

		public IdKeyword(Uri id)
		{
			Id = id;
		}

		public ValidationResults Validate(ValidationContext context)
		{
			context.CurrentUri = Id;
			return ValidationResults.Null;
		}
	}

	public class IdKeywordJsonConverter : JsonConverter<IdKeyword>
	{
		public override IdKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var uriString = reader.GetString();
			if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
				throw new JsonException("Expected absolute URI");

			return new IdKeyword(uri);
		}

		public override void Write(Utf8JsonWriter writer, IdKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString("$id", value.Id.OriginalString);
		}
	}
}