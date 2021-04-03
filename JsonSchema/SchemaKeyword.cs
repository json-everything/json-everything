using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `$schema`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaPriority(long.MinValue)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[Vocabulary(Vocabularies.Core202012Id)]
	[JsonConverter(typeof(SchemaKeywordJsonConverter))]
	public class SchemaKeyword : IJsonSchemaKeyword, IEquatable<SchemaKeyword>
	{
		internal const string Name = "$schema";

		/// <summary>
		/// The meta-schema ID.
		/// </summary>
		public Uri Schema { get; }

		/// <summary>
		/// Creates a new <see cref="SchemaKeyword"/>.
		/// </summary>
		/// <param name="schema">The meta-schema ID.</param>
		public SchemaKeyword(Uri schema)
		{
			Schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.Options.Log.EnterKeyword(Name);
			var metaSchema = context.Options.SchemaRegistry.Get(Schema);
			if (metaSchema == null)
			{
				context.Message = $"Could not resolve schema `{Schema.OriginalString}` for meta-schema validation";
				context.IsValid = false;
				return;
			}

			var vocabularyKeyword = metaSchema.Keywords.OfType<VocabularyKeyword>().FirstOrDefault();
			if (vocabularyKeyword != null) 
				context.MetaSchemaVocabs = vocabularyKeyword.Vocabulary;

			if (!context.Options.ValidateMetaSchema)
			{
				context.IsValid = true;
				return;
			}

			var schemaAsJson = JsonDocument.Parse(JsonSerializer.Serialize(context.LocalSchema)).RootElement;
			var newOptions = ValidationOptions.From(context.Options);
			newOptions.ValidateMetaSchema = false;
			var results = metaSchema.Validate(schemaAsJson, newOptions);

			context.IsValid = results.IsValid;
			if (!context.IsValid)
				context.Message = $"Cannot validate current schema against meta-schema `{Schema.OriginalString}`";
			context.Options.Log.ExitKeyword(Name, context.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(SchemaKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Schema, other.Schema);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as SchemaKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Schema.GetHashCode();
		}
	}

	internal class SchemaKeywordJsonConverter : JsonConverter<SchemaKeyword>
	{
		public override SchemaKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var uriString = reader.GetString();
			if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
				throw new JsonException("Expected absolute URI");

			return new SchemaKeyword(uri);
		}

		public override void Write(Utf8JsonWriter writer, SchemaKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(SchemaKeyword.Name, value.Schema.OriginalString);
		}
	}
}