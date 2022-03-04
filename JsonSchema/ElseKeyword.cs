using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `else`.
	/// </summary>
	[Applicator]
	[SchemaKeyword(Name)]
	[SchemaPriority(10)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(ElseKeywordJsonConverter))]
	public class ElseKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<ElseKeyword>
	{
		internal const string Name = "else";

		/// <summary>
		/// The schema to match.
		/// </summary>
		public JsonSchema Schema { get; }

		/// <summary>
		/// Creates a new <see cref="ElseKeyword"/>.
		/// </summary>
		/// <param name="value">The schema to match.</param>
		public ElseKeyword(JsonSchema value)
		{
			Schema = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			var annotation = context.LocalResult.TryGetAnnotation(IfKeyword.Name);
			if (annotation == null)
			{
				context.LocalResult.Pass();
				context.NotApplicable(() => $"No annotation found for {IfKeyword.Name}.");
				return;
			}

			if ((bool) annotation)
			{
				context.LocalResult.Pass();
				context.NotApplicable(() => $"Annotation for {IfKeyword.Name} is {annotation}.");
				return;
			}

			Schema.ValidateSubschema(context);
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			throw new NotImplementedException();
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			Schema.RegisterSubschemas(registry, currentUri);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(ElseKeyword? other)
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
			return Equals(obj as ElseKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Schema.GetHashCode();
		}
	}

	internal class ElseKeywordJsonConverter : JsonConverter<ElseKeyword>
	{
		public override ElseKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new ElseKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, ElseKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ElseKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}