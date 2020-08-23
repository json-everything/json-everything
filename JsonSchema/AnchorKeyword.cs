using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaPriority(long.MinValue + 2)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(VocabularyRegistry.Core201909Id)]
	[JsonConverter(typeof(AnchorKeywordJsonConverter))]
	public class AnchorKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$anchor";
		internal static readonly Regex AnchorPattern = new Regex("^[A-Za-z][-A-Za-z0-9.:_]*$");

		public string Anchor { get; }

		public AnchorKeyword(string anchor)
		{
			Anchor = anchor;
		}

		public void Validate(ValidationContext context)
		{
			context.IsValid = true;
		}
	}

	public class AnchorKeywordJsonConverter : JsonConverter<AnchorKeyword>
	{
		public override AnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var uriString = reader.GetString();
			if (!AnchorKeyword.AnchorPattern.IsMatch(uriString))
				throw new JsonException("Expected anchor format");

			return new AnchorKeyword(uriString);
		}

		public override void Write(Utf8JsonWriter writer, AnchorKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(AnchorKeyword.Name, value.Anchor);
		}
	}
}