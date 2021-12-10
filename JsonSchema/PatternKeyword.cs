using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Json.Schema
{
	/// <summary>
	/// Handles `pattern`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(PatternKeywordJsonConverter))]
	public class PatternKeyword : IJsonSchemaKeyword, IEquatable<PatternKeyword>
	{
		internal const string Name = "pattern";

		/// <summary>
		/// The regular expression.
		/// </summary>
		public Regex Value { get; }
		/// <summary>
		/// If the pattern is invalid or unsupported by <see cref="Regex"/>, it will appear here.
		/// </summary>
		/// <remarks>
		/// All validations will fail if this is populated.
		/// </remarks>
		public string? InvalidPattern { get; }

		/// <summary>
		/// Creates a new <see cref="PatternKeyword"/>.
		/// </summary>
		/// <param name="value">The regular expression.</param>
		public PatternKeyword(Regex value)
		{
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		private PatternKeyword(string invalidPattern)
		{
			InvalidPattern = invalidPattern;
			Value = new Regex($"^{Guid.NewGuid():N}$");
		}

		internal static PatternKeyword InvalidRegex(string pattern) => new(pattern);

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			if (InvalidPattern != null)
			{
				context.LocalResult.Fail($"The regular expression `{InvalidPattern}` is either invalid or not supported");
				context.ExitKeyword(Name, false);
				return;
			}

			if (context.LocalInstance.ValueKind != JsonValueKind.String)
			{
				context.LocalResult.Pass();
				context.WrongValueKind(context.LocalInstance.ValueKind);
				return;
			}

			var str = context.LocalInstance.GetString();
			if (Value.IsMatch(str))
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail("The string value was not a match for the indicated regular expression");
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(PatternKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Value.ToString() == other.Value.ToString();
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as PatternKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}

	internal class PatternKeywordJsonConverter : JsonConverter<PatternKeyword>
	{
		public override PatternKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var str = reader.GetString();
			try
			{
				var regex = new Regex(str, RegexOptions.ECMAScript | RegexOptions.Compiled);

				return new PatternKeyword(regex);
			}
			catch
			{
				return PatternKeyword.InvalidRegex(str);
			}
		}
		public override void Write(Utf8JsonWriter writer, PatternKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(PatternKeyword.Name, value.Value.ToString());
		}
	}
}