using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `$defs`.
	/// </summary>
	[SchemaPriority(long.MinValue + 1)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(DefsKeywordJsonConverter))]
	public class DefsKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "$defs";

		/// <summary>
		/// The collection of schema definitions.
		/// </summary>
		public IReadOnlyDictionary<string, JsonSchema> Definitions { get; }

		/// <summary>
		/// Creates a new <see cref="DefsKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of schema definitions.</param>
		public DefsKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Definitions = values;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.IsValid = true;
			context.Ignore = true;
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			return Definitions.TryGetValue(value, out var schema) ? schema : null;
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Definitions.Values)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}
	}

	internal class DefsKeywordJsonConverter : JsonConverter<DefsKeyword>
	{
		public override DefsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);
			return new DefsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, DefsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DefsKeyword.Name);
			writer.WriteStartObject();
			foreach (var kvp in value.Definitions)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}