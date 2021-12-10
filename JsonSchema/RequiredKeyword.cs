using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `requires`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(RequiredKeywordJsonConverter))]
	public class RequiredKeyword : IJsonSchemaKeyword, IEquatable<RequiredKeyword>
	{
		internal const string Name = "required";

		/// <summary>
		/// The required properties.
		/// </summary>
		public IReadOnlyList<string> Properties { get; }

		/// <summary>
		/// Creates a new <see cref="RequiredKeyword"/>.
		/// </summary>
		/// <param name="values">The required properties.</param>
		public RequiredKeyword(params string[] values)
		{
			Properties = values.ToList() ?? throw new ArgumentNullException(nameof(values));
		}

		/// <summary>
		/// Creates a new <see cref="RequiredKeyword"/>.
		/// </summary>
		/// <param name="values">The required properties.</param>
		public RequiredKeyword(IEnumerable<string> values)
		{
			Properties = values as List<string> ?? values.ToList();
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.LocalResult.Pass();
				context.WrongValueKind(context.LocalInstance.ValueKind);
				return;
			}

			context.Options.LogIndentLevel++;
			var notFound = new List<string>();
			for (int i = 0; i < Properties.Count; i++)
			{
				var property = Properties[i];
				context.Log(() => $"Checking for property '{property}'");
				if (!context.LocalInstance.TryGetProperty(property, out _))
					notFound.Add(property);
				if (notFound.Count != 0 && context.ApplyOptimizations) break;
			}
			if (notFound.Any())
				context.Log(() => $"Missing properties: [{string.Join(",", notFound.Select(x => $"'{x}'"))}]");
			context.Options.LogIndentLevel--;

			if (notFound.Count == 0)
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail($"Required properties [{string.Join(", ", notFound)}] were not present");
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(RequiredKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Properties.ContentsEqual(other.Properties);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as RequiredKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Properties.GetUnorderedCollectionHashCode();
		}
	}

	internal class RequiredKeywordJsonConverter : JsonConverter<RequiredKeyword>
	{
		public override RequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);

			if (document.RootElement.ValueKind != JsonValueKind.Array)
				throw new JsonException("Expected array");

			return new RequiredKeyword(document.RootElement.EnumerateArray()
				.Select(e => e.GetString()));
		}
		public override void Write(Utf8JsonWriter writer, RequiredKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(RequiredKeyword.Name);
			writer.WriteStartArray();
			foreach (var property in value.Properties)
			{
				writer.WriteStringValue(property);
			}
			writer.WriteEndArray();
		}
	}
}