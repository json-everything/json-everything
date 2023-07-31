using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Represents the `data` keyword.
/// </summary>
[SchemaKeyword(Name)]
[SchemaPriority(int.MinValue)]
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

		if (failedReferences.Any())
			throw new RefResolutionException(failedReferences.Select(x => x.ToString()));

		var json = JsonSerializer.Serialize(data);
		var subschema = JsonSerializer.Deserialize<JsonSchema>(json)!;

		var schemaEvaluation = subschema
			.GetConstraint(JsonPointer.Create(Name), evaluation.Results.InstanceLocation, evaluation.Results.InstanceLocation, context)
			.BuildEvaluation(evaluation.LocalInstance, evaluation.Results.InstanceLocation, JsonPointer.Create(Name), context.Options);

		evaluation.ChildEvaluations = new[] { schemaEvaluation };

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

internal class DataKeywordJsonConverter : JsonConverter<DataKeyword>
{
	private static readonly string[] _coreKeywords = Schema.Vocabularies.Core202012.Keywords.Select(GetKeyword).ToArray();

	private static string GetKeyword(Type keywordType)
	{
		var field = keywordType.GetField("Name", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		return (string)field!.GetValue(null);
	}

	public override DataKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException("Expected object");

		var references = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options)!
			.ToDictionary(kvp => kvp.Key, kvp => JsonSchemaBuilderExtensions.CreateResourceIdentifier(kvp.Value));

		if (references.Keys.Intersect(_coreKeywords).Any())
			throw new JsonException("Core keywords are explicitly disallowed.");

		return new DataKeyword(references);
	}

	public override void Write(Utf8JsonWriter writer, DataKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DataKeyword.Name);
		writer.WriteStartObject();
		foreach (var kvp in value.References)
		{
			writer.WritePropertyName(kvp.Key);
			switch (kvp.Value)
			{
				case JsonPointerIdentifier jp:
					JsonSerializer.Serialize(writer, jp.Target, options);
					break;
				case RelativeJsonPointerIdentifier rjp:
					JsonSerializer.Serialize(writer, rjp.Target, options);
					break;
				case UriIdentifier uri:
					JsonSerializer.Serialize(writer, uri.Target, options);
					break;
			}
		}
		writer.WriteEndObject();
	}
}