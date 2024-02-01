using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
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
[JsonConverter(typeof(DataKeywordJsonConverter))]
public class DataKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "data";

	/// <summary>
	/// Gets or sets a method to download external references.
	/// </summary>
	/// <remarks>
	/// The default method simply attempts to download the resource.  There is no
	/// caching involved.
	/// </remarks>
	public static Func<Uri, JsonNode?>? Fetch { get; set; }

	/// <summary>
	/// Provides a registry for known external data sources.
	/// </summary>
	/// <remarks>
	/// This property stores full JSON documents retrievable by URI.  If the desired
	/// value exists as a sub-value of a document, a JSON Pointer URI fragment identifier
	/// should be used in the `data` keyword do identify the exact value location.
	///
	/// This registry will be checked before attempting to fetch the data.
	/// </remarks>
	public static ConcurrentDictionary<Uri, JsonNode> ExternalDataRegistry { get; } = new();


	/// <summary>
	/// The collection of keywords and references.
	/// </summary>
	public IReadOnlyDictionary<string, IDataResourceIdentifier> References { get; }

	/// <summary>
	/// Creates an instance of the <see cref="DataKeyword"/> class.
	/// </summary>
	/// <param name="references">The collection of keywords and references.</param>
	public DataKeyword(IReadOnlyDictionary<string, IDataResourceIdentifier> references)
	{
		References = references;
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
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(Name, Evaluator);
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context)
	{
		var data = new Dictionary<string, JsonNode>();
		var failedReferences = new List<IDataResourceIdentifier>();
		foreach (var reference in References)
		{
			if (!reference.Value.TryResolve(evaluation, context.Options.SchemaRegistry, out var resolved))
				failedReferences.Add(reference.Value);

			data.Add(reference.Key, resolved!);
		}

		if (failedReferences.Count != 0)
			throw new RefResolutionException(failedReferences.Select(x => x.ToString())!);

		var json = JsonSerializer.Serialize(data, JsonSchemaDataSerializerContext.Default.DictionaryStringJsonNode);
		var subschema = JsonSerializer.Deserialize(json, JsonSchemaSerializerContext.Default.JsonSchema)!;

		var schemaEvaluation = subschema
			.GetConstraint(JsonPointer.Create(Name), evaluation.Results.InstanceLocation, evaluation.Results.InstanceLocation, context)
			.BuildEvaluation(evaluation.LocalInstance, evaluation.Results.InstanceLocation, JsonPointer.Create(Name), context.Options);

		evaluation.ChildEvaluations = [schemaEvaluation];

		schemaEvaluation.Evaluate(context);

		if (!evaluation.ChildEvaluations.All(x => x.Results.IsValid))
			evaluation.Results.Fail();
	}

	/// <summary>
	/// Provides a simple data fetch method that supports `http`, `https`, and `file` URI schemes.
	/// </summary>
	/// <param name="uri">The URI to fetch.</param>
	/// <returns>A JSON string representing the data</returns>
	/// <exception cref="FormatException">
	/// Thrown when the URI scheme is not `http`, `https`, or `file`.
	/// </exception>
	public static JsonNode? SimpleDownload(Uri uri)
	{
		switch (uri.Scheme)
		{
			case "http":
			case "https":
				return new HttpClient().GetStringAsync(uri).Result;
			case "file":
				var filename = Uri.UnescapeDataString(uri.AbsolutePath);
				return File.ReadAllText(filename);
			default:
				throw new FormatException($"URI scheme '{uri.Scheme}' is not supported.  Only HTTP(S) and local file system URIs are allowed.");
		}
	}
}

/// <summary>
/// JSON converter for <see cref="DataKeyword"/>.
/// </summary>
public sealed class DataKeywordJsonConverter : AotCompatibleJsonConverter<DataKeyword>
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
	public override DataKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var references = options.Read(ref reader, JsonSchemaDataSerializerContext.Default.DictionaryStringString)!
			.ToDictionary(kvp => kvp.Key, kvp => JsonSchemaBuilderExtensions.CreateResourceIdentifier(kvp.Value));

		if (references.Keys.Intersect(_coreKeywords).Any())
			throw new JsonException("Core keywords are explicitly disallowed.");

		return new DataKeyword(references);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, DataKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var kvp in value.References)
		{
			writer.WritePropertyName(kvp.Key);
			switch (kvp.Value)
			{
				case JsonPointerIdentifier jp:
					options.Write(writer, jp.Target, JsonPointerSerializerContext.Default.JsonPointer);
					break;
				case RelativeJsonPointerIdentifier rjp:
					options.Write(writer, rjp.Target, JsonPointerSerializerContext.Default.RelativeJsonPointer);
					break;
				case UriIdentifier uri:
					options.Write(writer, uri.Target, JsonSchemaDataSerializerContext.Default.Uri);
					break;
			}
		}
		writer.WriteEndObject();
	}
}