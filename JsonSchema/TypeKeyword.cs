using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Handles `type`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Validation201909Id)]
[Vocabulary(Vocabularies.Validation202012Id)]
[Vocabulary(Vocabularies.ValidationNextId)]
[JsonConverter(typeof(TypeKeywordJsonConverter))]
public class TypeKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "type";

	/// <summary>
	/// The expected type.
	/// </summary>
	public SchemaValueType Type { get; }

	/// <summary>
	/// Creates a new <see cref="TypeKeyword"/>.
	/// </summary>
	/// <param name="type">The expected type.</param>
	public TypeKeyword(SchemaValueType type)
	{
		Type = type;
	}

	/// <summary>
	/// Creates a new <see cref="TypeKeyword"/>.
	/// </summary>
	/// <param name="types">The expected types.</param>
	public TypeKeyword(params SchemaValueType[] types)
	{
		// TODO: protect input

		Type = types.Aggregate((x, y) => x | y);
	}

	/// <summary>
	/// Creates a new <see cref="TypeKeyword"/>.
	/// </summary>
	/// <param name="types">The expected types.</param>
	public TypeKeyword(IEnumerable<SchemaValueType> types)
	{
		// TODO: protect input

		Type = types.Aggregate((x, y) => x | y);
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		return new KeywordConstraint(Name, (e, c) => Evaluator(e, c, Type));
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context, SchemaValueType expectedType)
	{
		var instanceType = evaluation.LocalInstance.GetSchemaValueType();
		if (expectedType.HasFlag(instanceType)) return;
		if (instanceType == SchemaValueType.Integer && expectedType.HasFlag(SchemaValueType.Number)) return;
		if (instanceType == SchemaValueType.Number)
		{
			var number = evaluation.LocalInstance!.AsValue().GetNumber();
			if (number == Math.Truncate(number!.Value) && expectedType.HasFlag(SchemaValueType.Integer)) return;
		}

		var expected = expectedType.ToString().ToLower();
		evaluation.Results.Fail(Name, ErrorMessages.GetType(context.Options.Culture), ("received", instanceType), ("expected", expected));
	}
}

/// <summary>
/// JSON converter for <see cref="TypeKeyword"/>.
/// </summary>
public sealed class TypeKeywordJsonConverter : JsonConverter<TypeKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="TypeKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override TypeKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var type = JsonSerializer.Deserialize(ref reader, JsonSchemaSerializationContext.Default.SchemaValueType);

		return new TypeKeyword(type);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, TypeKeyword value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Type, options);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="TypeKeyword"/>.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the type of value provided in the JSON instance
	///   - [[expected]] - the type(s) required by the schema
	/// </remarks>
	public static string? Type { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="TypeKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the type of value provided in the JSON instance
	///   - [[expected]] - the type(s) required by the schema
	/// </remarks>
	public static string GetType(CultureInfo? culture)
	{
		return Type ?? Get(culture);
	}
}