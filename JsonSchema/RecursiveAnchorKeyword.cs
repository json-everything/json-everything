using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `$recursiveAnchor`.
	/// </summary>
	[SchemaPriority(long.MinValue + 3)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(RecursiveAnchorKeywordJsonConverter))]
	public class RecursiveAnchorKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$recursiveAnchor";

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.CurrentAnchor ??= context.LocalSchema;
			context.SetAnnotation(Name, true);
			context.IsValid = true;
		}
	}

	internal class RecursiveAnchorKeywordJsonConverter : JsonConverter<RecursiveAnchorKeyword>
	{
		public override RecursiveAnchorKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.True)
				throw new JsonException("Expected true");

			reader.GetBoolean();

			return new RecursiveAnchorKeyword();
		}
		public override void Write(Utf8JsonWriter writer, RecursiveAnchorKeyword value, JsonSerializerOptions options)
		{
			writer.WriteBoolean(RecursiveAnchorKeyword.Name, true);
		}
	}
}