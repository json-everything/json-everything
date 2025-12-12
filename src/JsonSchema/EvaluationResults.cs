using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;
using Json.Schema.Keywords;

namespace Json.Schema;

/// <summary>
/// The results object for evaluations.
/// </summary>
[Serializable]
[JsonConverter(typeof(EvaluationResultsJsonConverter))]
public class EvaluationResults
{
	private readonly Uri _currentUri;
	private readonly HashSet<string>? _backgroundAnnotations;
	private Uri? _schemaLocation;

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
	public Dictionary<string, JsonElement>? Annotations { get; set; }

	/// <summary>
	/// The collection of error from this node.
	/// </summary>
	public Dictionary<string, string>? Errors { get; set; }

	/// <summary>
	/// Gets the parent result.
	/// </summary>
	public EvaluationResults? Parent { get; set; }

	internal OutputFormat Format { get; private set; } = OutputFormat.Hierarchical;

	internal bool IncludeDroppedAnnotations { get; }

	internal IReadOnlyDictionary<string, JsonElement>? AnnotationsToSerialize =>
		Annotations is not null && Annotations.Count != 0
			? Annotations.Where(x => !(_backgroundAnnotations?.Contains(x.Key) ?? false)).ToDictionary(x => x.Key, x => x.Value)
			: null;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	internal EvaluationResults(JsonPointer evaluationPath, Uri schemaLocation, JsonPointer instanceLocation, EvaluationOptions options)
	{
		EvaluationPath = evaluationPath;
		_schemaLocation = schemaLocation;
		InstanceLocation = instanceLocation;

		IncludeDroppedAnnotations = options.PreserveDroppedAnnotations;
		//if (options.IgnoredAnnotations != null)
		//{
		//	_backgroundAnnotations = new HashSet<string>(options.IgnoredAnnotations.Where(x => x.ProducesDependentAnnotations()).Select(x => x.Keyword()));
		//}
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

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
		_backgroundAnnotations = other._backgroundAnnotations;
	}

	private Uri BuildSchemaLocation()
	{
		var localEvaluationPathSegmentCount = EvaluationPath.SegmentCount;
		for (var i = EvaluationPath.SegmentCount - 1; i >= 0; i--)
		{
			var segment = EvaluationPath[i];
			if (segment == RefKeyword.Instance.Name ||
			    segment == Keywords.Draft201909.RecursiveRefKeyword.Instance.Name ||
			    segment == DynamicRefKeyword.Instance.Name)
			{
				localEvaluationPathSegmentCount = i + 1;
				break;
			}
		}

		JsonPointer fragment; // 2 allocations
		if (localEvaluationPathSegmentCount == 0)
			fragment = JsonPointer.Empty;
		else if (localEvaluationPathSegmentCount == EvaluationPath.SegmentCount)
			fragment = EvaluationPath;
		else
			fragment = EvaluationPath.GetLocal(localEvaluationPathSegmentCount);

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
		if (Details == null)
			Details = [];
		else
			Details.Clear();
		foreach (var child in children)
		{
			child.Details?.Clear();
			child.Format = OutputFormat.List;
		}
		//_details.AddRange(children.Where(x => (x.IsValid && x.HasAnnotations) || (!x.IsValid && x.HasErrors)));
		Details.AddRange(children);
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
			current.Details?.Clear();
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
		Details?.Clear();
		Annotations?.Clear();
		Errors?.Clear();
		Format = OutputFormat.Flag;
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
				options.WriteDictionary(writer, value.AnnotationsToSerialize!, JsonSchemaSerializerContext.Default.JsonElement);
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
				options.WriteDictionary(writer, value.AnnotationsToSerialize!, JsonSchemaSerializerContext.Default.JsonElement);
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
