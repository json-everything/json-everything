using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `not`.
	/// </summary>
	[Applicator]
	[SchemaPriority(20)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(NotKeywordJsonConverter))]
	public class NotKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "not";

		/// <summary>
		/// The schema to not match.
		/// </summary>
		public JsonSchema Schema { get; }

		/// <summary>
		/// Creates a new <see cref="NotKeyword"/>.
		/// </summary>
		/// <param name="value">The schema to not match.</param>
		public NotKeyword(JsonSchema value)
		{
			Schema = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			var subContext = ValidationContext.From(context,
				subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create(Name)));
			Schema.ValidateSubschema(subContext);
			context.NestedContexts.Add(subContext);
			context.IsValid = !subContext.IsValid;
			context.ConsolidateAnnotations();
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			return value == null ? Schema : null;
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			Schema.RegisterSubschemas(registry, currentUri);
		}
	}

	internal class NotKeywordJsonConverter : JsonConverter<NotKeyword>
	{
		public override NotKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new NotKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, NotKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(NotKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}