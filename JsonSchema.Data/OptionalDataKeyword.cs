using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Represents the `data` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
[Vocabulary(Vocabularies.DataId)]
[JsonConverter(typeof(OptionalDataKeywordJsonConverter))]
public class OptionalDataKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "optionalData";

	/// <summary>
	/// The collection of keywords and references.
	/// </summary>
	public IReadOnlyDictionary<string, IDataResourceIdentifier> References { get; }

	/// <summary>
	/// Creates an instance of the <see cref="DataKeyword"/> class.
	/// </summary>
	/// <param name="references">The collection of keywords and references.</param>
	public OptionalDataKeyword(IReadOnlyDictionary<string, IDataResourceIdentifier> references)
	{
		References = references;
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
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		Span<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var data = new Dictionary<string, JsonNode>();
		foreach (var reference in References)
		{
			 if (reference.Value.TryResolve(evaluation, context.Options.SchemaRegistry, out var resolved))
				data.Add(reference.Key, resolved!);
		}

		var json = JsonSerializer.Serialize(data, JsonSchemaDataSerializerContext.Default.DictionaryStringJsonNode);
		var subschema = JsonSerializer.Deserialize(json, JsonSchemaDataSerializerContext.Default.JsonSchema)!;

		var schemaEvaluation = subschema
			.GetConstraint(JsonPointer.Create(Name), evaluation.Results.InstanceLocation, evaluation.Results.InstanceLocation, context)
			.BuildEvaluation(evaluation.LocalInstance, evaluation.Results.InstanceLocation, JsonPointer.Create(Name), context.Options);

		evaluation.ChildEvaluations = [schemaEvaluation];

		schemaEvaluation.Evaluate(context);

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}
}

/// <summary>
/// JSON converter for <see cref="DataKeyword"/>.
/// </summary>
public sealed class OptionalDataKeywordJsonConverter : WeaklyTypedJsonConverter<OptionalDataKeyword>
{
	private static readonly string[] _coreKeywords =
		Schema.Vocabularies.Core202012.Keywords
			.Where(x => x != typeof(UnrecognizedKeyword))
			.Select(x => x.Keyword())
			.ToArray();

	/// <summary>Reads and converts the JSON to type <see cref="DataKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override OptionalDataKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var references = options.ReadDictionary(ref reader, JsonSchemaDataSerializerContext.Default.String)!
			.ToDictionary(kvp => kvp.Key, kvp => JsonSchemaBuilderExtensions.CreateResourceIdentifier(kvp.Value));

		if (references.Keys.Intersect(_coreKeywords).Any())
			throw new JsonException("Core keywords are explicitly disallowed.");

		return new OptionalDataKeyword(references);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, OptionalDataKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var kvp in value.References)
		{
			writer.WritePropertyName(kvp.Key);
			switch (kvp.Value)
			{
				case JsonPointerIdentifier jp:
					options.Write(writer, jp.Target, JsonSchemaDataSerializerContext.Default.JsonPointer);
					break;
				case RelativeJsonPointerIdentifier rjp:
					options.Write(writer, rjp.Target, JsonSchemaDataSerializerContext.Default.RelativeJsonPointer);
					break;
				case UriIdentifier uri:
					options.Write(writer, uri.Target, JsonSchemaDataSerializerContext.Default.Uri);
					break;
			}
		}
		writer.WriteEndObject();
	}
}