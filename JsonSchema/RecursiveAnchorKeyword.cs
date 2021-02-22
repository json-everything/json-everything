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
	public class RecursiveAnchorKeyword : IJsonSchemaKeyword, IEquatable<RecursiveAnchorKeyword>
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

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(RecursiveAnchorKeyword? other)
		{
			return true;
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as RecursiveAnchorKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
			return base.GetHashCode();
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