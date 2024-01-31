using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// The results object for evaluations.
/// </summary>
[JsonConverter(typeof(EvaluationResultsJsonConverter))]
public class EvaluationResults
{
	private readonly Uri _currentUri;
	private readonly HashSet<string>? _backgroundAnnotations;
	private readonly HashSet<string>? _ignoredAnnotations;
	private JsonPointer? _reference;
	private Uri? _schemaLocation;
	private List<EvaluationResults>? _details;
	private Dictionary<string, JsonNode?>? _annotations;
	private Dictionary<string, string>? _errors;

	/// <summary>
	/// Indicates whether the validation passed or failed.
	/// </summary>
	public bool IsValid { get; private set; } = true;
	/// <summary>
	/// The schema location that generated this node.
	/// </summary>
	public JsonPointer EvaluationPath { get; }
	/// <summary>
	/// The instance location that was processed.
	/// </summary>
	public JsonPointer InstanceLocation { get; }

	/// <summary>
	/// The absolute schema location.
	/// </summary>
	/// <remarks>
	/// If the schema did not have an absolute `$id`, a generated base URI will be used.
	/// </remarks>
	public Uri SchemaLocation => _schemaLocation ??= BuildSchemaLocation();

	/// <summary>
	/// The collection of nested results.
	/// </summary>
	public IReadOnlyList<EvaluationResults> Details => _details ??= [];

	/// <summary>
	/// Gets whether there are nested results.
	/// </summary>
	/// <remarks>
	/// Because <see cref="Details"/> is lazily loaded, this property allows the check without
	/// the side effect of allocating a list object.
	/// </remarks>
	public bool HasDetails => _details is not (null or { Count: 0 });

	/// <summary>
	/// The collection of annotations from this node.
	/// </summary>
	public IReadOnlyDictionary<string, JsonNode?>? Annotations => _annotations;

	/// <summary>
	/// Gets whether this node has annotations.
	/// </summary>
	public bool HasAnnotations => Annotations is not (null or { Count: 0 });

	/// <summary>
	/// The collection of error from this node.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Errors => _errors;

	/// <summary>
	/// Gets whether this node has errors.
	/// </summary>
	public bool HasErrors => Errors is not (null or { Count: 0 });

	/// <summary>
	/// Gets the parent result.
	/// </summary>
	public EvaluationResults? Parent { get; private set; }

	internal bool Exclude { get; private set; }

	internal OutputFormat Format { get; private set; } = OutputFormat.Hierarchical;

	internal bool IncludeDroppedAnnotations { get; }

	internal IReadOnlyDictionary<string, JsonNode?>? AnnotationsToSerialize =>
		HasAnnotations
			? _annotations!.Where(x => !(_backgroundAnnotations?.Contains(x.Key) ?? false)).ToDictionary(x => x.Key, x => x.Value)
			: null;

	internal EvaluationResults(JsonPointer evaluationPath, Uri schemaLocation, JsonPointer instanceLocation, EvaluationOptions options)
	{
		EvaluationPath = evaluationPath;
		_currentUri = schemaLocation;
		InstanceLocation = instanceLocation;

		IncludeDroppedAnnotations = options.PreserveDroppedAnnotations;
		if (options.IgnoredAnnotations != null)
		{
			_ignoredAnnotations = new HashSet<string>(options.IgnoredAnnotations.Where(x => !x.ProducesDependentAnnotations()).Select(x => x.Keyword()));
			_backgroundAnnotations = new HashSet<string>(options.IgnoredAnnotations.Where(x => x.ProducesDependentAnnotations()).Select(x => x.Keyword()));
		}
	}

	private EvaluationResults(EvaluationResults other)
	{
		IsValid = other.IsValid;
		EvaluationPath = other.EvaluationPath;
		_currentUri = other._currentUri;
		_schemaLocation = other._schemaLocation;
		InstanceLocation = other.InstanceLocation;
		_annotations = other._annotations?.ToDictionary(x => x.Key, x => x.Value);
		_errors = other._errors?.ToDictionary(x => x.Key, x => x.Value);
		IncludeDroppedAnnotations = other.IncludeDroppedAnnotations;
		_ignoredAnnotations = other._ignoredAnnotations;
		_backgroundAnnotations = other._backgroundAnnotations;
	}

	internal void SetSchemaReference(JsonPointer pointer)
	{
		_reference = pointer;
	}

	private Uri BuildSchemaLocation()
	{
		var localEvaluationPathStart = 0;
		for (var i = 0; i < EvaluationPath.Segments.Length; i++)
		{
			var segment = EvaluationPath.Segments[i];
			if (segment.Value is RefKeyword.Name or RecursiveRefKeyword.Name or DynamicRefKeyword.Name)
				localEvaluationPathStart = i + 1;
		}

		if (_reference == null && _currentUri == Parent?._currentUri)
			_reference = Parent._reference;
		var fragment = _reference ?? JsonPointer.Empty;
		fragment = fragment.Combine(EvaluationPath.Segments.Skip(localEvaluationPathStart).ToArray());

		return fragment == JsonPointer.Empty
			? _currentUri
			: new Uri(_currentUri, "#" + fragment);
	}

	/// <summary>
	/// Transforms the results to the `basic` format.
	/// </summary>
	public void ToList()
	{
		var children = GetAllChildren().ToList();
		if (children.Count == 0) return;

		children.Remove(this);
		children.Insert(0, new EvaluationResults(this) { Parent = this });
		_annotations?.Clear();
		_errors?.Clear();
		if (_details == null)
			_details = [];
		else
			_details.Clear();
		foreach (var child in children)
		{
			child._details?.Clear();
			child.Format = OutputFormat.List;
		}
		//_details.AddRange(children.Where(x => (x.IsValid && x.HasAnnotations) || (!x.IsValid && x.HasErrors)));
		_details.AddRange(children);
		Format = OutputFormat.List;
	}

	private IEnumerable<EvaluationResults> GetAllChildren()
	{
		var all = new List<EvaluationResults>();
		var toProcess = new Queue<EvaluationResults>();

		toProcess.Enqueue(this);
		while (toProcess.Count != 0)
		{
			var current = toProcess.Dequeue();
			all.Add(current);
			if (!current.HasDetails) continue;

			foreach (var nestedResult in current.Details)
			{
				toProcess.Enqueue(nestedResult);
			}
			current._details?.Clear();
		}

		// we still include the root because it may have annotations
		// don't report annotations at the root of the output
		return all;
	}

	/// <summary>
	/// Transforms the results to the `flag` format.
	/// </summary>
	public void ToFlag()
	{
		_details?.Clear();
		_annotations?.Clear();
		_errors?.Clear();
		Format = OutputFormat.Flag;
	}

	/// <summary>
	/// Sets an annotation.
	/// </summary>
	/// <param name="keyword">The annotation key.  Typically the name of the keyword.</param>
	/// <param name="value">The annotation value.</param>
	public void SetAnnotation(string keyword, JsonNode? value)
	{
		if (_ignoredAnnotations?.Any(x => x == keyword) ?? false) return;

		_annotations ??= [];

		_annotations[keyword] = value;
	}

	/// <summary>
	/// Tries to get an annotation.
	/// </summary>
	/// <param name="keyword">The annotation key.</param>
	/// <param name="annotation"></param>
	/// <returns>The annotation or null.</returns>
	public bool TryGetAnnotation(string keyword, out JsonNode? annotation)
	{
		annotation = null;
		if (!HasAnnotations) return false;
		return Annotations!.TryGetValue(keyword, out annotation);
	}

	/// <summary>
	/// Gets all annotations of a particular data type for the current evaluation level.
	/// </summary>
	/// <param name="keyword">The key under which the annotation is stored.  Typically a keyword.</param>
	/// <returns>The set of all annotations for the current evaluation level.</returns>
	public IEnumerable<JsonNode?> GetAllAnnotations(string keyword)
	{
		if (HasAnnotations && _annotations!.TryGetValue(keyword, out var annotation))
			yield return annotation;

		if (!HasDetails) yield break;

		var validResults = Details.Where(x => x.IsValid && x.InstanceLocation == InstanceLocation);
		var allAnnotations = validResults.SelectMany(x => x.GetAllAnnotations(keyword));
		foreach (var nestedAnnotation in allAnnotations)
		{
			yield return nestedAnnotation;
		}
	}

	/// <summary>
	/// Marks the result as invalid.
	/// </summary>
	/// <remarks>
	/// For better support for customization, consider using the overload that takes parameters.
	/// </remarks>
	public void Fail()
	{
		IsValid = false;
	}

	/// <summary>
	/// Marks the result as invalid.
	/// </summary>
	/// <param name="keyword">The keyword that failed validation.</param>
	/// <param name="message">An error message.</param>
	/// <remarks>
	/// For better support for customization, consider using the overload that takes parameters.
	/// </remarks>
	public void Fail(string keyword, string? message)
	{
		IsValid = false;
		if (message == null) return;

		_errors ??= [];
		_errors[keyword] = message;
	}

	internal void AddNestedResult(EvaluationResults results)
	{
		_details ??= [];
		_details.Add(results);
		results.Parent = this;
	}
}

internal class EvaluationResultsJsonConverter : AotCompatibleJsonConverter<EvaluationResults>
{
	public override EvaluationResults Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, EvaluationResults value, JsonSerializerOptions options)
	{
		if (value.Exclude) return;

		writer.WriteStartObject();

		writer.WriteBoolean("valid", value.IsValid);

		if (value.Format == OutputFormat.Hierarchical || value.Parent != null)
		{
			writer.WritePropertyName("evaluationPath");
			options.Write(writer, value.EvaluationPath, JsonSchemaSerializerContext.Default.JsonPointer);

			// this can still be null if the root schema is a boolean
			if (value.SchemaLocation != null!)
			{
				writer.WritePropertyName("schemaLocation");
				var schemaLocation = value.SchemaLocation.OriginalString;
				if (string.IsNullOrEmpty(value.SchemaLocation.Fragment))
					schemaLocation += "#"; // see https://github.com/json-schema-org/JSON-Schema-Test-Suite/pull/671
				options.Write(writer, schemaLocation, JsonSchemaSerializerContext.Default.String);
			}

			writer.WritePropertyName("instanceLocation");
			options.Write(writer, value.InstanceLocation, JsonSchemaSerializerContext.Default.JsonPointer);
		}

		if (value.IsValid)
		{
			if (value.AnnotationsToSerialize != null)
			{
				writer.WritePropertyName("annotations");
				options.Write(writer, value.AnnotationsToSerialize!, JsonSchemaSerializerContext.Default.IReadOnlyDictionaryStringJsonNode);
			}
		}
		else
		{
			if (value.HasErrors)
			{
				writer.WritePropertyName("errors");
				options.Write(writer, value.Errors, JsonSchemaSerializerContext.Default.IReadOnlyDictionaryStringString);
			}
			if (value is { IncludeDroppedAnnotations: true, AnnotationsToSerialize: not null })
			{
				writer.WritePropertyName("droppedAnnotations");
				options.Write(writer, value.AnnotationsToSerialize!, JsonSchemaSerializerContext.Default.IReadOnlyDictionaryStringJsonNode);
			}
		}

		if (value.HasDetails)
		{
			writer.WritePropertyName("details");
			options.Write(writer, value.Details, JsonSchemaSerializerContext.Default.IReadOnlyListEvaluationResults);
}

		writer.WriteEndObject();
	}
}

/// <summary>
/// Produces output formats specified by 2019-09 and 2020-12.
/// </summary>
public class Pre202012EvaluationResultsJsonConverter : AotCompatibleJsonConverter<EvaluationResults>
{
	/// <summary>
	/// Holder for an annotation value.
	/// </summary>
	private class Annotation
	{
		/// <summary>
		/// The keyword that created the annotation (acts as a key for lookup).
		/// </summary>
		public string Owner { get; }
		/// <summary>
		/// The annotation value.
		/// </summary>
		public object? Value { get; }
		/// <summary>
		/// The pointer to the keyword that created the annotation.
		/// </summary>
		public JsonPointer Source { get; }

		/// <summary>
		/// Creates a new <see cref="Annotation"/>.
		/// </summary>
		/// <param name="owner">The keyword that created the annotation (acts as a key for lookup).</param>
		/// <param name="value">The annotation value.</param>
		/// <param name="source">The pointer to the keyword that created the annotation.</param>
		public Annotation(string owner, object? value, in JsonPointer source)
		{
			Owner = owner;
			Value = value;
			Source = source;
		}
	}

	/// <summary>Reads and converts the JSON to type <see cref="EvaluationResults"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override EvaluationResults Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, EvaluationResults value, JsonSerializerOptions options)
	{
		if (value.Exclude) return;

		writer.WriteStartObject();

		writer.WriteBoolean("valid", value.IsValid);

		if (value.Format == OutputFormat.Hierarchical || value.Parent != null)
		{
			writer.WritePropertyName("keywordLocation");
			options.Write(writer, value.EvaluationPath, JsonSchemaSerializerContext.Default.JsonPointer);

			writer.WritePropertyName("absoluteKeywordLocation");
			options.Write(writer, value.SchemaLocation, JsonSchemaSerializerContext.Default.Uri);

			writer.WritePropertyName("instanceLocation");
			options.Write(writer, value.InstanceLocation, JsonSchemaSerializerContext.Default.JsonPointer);
		}

		bool skipCloseObject = false;
		if (!value.IsValid)
		{
			if (value.HasErrors && value.Errors!.TryGetValue(string.Empty, out var localError))
				writer.WriteString("error", localError);

			if (value.Format == OutputFormat.Hierarchical)
			{
				if ((value.HasErrors && value.Errors!.Any(x => x.Key != string.Empty)) || value.Details.Any())
				{
					writer.WritePropertyName("errors");

					writer.WriteStartArray();

					foreach (var result in value.Details)
					{
						options.Write(writer, result, JsonSchemaSerializerContext.Default.EvaluationResults);
					}

					if (value.HasErrors)
					{
						foreach (var error in value.Errors!)
						{
							WriteError(writer, value, error.Key, error.Value, options);
						}
					}

					writer.WriteEndArray();
				}
			}
			else
			{
				if (value.HasDetails)
				{
					writer.WritePropertyName("errors");
					writer.WriteStartArray();

					foreach (var result in value.Details)
					{
						options.Write(writer, result, JsonSchemaSerializerContext.Default.EvaluationResults);
					}
					writer.WriteEndArray();
				}

				if (value.HasErrors && value.Errors!.Any(x => x.Key != string.Empty))
				{
					skipCloseObject = true;
					writer.WriteEndObject();
					foreach (var error in value.Errors!)
					{
						WriteError(writer, value, error.Key, error.Value, options);
					}
				}
			}
		}
		else
		{
			if (value.Format == OutputFormat.Hierarchical)
			{
				if (value.AnnotationsToSerialize != null || value.Details.Any())
				{
					writer.WritePropertyName("annotations");
					writer.WriteStartArray();

					var annotations = value.AnnotationsToSerialize?.Select(x => new Annotation(x.Key, x.Value, value.EvaluationPath.Combine(x.Key))).ToArray();

					// this too

					foreach (var result in value.Details)
					{
						var annotation = annotations?.SingleOrDefault(a => a.Source.Equals(result.EvaluationPath));
						if (annotation != null)
						{
							WriteAnnotation(writer, value, annotation, options);
						}
						else
						{
							options.Write(writer, result, JsonSchemaSerializerContext.Default.EvaluationResults);
						}
					}

					if (annotations != null)
					{
						foreach (var annotation in annotations)
						{
							WriteAnnotation(writer, value, annotation, options);
						}
					}

					writer.WriteEndArray();
				}
			}
			else
			{
				var annotations = value.AnnotationsToSerialize?.Select(x => new Annotation(x.Key, x.Value, value.EvaluationPath.Combine(x.Key))).ToArray() ?? [];

				if (value.HasDetails)
				{
					writer.WritePropertyName("annotations");
					writer.WriteStartArray();

					foreach (var result in value.Details)
					{
						var annotation = annotations.SingleOrDefault(a => a.Source.Equals(result.EvaluationPath));
						if (annotation != null) continue;

						options.Write(writer, result, JsonSchemaSerializerContext.Default.EvaluationResults);
					}
					writer.WriteEndArray();
				}

				if (value.HasAnnotations)
				{
					skipCloseObject = true;
					writer.WriteEndObject();
					foreach (var annotation in annotations)
					{
						WriteAnnotation(writer, value, annotation, options);
					}
				}
			}
		}

		if (!skipCloseObject)
			writer.WriteEndObject();
	}

	private static void WriteError(Utf8JsonWriter writer, EvaluationResults value, string keyword, string error, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteBoolean("valid", value.IsValid);

		writer.WritePropertyName("keywordLocation");
		options.Write(writer, value.EvaluationPath.Combine(keyword), JsonSchemaSerializerContext.Default.JsonPointer);

		writer.WritePropertyName("absoluteKeywordLocation");
		if (value.SchemaLocation.OriginalString.Contains('#'))
			options.Write(writer, value.SchemaLocation.OriginalString + $"/{keyword}", JsonSchemaSerializerContext.Default.String);
		else
			options.Write(writer, value.SchemaLocation.OriginalString + $"#/{keyword}", JsonSchemaSerializerContext.Default.String);

		writer.WritePropertyName("instanceLocation");
		options.Write(writer, value.InstanceLocation, JsonSchemaSerializerContext.Default.JsonPointer);

		writer.WritePropertyName("error");
		options.Write(writer, error, JsonSchemaSerializerContext.Default.String);

		writer.WriteEndObject();
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	private static void WriteAnnotation(Utf8JsonWriter writer, EvaluationResults value, Annotation annotation, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteBoolean("valid", value.IsValid);

		writer.WritePropertyName("keywordLocation");
		options.Write(writer, annotation.Source, JsonSchemaSerializerContext.Default.JsonPointer);

		writer.WritePropertyName("absoluteKeywordLocation");
		if (value.SchemaLocation.OriginalString.Contains('#'))
			options.Write(writer, value.SchemaLocation.OriginalString + $"/{annotation.Owner}", JsonSchemaSerializerContext.Default.String);
		else
			options.Write(writer, value.SchemaLocation.OriginalString + $"#/{annotation.Owner}", JsonSchemaSerializerContext.Default.String);

		writer.WritePropertyName("instanceLocation");
		options.Write(writer, value.InstanceLocation, JsonSchemaSerializerContext.Default.JsonPointer);

		writer.WritePropertyName("annotation");
		JsonSerializer.Serialize(writer, annotation.Value, options);

		writer.WriteEndObject();
	}
}