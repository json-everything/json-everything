using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `then`.
	/// </summary>
	[Applicator]
	[SchemaKeyword(Name)]
	[SchemaPriority(10)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(ThenKeywordJsonConverter))]
	public class ThenKeyword : IJsonSchemaKeyword, IRefResolvable, IEquatable<ThenKeyword>
	{
		internal const string Name = "then";

		/// <summary>
		/// The schema to match.
		/// </summary>
		public JsonSchema Schema { get; }

		/// <summary>
		/// Creates a new <see cref="ThenKeyword"/>.
		/// </summary>
		/// <param name="value">The schema to match.</param>
		public ThenKeyword(JsonSchema value)
		{
			Schema = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			var annotation = context.TryGetAnnotation(IfKeyword.Name);
			if (annotation == null || !(bool) annotation)
			{
				context.IsValid = true;
				return;
			}

			var subContext = ValidationContext.From(context);
			Schema.ValidateSubschema(subContext);
			context.NestedContexts.Add(subContext);

			context.ConsolidateAnnotations();
			context.IsValid = subContext.IsValid;
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			return value == null ? Schema : null;
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			Schema.RegisterSubschemas(registry, currentUri);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(ThenKeyword other)
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
			return Equals(obj as ThenKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Schema != null ? Schema.GetHashCode() : 0;
		}
	}

	internal class ThenKeywordJsonConverter : JsonConverter<ThenKeyword>
	{
		public override ThenKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new ThenKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, ThenKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ThenKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}