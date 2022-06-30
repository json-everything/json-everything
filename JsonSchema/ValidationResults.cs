using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// The results object for validations.
/// </summary>
[JsonConverter(typeof(ValidationResultsJsonConverter))]
public class ValidationResults
{
	private readonly Uri _currentUri;
	private Uri? _absoluteUri;
	private readonly JsonPointer? _reference;
	private List<ValidationResults>? _nestedResults;
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
	/// The absolute schema location.  Only available if the schema had an absolute URI ID.
	/// </summary>
	public Uri? SchemaLocation => _absoluteUri ??= BuildAbsoluteUri();

	/// <summary>
	/// The collection of nested results.
	/// </summary>
	public IReadOnlyList<ValidationResults> NestedResults => _nestedResults ??= new List<ValidationResults>();

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

	public IReadOnlyDictionary<string, string>? Errors => _errors;

	/// <summary>
	/// Gets whether there are annotation.
	/// </summary>
	/// <remarks>
	/// Because <see cref="Annotations"/> is lazily loaded, this property allows the check without
	/// the side effect of allocating a list object.
	/// </remarks>
	public bool HasAnnotations => Annotations is not (null or { Count: 0 });

	public bool HasErrors => Errors is not (null or { Count: 0 });

	/// <summary>
	/// Gets the parent result.
	/// </summary>
	public ValidationResults? Parent { get; private set; }

	internal bool Exclude { get; private set; }

	internal ValidationResults(ValidationContext context)
	{
		EvaluationPath = context.EvaluationPath;
		_currentUri = context.CurrentUri;
		InstanceLocation = context.InstanceLocation;
		_reference = context.Reference;
	}

	/// <summary>
	/// Transforms the results to the `basic` format.
	/// </summary>
	public void ToBasic()
	{
		var children = GetAllChildren().ToList();
		if (!children.Any()) return;

		children.Remove(this);
		_nestedResults!.Clear();
		_nestedResults.AddRange(children.Where(x => (x.IsValid && x.HasAnnotations) || (!x.IsValid && x.HasErrors)));
	}

	/// <summary>
	/// Transforms the results to the `flag` format.
	/// </summary>
	public void ToFlag()
	{
		_nestedResults?.Clear();
		_annotations?.Clear();
		_errors?.Clear();
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
	/// Gets all annotations of a particular data type for the current validation level.
	/// </summary>
	/// <typeparam name="T">The data type.</typeparam>
	/// <param name="keyword">The key under which the annotation is stored.  Typically a keyword.</param>
	/// <returns>The set of all annotations for the current validation level.</returns>
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
	/// Marks the result as valid.
	/// </summary>
	public void Pass()
	{
		//IsValid = true;
	}

	/// <summary>
	/// Marks the result as invalid.
	/// </summary>
	/// <param name="message">(optional) An error message.</param>
	/// <remarks>
	/// For better support for customization, consider using the overload that takes parameters.
	/// </remarks>
	public void Fail(string keyword, string? message = null)
	{
		IsValid = false;
		if (message == null) return;

		_errors ??= new();
		_errors[keyword] = message;
	}

	/// <summary>
	/// Marks the result as invalid.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="parameters">Parameters to replace in the message.</param>
	public void Fail(string keyword, string message, params (string token, object? value)[] parameters)
	{
		IsValid = false;
		_errors ??= new();
		_errors[keyword] = message.ReplaceTokens(parameters);
	}

	internal void AddNestedResult(ValidationResults results)
	{
		_nestedResults ??= new List<ValidationResults>();
		_nestedResults.Add(results);
		results.Parent = this;
	}

	internal void Ignore()
	{
		IsValid = true;
		Exclude = true;
	}

	private Uri? BuildAbsoluteUri()
	{
		return BuildAbsoluteUri(EvaluationPath);
	}

	private Uri? BuildAbsoluteUri(JsonPointer pointer)
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		if (_currentUri == null || !_currentUri.IsAbsoluteUri) return null;
		if (pointer.Segments.All(s => s.Value != RefKeyword.Name &&
		                              s.Value != RecursiveRefKeyword.Name))
		{
			return new Uri(_currentUri, JsonPointer.Create(pointer.Segments, true).ToString());
		}

		var lastIndexOfRef = pointer.Segments
			.Select((s, i) => (s, i))
			.Last(s => s.s.Value is RefKeyword.Name or RecursiveRefKeyword.Name).i;
		var absoluteSegments = pointer.Segments.Skip(lastIndexOfRef + 1);

		if (_reference != null)
			absoluteSegments = _reference.Segments.Concat(absoluteSegments);

		return new Uri(_currentUri, JsonPointer.Create(absoluteSegments, true).ToString());
	}

	private IEnumerable<ValidationResults> GetAllChildren()
	{
		var all = new List<ValidationResults>();
		var toProcess = new Queue<ValidationResults>();

		toProcess.Enqueue(this);
		while (toProcess.Any())
		{
			var current = toProcess.Dequeue();
			all.Add(current);
			if (!current.HasNestedResults) continue;
			
			foreach (var nestedResult in current.NestedResults)
			{
				toProcess.Enqueue(nestedResult);
			}
			current._nestedResults?.Clear();
		}

		// we still include the root because it may have annotations
		// don't report annotations at the root of the output
		return all;
	}
}

internal class ValidationResultsJsonConverter : JsonConverter<ValidationResults>
{
	public override ValidationResults Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, ValidationResults value, JsonSerializerOptions options)
	{
		if (value.Exclude) return;

		writer.WriteStartObject();

		writer.WriteBoolean("valid", value.IsValid);

		writer.WritePropertyName("evaluationPath");
		JsonSerializer.Serialize(writer, value.EvaluationPath, options);

		if (value.SchemaLocation != null)
		{
			writer.WritePropertyName("schemaLocation");
			JsonSerializer.Serialize(writer, value.SchemaLocation, options);
		}

		writer.WritePropertyName("instanceLocation");
		JsonSerializer.Serialize(writer, value.InstanceLocation, options);

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