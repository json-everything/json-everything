using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// The results object for evaluations.
/// </summary>
[JsonConverter(typeof(EvaluationResultsJsonConverter))]
public class EvaluationResults
{
	private readonly Uri _currentUri;
	private readonly JsonPointer? _reference;
	private Uri? _schemaLocation;
	private List<EvaluationResults>? _nestedResults;
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
	/// If the schema did not have an absolute `$id`, the value from
	/// <see cref="EvaluationOptions.DefaultBaseUri"/> will be used.
	/// </remarks>
	public Uri SchemaLocation => _schemaLocation ??= BuildSchemaLocation();

	/// <summary>
	/// The collection of nested results.
	/// </summary>
	public IReadOnlyList<EvaluationResults> NestedResults => _nestedResults ??= new List<EvaluationResults>();

	/// <summary>
	/// Gets whether there are nested results.
	/// </summary>
	/// <remarks>
	/// Because <see cref="NestedResults"/> is lazily loaded, this property allows the check without
	/// the side effect of allocating a list object.
	/// </remarks>
	public bool HasNestedResults => _nestedResults is not (null or{ Count: 0 });

	/// <summary>
	/// The collection of annotations from this node.
	/// </summary>
	public IReadOnlyDictionary<string, JsonNode?>? Annotations => _annotations;

	/// <summary>
	/// The collection of error from this node.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Errors => _errors;

	/// <summary>
	/// Gets whether this node has annotations.
	/// </summary>
	public bool HasAnnotations => Annotations is not (null or { Count: 0 });

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

	internal EvaluationResults(EvaluationContext context)
	{
		EvaluationPath = context.EvaluationPath;
		_currentUri = context.CurrentUri;
		_reference = context.Reference;
		InstanceLocation = context.InstanceLocation;
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

		var fragment = _reference ?? JsonPointer.UrlEmpty;
		fragment = fragment.Combine(EvaluationPath.Segments.Skip(localEvaluationPathStart).ToArray());

		return new Uri(_currentUri, fragment.ToString());
	}

	/// <summary>
	/// Transforms the results to the `basic` format.
	/// </summary>
	public void ToBasic()
	{
		var children = GetAllChildren().ToList();
		if (!children.Any()) return;

		children.Remove(this);
		children.Insert(0, new EvaluationResults(this) { Parent = this });
		_annotations?.Clear();
		_errors?.Clear();
		if (_nestedResults == null)
			_nestedResults = new List<EvaluationResults>();
		else
			_nestedResults.Clear();
		_nestedResults.AddRange(children.Where(x => (x.IsValid && x.HasAnnotations) || (!x.IsValid && x.HasErrors)));
		Format = OutputFormat.Basic;
	}

	private IEnumerable<EvaluationResults> GetAllChildren()
	{
		var all = new List<EvaluationResults>();
		var toProcess = new Queue<EvaluationResults>();

		toProcess.Enqueue(this);
		while (toProcess.Any())
		{
			var current = toProcess.Dequeue();
			all.Add(current);
			if (!current.HasNestedResults) continue;

			foreach (var nestedResult in current.NestedResults.Where(x => x.IsValid == current.IsValid))
			{
				toProcess.Enqueue(nestedResult);
			}
			current._nestedResults?.Clear();
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
		_nestedResults?.Clear();
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
		_annotations ??= new();

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

		if (!HasNestedResults) yield break;

		var validResults = NestedResults.Where(x => x.IsValid && x.InstanceLocation == InstanceLocation);
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
	/// <param name="message">(optional) An error message.</param>
	/// <remarks>
	/// For better support for customization, consider using the overload that takes parameters.
	/// </remarks>
	public void Fail(string keyword, string? message)
	{
		IsValid = false;
		if (message == null) return;

		_errors ??= new();
		_errors[keyword] = message;
	}

	/// <summary>
	/// Marks the result as invalid.
	/// </summary>
	/// <param name="keyword">The keyword that failed validation.</param>
	/// <param name="message">The error message.</param>
	/// <param name="parameters">Parameters to replace in the message.</param>
	public void Fail(string keyword, string message, params (string token, object? value)[] parameters)
	{
		IsValid = false;
		_errors ??= new();
		_errors[keyword] = message.ReplaceTokens(parameters);
	}

	internal void AddNestedResult(EvaluationResults results)
	{
		_nestedResults ??= new List<EvaluationResults>();
		_nestedResults.Add(results);
		results.Parent = this;
	}

	internal void Ignore()
	{
		IsValid = true;
		Exclude = true;
	}
}

internal class EvaluationResultsJsonConverter : JsonConverter<EvaluationResults>
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
			JsonSerializer.Serialize(writer, value.EvaluationPath, options);

			// this can still be null if the root schema is a boolean
			if (value.SchemaLocation != null!)
			{
				writer.WritePropertyName("schemaLocation");
				JsonSerializer.Serialize(writer, value.SchemaLocation, options);
			}

			writer.WritePropertyName("instanceLocation");
			JsonSerializer.Serialize(writer, value.InstanceLocation, options);
		}

		if (value.IsValid)
		{
			if (value.HasAnnotations)
			{
				writer.WritePropertyName("annotations");
				JsonSerializer.Serialize(writer, value.Annotations, options);
			}
		}
		else if (value.HasErrors)
		{
			writer.WritePropertyName("errors");
			JsonSerializer.Serialize(writer, value.Errors, options);
		}

		if (value.HasNestedResults)
		{
			writer.WritePropertyName("nested");
			JsonSerializer.Serialize(writer, value.NestedResults, options);
		}

		writer.WriteEndObject();
	}
}

/// <summary>
/// Produces output formats specified by 2019-09 and 2020-12.
/// </summary>
public class Pre202012EvaluationResultsJsonConverter : JsonConverter<EvaluationResults>
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

	/// <summary>Reads and converts the JSON to type <typeparamref name="T" />.</summary>
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

		writer.WritePropertyName("keywordLocation");
		JsonSerializer.Serialize(writer, value.EvaluationPath, options);

		if (value.SchemaLocation != null)
		{
			writer.WritePropertyName("absoluteKeywordLocation");
			JsonSerializer.Serialize(writer, value.SchemaLocation, options);
		}

		writer.WritePropertyName("instanceLocation");
		JsonSerializer.Serialize(writer, value.InstanceLocation, options);

		if (!value.IsValid)
		{
			if (value.HasErrors && value.Errors!.TryGetValue(string.Empty, out var localError))
				writer.WriteString("error", localError);

			if ((value.HasErrors && value.Errors!.Any(x => x.Key != string.Empty)) || value.NestedResults.Any())
			{
				writer.WritePropertyName("errors");
				JsonSerializer.Serialize(writer, value.NestedResults, options);

				if (value.HasErrors)
				{
					foreach (var error in value.Errors!)
					{
						WriteError(writer, value, error.Key, error.Value, options);
					}
				}
			}
		}
		else if ((value.HasAnnotations && value.Annotations!.Any()) || value.NestedResults.Any())
		{
			writer.WritePropertyName("annotations");
			writer.WriteStartArray();

			var annotations = value.Annotations.Select(x => new Annotation(x.Key, x.Value, value.EvaluationPath.Combine(x.Key))).ToList();

			foreach (var result in value.NestedResults)
			{
				var annotation = annotations.SingleOrDefault(a => a.Source.Equals(result.EvaluationPath));
				if (annotation != null)
				{
					WriteAnnotation(writer, value, annotation, options);
				}
				else
				{
					JsonSerializer.Serialize(writer, result, options);
				}
			}

			foreach (var annotation in annotations)
			{
				WriteAnnotation(writer, value, annotation, options);
			}

			writer.WriteEndArray();
		}

		writer.WriteEndObject();
	}

	private static void WriteError(Utf8JsonWriter writer, EvaluationResults value, string keyword, string error, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteBoolean("valid", value.IsValid);

		writer.WritePropertyName("keywordLocation");
		JsonSerializer.Serialize(writer, value.EvaluationPath.Combine(keyword), options);

		if (value.SchemaLocation != null)
		{
			writer.WritePropertyName("absoluteKeywordLocation");
			JsonSerializer.Serialize(writer, value.SchemaLocation.OriginalString + $"/{keyword}", options);
		}

		writer.WritePropertyName("instanceLocation");
		JsonSerializer.Serialize(writer, value.InstanceLocation, options);

		writer.WritePropertyName("error");
		JsonSerializer.Serialize(writer, error, options);

		writer.WriteEndObject();
	}

	private static void WriteAnnotation(Utf8JsonWriter writer, EvaluationResults value, Annotation annotation, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteBoolean("valid", value.IsValid);

		writer.WritePropertyName("keywordLocation");
		JsonSerializer.Serialize(writer, annotation.Source, options);

		if (value.SchemaLocation != null)
		{
			writer.WritePropertyName("absoluteKeywordLocation");
			JsonSerializer.Serialize(writer, value.SchemaLocation.OriginalString + $"/{annotation.Owner}", options);
		}

		writer.WritePropertyName("instanceLocation");
		JsonSerializer.Serialize(writer, value.InstanceLocation, options);

		writer.WritePropertyName("annotation");
		JsonSerializer.Serialize(writer, annotation.Value, options);

		writer.WriteEndObject();
	}
}