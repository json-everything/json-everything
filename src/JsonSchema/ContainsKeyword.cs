using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `contains`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.Applicator201909Id)]
[Vocabulary(Vocabularies.Applicator202012Id)]
[Vocabulary(Vocabularies.ApplicatorNextId)]
[DependsOnAnnotationsFrom(typeof(MinContainsKeyword))]
[DependsOnAnnotationsFrom(typeof(MaxContainsKeyword))]
[JsonConverter(typeof(ContainsKeywordJsonConverter))]
public class ContainsKeyword : IJsonSchemaKeyword, ISchemaContainer
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "contains";

	/// <summary>
	/// The schema to match.
	/// </summary>
	public JsonSchema Schema { get; }

	/// <summary>
	/// Creates a new <see cref="ContainsKeyword"/>.
	/// </summary>
	/// <param name="value">The schema to match.</param>
	public ContainsKeyword(JsonSchema value)
	{
		Schema = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	///     The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	///     Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, ReadOnlySpan<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		var subschemaConstraint = Schema.GetConstraint(JsonPointer_Old.Create(Name), schemaConstraint.BaseInstanceLocation, JsonPointer_Old.Empty, context);

		subschemaConstraint.InstanceLocator = SubschemaConstraintInstanceLocator;

		return new KeywordConstraint(Name, Evaluator)
		{
			ChildDependencies = [subschemaConstraint]
		};
	}

	private static IEnumerable<JsonPointer_Old> SubschemaConstraintInstanceLocator(KeywordEvaluation evaluation)
	{
		if (evaluation.LocalInstance is not JsonArray array) yield break;
		if (array.Count == 0) yield break;

		for (int i = 0; i < array.Count; i++)
		{
			yield return CommonJsonPointers.GetNumberSegment(i);
		}
	}

	private static void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		if (evaluation.LocalInstance is JsonArray)
		{
			uint minimum = 1;
			uint? maximum = null;
			if (context.EvaluatingAs >= SpecVersion.Draft201909)
			{
				// still need to check spec version because unknown keywords are collected as annotations.
				if (evaluation.Results.TryGetAnnotation(MinContainsKeyword.Name, out var minContainsAnnotation))
					minimum = minContainsAnnotation!.GetValue<uint>();
				if (evaluation.Results.TryGetAnnotation(MaxContainsKeyword.Name, out var maxContainsAnnotation))
					maximum = maxContainsAnnotation!.GetValue<uint>();
			}

			var validIndices = evaluation.ChildEvaluations
				.Where(x => x.Results.IsValid)
				.Select(x => int.Parse(x.RelativeInstanceLocation[0]))
				.ToArray();

			var actual = validIndices.Length;
			if (actual < minimum)
				evaluation.Results.Fail(Name, ErrorMessages.GetContainsTooFew(context.Options.Culture)
					.ReplaceToken("received", actual)
					.ReplaceToken("minimum", minimum));
			else if (actual > maximum)
				evaluation.Results.Fail(Name, ErrorMessages.GetContainsTooMany(context.Options.Culture)
					.ReplaceToken("received", actual)
					.ReplaceToken("maximum", maximum.Value));
			
			evaluation.Results.SetAnnotation(Name, JsonSerializer.SerializeToNode(validIndices, JsonSchemaSerializerContext.Default.Int32Array));
			return;
		}

		evaluation.MarkAsSkipped();
	}
}

/// <summary>
/// JSON converter for <see cref="ContainsKeyword"/>.
/// </summary>
public sealed class ContainsKeywordJsonConverter : WeaklyTypedJsonConverter<ContainsKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="ContainsKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override ContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var schema = options.Read(ref reader, JsonSchemaSerializerContext.Default.JsonSchema)!;

		return new ContainsKeyword(schema);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, ContainsKeyword value, JsonSerializerOptions options)
	{
		options.Write(writer, value.Schema, JsonSchemaSerializerContext.Default.JsonSchema);
	}
}

public static partial class ErrorMessages
{
	/// <summary>
	/// Gets or sets the error message for <see cref="ContainsKeyword"/> when there are too few matching items.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[minimum]] - the lower limit specified in the schema
	/// </remarks>
	public static string? ContainsTooFew { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="ContainsKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[minimum]] - the lower limit specified in the schema
	/// </remarks>
	public static string GetContainsTooFew(CultureInfo? culture)
	{
		return ContainsTooFew ?? Get(culture);
	}

	/// <summary>
	/// Gets or sets the error message for <see cref="ContainsKeyword"/> when there are too many matching items.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[maximum]] - the upper limit specified in the schema
	/// </remarks>
	public static string? ContainsTooMany { get; set; }

	/// <summary>
	/// Gets the error message for <see cref="ContainsKeyword"/> for a specific culture.
	/// </summary>
	/// <param name="culture">The culture to retrieve.</param>
	/// <remarks>
	///	Available tokens are:
	///   - [[received]] - the number of matching items provided in the JSON instance
	///   - [[maximum]] - the upper limit specified in the schema
	/// </remarks>
	public static string GetContainsTooMany(CultureInfo? culture)
	{
		return ContainsTooMany ?? Get(culture);
	}
}