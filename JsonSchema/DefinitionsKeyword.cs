using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `definitions`.
	/// </summary>
	[SchemaPriority(long.MinValue + 1)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[JsonConverter(typeof(DefinitionsKeywordJsonConverter))]
	public class DefinitionsKeyword : IJsonSchemaKeyword, IRefResolvable, IKeyedSchemaCollector, IEquatable<DefinitionsKeyword>
	{
		internal const string Name = "definitions";

		/// <summary>
		/// The collection of schema definitions.
		/// </summary>
		public IReadOnlyDictionary<string, JsonSchema> Definitions { get; }

		IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Definitions;

		/// <summary>
		/// Creates a new <see cref="DefinitionsKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of schema definitions.</param>
		public DefinitionsKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Definitions = values ?? throw new ArgumentNullException(nameof(values));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			context.LocalResult.Ignore();
			context.ExitKeyword(Name, true);
		}

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			throw new NotImplementedException();
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Definitions.Values)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(DefinitionsKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Definitions.Count != other.Definitions.Count) return false;
			var byKey = Definitions.Join(other.Definitions,
					td => td.Key,
					od => od.Key,
					(td, od) => new {ThisDef = td.Value, OtherDef = od.Value})
				.ToList();
			if (byKey.Count != Definitions.Count) return false;

			return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as DefinitionsKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Definitions.GetStringDictionaryHashCode();
		}
	}

	internal class DefinitionsKeywordJsonConverter : JsonConverter<DefinitionsKeyword>
	{
		public override DefinitionsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);
			return new DefinitionsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, DefinitionsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DefinitionsKeyword.Name);
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