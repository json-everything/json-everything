using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Experiments;

public class EvaluationResults
{
	[JsonPropertyName("valid")]
	public bool Valid { get; set; }

	[JsonPropertyName("schemaLocation")]
	public Uri SchemaLocation { get; set; }

	[JsonPropertyName("instanceLocation")]
	public JsonPointer InstanceLocation { get; set; }

	[JsonPropertyName("evaluationPath")]
	public JsonPointer EvaluationPath { get; set; }

	[JsonPropertyName("annotations")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyDictionary<string, JsonNode?>? Annotations { get; set; }

	[JsonPropertyName("errors")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyDictionary<string, string>? Errors { get; set; }

	[JsonPropertyName("details")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public EvaluationResults[]? Details { get; set; }
}

public class EvaluationResultsFlagJsonConverter : JsonConverter<EvaluationResults>
{
	public override EvaluationResults? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, EvaluationResults value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteBoolean("valid", value.Valid);
		writer.WriteEndObject();
	}
}

public class EvaluationResultsListJsonConverter : JsonConverter<EvaluationResults>
{
	public override EvaluationResults? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, EvaluationResults value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteBoolean("valid", value.Valid);

		if (value.Details is not null)
		{
			var toWrite = new Queue<EvaluationResults>(value.Details);
			while (toWrite.Count != 0)
			{
				var node = toWrite.Dequeue();
				WriteNode(writer, node, options);
				if (node.Details is null) continue;

				foreach (var detail in node.Details)
				{
					toWrite.Enqueue(detail);
				}
			}
		}

		writer.WriteEndObject();
	}

	private static void WriteNode(Utf8JsonWriter writer, EvaluationResults value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteBoolean("valid", value.Valid);
		writer.WriteString("schemaLocation", value.SchemaLocation.ToString());
		writer.WriteString("evaluationPath", value.EvaluationPath.ToString());
		if (value.Annotations is not null)
		{
			writer.WritePropertyName("annotations");
			options.WriteDictionary(writer, value.Annotations!, JsonSchemaSerializerContext.Default.JsonNode);
		}
		if (value.Errors is not null)
		{
			writer.WritePropertyName("errors");
			options.WriteDictionary(writer, value.Errors!, JsonSchemaSerializerContext.Default.String);
		}
		writer.WriteEndObject();
	}
}
