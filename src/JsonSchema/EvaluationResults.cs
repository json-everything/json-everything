using System;
using System.Collections.Generic;
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

	/// <summary>
	/// Indicates whether the validation passed or failed.
	/// </summary>
	public bool IsValid { get; set; } = true;
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
	public List<EvaluationResults>? Details { get; set; }

	/// <summary>
	/// The collection of annotations from this node.
	/// </summary>
	public Dictionary<string, JsonNode?>? Annotations { get; set; }

	/// <summary>
	/// The collection of error from this node.
	/// </summary>
	public Dictionary<string, string>? Errors { get; set; }

	/// <summary>
	/// Gets the parent result.
	/// </summary>
	public EvaluationResults? Parent { get; private set; }

	internal bool Exclude { get; private set; }

	internal OutputFormat Format { get; private set; } = OutputFormat.Hierarchical;

	internal bool IncludeDroppedAnnotations { get; }

	internal IReadOnlyDictionary<string, JsonNode?>? AnnotationsToSerialize =>
		Annotations is not null && Annotations.Count != 0
			? Annotations!.Where(x => !(_backgroundAnnotations?.Contains(x.Key) ?? false)).ToDictionary(x => x.Key, x => x.Value)
			: null;

	internal EvaluationResults(JsonPointer evaluationPath, Uri schemaLocation, JsonPointer instanceLocation, EvaluationOptions options)
	{
		EvaluationPath = evaluationPath;
		_currentUri = schemaLocation;
		InstanceLocation = instanceLocation;

		IncludeDroppedAnnotations = options.PreserveDroppedAnnotations;
		if (options.IgnoredAnnotations != null)
		{
			//_ignoredAnnotations = new HashSet<string>(options.IgnoredAnnotations.Where(x => !x.ProducesDependentAnnotations()).Select(x => x.Keyword()));
			//_backgroundAnnotations = new HashSet<string>(options.IgnoredAnnotations.Where(x => x.ProducesDependentAnnotations()).Select(x => x.Keyword()));
		}
	}

	private EvaluationResults(EvaluationResults other)
	{
		IsValid = other.IsValid;
		EvaluationPath = other.EvaluationPath;
		_currentUri = other._currentUri;
		_schemaLocation = other._schemaLocation;
		InstanceLocation = other.InstanceLocation;
		Annotations = other.Annotations?.ToDictionary(x => x.Key, x => x.Value);
		Errors = other.Errors?.ToDictionary(x => x.Key, x => x.Value);
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
		for (var i = 0; i < EvaluationPath.SegmentCount; i++)
		{
			var segment = EvaluationPath[i];
			//if (segment == RefKeyword.Name ||
			//    segment == RecursiveRefKeyword.Name ||
			//    segment == DynamicRefKeyword.Name)
			//	localEvaluationPathStart = i + 1;
		}

		if (_reference == null && _currentUri == Parent?._currentUri)
			_reference = Parent._reference;
		var fragment = _reference ?? JsonPointer.Empty;
		fragment = fragment.Combine(EvaluationPath.GetLocal(localEvaluationPathStart));  // 2 allocations

		return fragment == JsonPointer.Empty
			? _currentUri
			: new Uri(_currentUri, "#" + fragment);
	}

	/// <summary>
	/// Transforms the results to the `basic` format.
	/// </summary>
	public void ToList()
	{
		var children = GetAllChildren();
		if (children.Count == 0) return;

		children.Remove(this);
		children.Insert(0, new EvaluationResults(this) { Parent = this });
		Annotations?.Clear();
		Errors?.Clear();
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

	private List<EvaluationResults> GetAllChildren()
	{
		var all = new List<EvaluationResults>();
		var toProcess = new Queue<EvaluationResults>();

		toProcess.Enqueue(this);
		while (toProcess.Count != 0)
		{
			var current = toProcess.Dequeue();
			all.Add(current);
			if (current.Details is null || current.Details.Count == 0) continue;

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
		Annotations?.Clear();
		Errors?.Clear();
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

		Annotations ??= [];

		Annotations[keyword] = value;
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
		if (Annotations is null || Annotations.Count == 0) return false;
		return Annotations!.TryGetValue(keyword, out annotation);
	}

	/// <summary>
	/// Gets all annotations of a particular data type for the current evaluation level.
	/// </summary>
	/// <param name="keyword">The key under which the annotation is stored.  Typically a keyword.</param>
	/// <returns>The set of all annotations for the current evaluation level.</returns>
	public IEnumerable<JsonNode?> GetAllAnnotations(string keyword)
	{
		if (Annotations is not null && Annotations!.TryGetValue(keyword, out var annotation))
			yield return annotation;

		if (Details is null || Details.Count == 0) yield break;

		var validResults = Details.Where(x => x.IsValid && x.InstanceLocation == InstanceLocation);
		var allAnnotations = validResults.SelectMany(x => x.GetAllAnnotations(keyword));
		foreach (var nestedAnnotation in allAnnotations)
		{
			yield return nestedAnnotation;
		}
	}
}

/// <summary>
/// Default converter for <see cref="EvaluationResults"/>.
/// </summary>
public class EvaluationResultsJsonConverter : WeaklyTypedJsonConverter<EvaluationResults>
{
	/// <summary>Reads and converts the JSON to type <see cref="EvaluationResults" />.</summary>
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
				options.WriteDictionary(writer, value.AnnotationsToSerialize!, JsonSchemaSerializerContext.Default.JsonNode);
			}
		}
		else
		{
			if (value.Errors is not null && value.Errors.Count != 0)
			{
				writer.WritePropertyName("errors");
				options.WriteDictionary(writer, value.Errors!, JsonSchemaSerializerContext.Default.String);
			}
			if (value is { IncludeDroppedAnnotations: true, AnnotationsToSerialize: not null })
			{
				writer.WritePropertyName("droppedAnnotations");
				options.WriteDictionary(writer, value.AnnotationsToSerialize!, JsonSchemaSerializerContext.Default.JsonNode);
			}
		}

		if (value.Details is not null && value.Details.Count != 0)
		{
			writer.WritePropertyName("details");
			options.WriteList(writer, value.Details, JsonSchemaSerializerContext.Default.EvaluationResults);
		}

		writer.WriteEndObject();
	}
}
