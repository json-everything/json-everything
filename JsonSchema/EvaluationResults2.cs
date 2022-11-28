using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// The results object for evaluations.
/// </summary>
[JsonConverter(typeof(EvaluationResults2JsonConverter))]
public class EvaluationResults2
{
	private List<EvaluationResults2>? _nestedResults;
	private Dictionary<string, JsonNode?>? _annotations;
	private Dictionary<string, string>? _errors;

	/// <summary>
	/// Indicates whether the validation passed or failed.
	/// </summary>
	public bool IsValid { get; internal set; }
	/// <summary>
	/// The schema location that generated this node.
	/// </summary>
	public JsonPointer EvaluationPath { get; internal set; }
	/// <summary>
	/// The instance location that was processed.
	/// </summary>
	public JsonPointer InstanceLocation { get; internal set; }

	/// <summary>
	/// The absolute schema location.
	/// </summary>
	/// <remarks>
	/// If the schema did not have an absolute `$id`, the value from
	/// <see cref="EvaluationOptions.DefaultBaseUri"/> will be used.
	/// </remarks>
	public Uri SchemaLocation { get; internal set; }

	/// <summary>
	/// The collection of nested results.
	/// </summary>
	public List<EvaluationResults2> NestedResults => _nestedResults ??= new List<EvaluationResults2>();

	/// <summary>
	/// Gets whether there are nested results.
	/// </summary>
	/// <remarks>
	/// Because <see cref="NestedResults"/> is lazily loaded, this property allows the check without
	/// the side effect of allocating a list object.
	/// </remarks>
	public bool HasNestedResults => _nestedResults is not (null or { Count: 0 });

	/// <summary>
	/// The collection of annotations from this node.
	/// </summary>
	public Dictionary<string, JsonNode?> Annotations => _annotations ??= new Dictionary<string, JsonNode?>();

	/// <summary>
	/// Gets whether this node has annotations.
	/// </summary>
	public bool HasAnnotations => Annotations is not (null or { Count: 0 });

	/// <summary>
	/// The collection of error from this node.
	/// </summary>
	public Dictionary<string, string> Errors => _errors ??= new Dictionary<string, string>();

	/// <summary>
	/// Gets whether this node has errors.
	/// </summary>
	public bool HasErrors => Errors is not (null or { Count: 0 });

	internal bool IncludeDroppedAnnotations { get; private set; }

	internal EvaluationResults2() { }
}

internal class EvaluationResults2JsonConverter : JsonConverter<EvaluationResults2>
{
	public override EvaluationResults2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, EvaluationResults2 value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteBoolean("valid", value.IsValid);

		if (value.EvaluationPath != null!)
		{
			writer.WritePropertyName("evaluationPath");
			JsonSerializer.Serialize(writer, value.EvaluationPath, options);
		}

		if (value.SchemaLocation != null!)
		{
			writer.WritePropertyName("schemaLocation");
			JsonSerializer.Serialize(writer, value.SchemaLocation, options);
		}

		if (value.InstanceLocation != null!)
		{
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
		else
		{
			if (value.HasErrors)
			{
				writer.WritePropertyName("errors");
				JsonSerializer.Serialize(writer, value.Errors, options);
			}
			if (value.IncludeDroppedAnnotations && value.HasAnnotations)
			{
				writer.WritePropertyName("droppedAnnotations");
				JsonSerializer.Serialize(writer, value.Annotations, options);
			}
		}

		if (value.HasNestedResults)
		{
			writer.WritePropertyName("details");
			JsonSerializer.Serialize(writer, value.NestedResults, options);
		}

		writer.WriteEndObject();
	}
}
