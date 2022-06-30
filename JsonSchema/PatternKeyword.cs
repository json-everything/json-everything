using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Json.Schema;

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
			context.LocalResult.Fail(Name, ErrorMessages.InvalidPattern, ("pattern", Value.ToString()));
			context.ExitKeyword(Name, false);
			return;
		}

		var schemaValueType = context.LocalInstance.GetSchemaValueType();
		if (schemaValueType != SchemaValueType.String)
		{
			context.LocalResult.Pass();
			context.WrongValueKind(schemaValueType);
			return;
		}

		var str = context.LocalInstance!.GetValue<string>();
		if (Value.IsMatch(str))
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail(Name, ErrorMessages.Pattern, ("received", str), ("pattern", Value.ToString()));
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

		var str = reader.GetString()!;
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

public static partial class ErrorMessages
{
	private static string? _invalidPattern;

	/// <summary>
	/// Gets or sets the error message for when the <see cref="PatternKeyword"/> contains
	/// an invalid or unsupported regular expression.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[pattern]] - the regular expression
	/// </remarks>
	public static string InvalidPattern
	{
		get => _invalidPattern ?? Get();
		set => _invalidPattern = value;
	}

	private static string? _pattern;

	/// <summary>
	/// Gets or sets the error message for <see cref="OneOfKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the value provided in the JSON instance
	///   - [[pattern]] - the number of subschemas that passed validation
	///
	/// The default messages are static and do not use these tokens as string values
	/// could be quite large.  They are provided to support custom messages.
	/// </remarks>
	public static string Pattern
	{
		get => _pattern ?? Get();
		set => _pattern = value;
	}
}