using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `$id`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaPriority(long.MinValue + 1)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(IdKeywordJsonConverter))]
	public class IdKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$id";

		/// <summary>
		/// The ID.
		/// </summary>
		public Uri Id { get; }

		/// <summary>
		/// Creates a new <see cref="IdKeyword"/>.
		/// </summary>
		/// <param name="id">The ID.</param>
		public IdKeyword(Uri id)
		{
			Id = id;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.ParentContext.CurrentUri = UpdateUri(context.CurrentUri);
			context.IsValid = true;
		}

		internal Uri UpdateUri(Uri currentUri)
		{
			return currentUri == null ? Id : new Uri(currentUri, Id);
		}
	}

	internal class IdKeywordJsonConverter : JsonConverter<IdKeyword>
	{
		public override IdKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var uriString = reader.GetString();
			if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
				throw new JsonException("Expected URI");

			return new IdKeyword(uri);
		}

		public override void Write(Utf8JsonWriter writer, IdKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(IdKeyword.Name, value.Id.OriginalString);
		}
	}
}