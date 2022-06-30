using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;
// ReSharper disable ConditionIsAlwaysTrueOrFalse
#pragma warning disable CS8618

namespace Json.Patch.Tests.Suite;

[JsonConverter(typeof(JsonPatchTestJsonConverter))]
public class JsonPatchTest
{
	public static readonly JsonSchema TestSchema = new JsonSchemaBuilder()
		.Schema(MetaSchemas.Draft202012Id)
		.Defs(
			("operationType", new JsonSchemaBuilder().Enum(
					"add",
					"remove",
					"replace",
					"move",
					"copy",
					"test"
				)
			)
		)
		.Type(SchemaValueType.Object)
		.Properties(
			("doc", true),
			("expected", true),
			("patch", new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.Properties(
						("op", new JsonSchemaBuilder().Ref("#/$defs/operationType")),
						("path", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Formats.JsonPointer)
						),
						("from", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Formats.JsonPointer)
						),
						("value", true)
					)
					.Required("op")
					.OneOf(
						new JsonSchemaBuilder()
							.Properties(("op", new JsonSchemaBuilder().Const("add")))
							.Required("path", "value"),
						new JsonSchemaBuilder()
							.Properties(("op", new JsonSchemaBuilder().Const("remove")))
							.Required("path"),
						new JsonSchemaBuilder()
							.Properties(("op", new JsonSchemaBuilder().Const("replace")))
							.Required("path", "value"),
						new JsonSchemaBuilder()
							.Properties(("op", new JsonSchemaBuilder().Const("move")))
							.Required("path", "from"),
						new JsonSchemaBuilder()
							.Properties(("op", new JsonSchemaBuilder().Const("copy")))
							.Required("path", "from"),
						new JsonSchemaBuilder()
							.Properties(("op", new JsonSchemaBuilder().Const("test")))
							.Required("path", "value")
					)
				)
			),
			("comment", new JsonSchemaBuilder().Type(SchemaValueType.String)),
			("error", new JsonSchemaBuilder().Type(SchemaValueType.String)),
			("disabled", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
		);

	public JsonNode? Doc { get; set; }
	public JsonNode? ExpectedValue { get; set; }
	public string? Error { get; set; }
	public string? Comment { get; set; }
	public JsonPatch? Patch { get; set; }
	public bool Disabled { get; set; }

	public bool ExpectsError => Error != null;
	public bool HasExpectedValue { get; set; }
}

public class JsonPatchTestJsonConverter : JsonConverter<JsonPatchTest?>
{
	private class Model
	{
		public JsonNode? Doc { get; set; }
		public JsonElement Expected { get; set; }
		public string? Error { get; set; }
		public string? Comment { get; set; }
		public JsonPatch Patch { get; set; }
		public bool Disabled { get; set; }
	}

	public override JsonPatchTest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonSerializer.Deserialize<JsonNode?>(ref reader, options);

		var results = JsonPatchTest.TestSchema.Validate(node, new ValidationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			RequireFormatValidation = true
		});
		if (results.IsValid)
		{
			var model = node.Deserialize<Model>(options)!;
			return new JsonPatchTest
			{
				Doc = model.Doc,
				ExpectedValue = model.Expected.AsNode(),
				HasExpectedValue = model.Expected.ValueKind != JsonValueKind.Undefined,
				Error = model.Error,
				Comment = model.Comment,
				Patch = model.Patch,
				Disabled = model.Disabled
			};
		}

		Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
		return null;
	}

	public override void Write(Utf8JsonWriter writer, JsonPatchTest? value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value!.Doc != null)
		{
			writer.WritePropertyName("doc");
			value.Doc.WriteTo(writer);
		}
		if (value.HasExpectedValue)
		{
			writer.WritePropertyName("expected");
			JsonSerializer.Serialize(writer, value.ExpectedValue, options);
		}
		if (value.Error != null)
			writer.WriteString("error", value.Error);
		if (value.Comment != null)
			writer.WriteString("comment", value.Comment);
		if (value.Patch != null)
		{
			writer.WritePropertyName("patch");
			JsonSerializer.Serialize(writer, value.Patch, options);
		}
		if (value.Disabled)
			writer.WriteBoolean("disabled", value.Disabled);

		writer.WriteEndObject();
	}
}