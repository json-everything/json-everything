using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;
// ReSharper disable ConditionIsAlwaysTrueOrFalse
#pragma warning disable CS8618

namespace Json.Patch.Tests.Suite
{
	[JsonConverter(typeof(JsonPatchTestJsonConverter))]
	public class JsonPatchTest
	{
		public static readonly JsonSchema TestSchema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft202012Id)
			.Defs(
				("operationType", new JsonSchemaBuilder().Enum(
						"add".AsJsonElement(),
						"remove".AsJsonElement(),
						"replace".AsJsonElement(),
						"move".AsJsonElement(),
						"copy".AsJsonElement(),
						"test".AsJsonElement()
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
								.Properties(("op", new JsonSchemaBuilder().Const("add".AsJsonElement())))
								.Required("path", "value"),
							new JsonSchemaBuilder()
								.Properties(("op", new JsonSchemaBuilder().Const("remove".AsJsonElement())))
								.Required("path"),
							new JsonSchemaBuilder()
								.Properties(("op", new JsonSchemaBuilder().Const("replace".AsJsonElement())))
								.Required("path", "value"),
							new JsonSchemaBuilder()
								.Properties(("op", new JsonSchemaBuilder().Const("move".AsJsonElement())))
								.Required("path", "from"),
							new JsonSchemaBuilder()
								.Properties(("op", new JsonSchemaBuilder().Const("copy".AsJsonElement())))
								.Required("path", "from"),
							new JsonSchemaBuilder()
								.Properties(("op", new JsonSchemaBuilder().Const("test".AsJsonElement())))
								.Required("path", "value")
						)
					)
				),
				("comment", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("error", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("disabled", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			);

		public JsonElement Doc { get; set; }
		public JsonElement ExpectedValue { get; set; }
		public string? Error { get; set; }
		public string? Comment { get; set; }
		public JsonPatch Patch { get; set; }
		public bool Disabled { get; set; }

		public bool ExpectsError => Error != null;
		public bool HasExpectedValue => ExpectedValue.ValueKind != JsonValueKind.Undefined;
	}

	public class JsonPatchTestJsonConverter : JsonConverter<JsonPatchTest?>
	{
		private class Model
		{
			public JsonElement Doc { get; set; }
			public JsonElement Expected { get; set; }
			public string? Error { get; set; }
			public string? Comment { get; set; }
			public JsonPatch Patch { get; set; }
			public bool Disabled { get; set; }
		}

		public override JsonPatchTest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

			var results = JsonPatchTest.TestSchema.Validate(element, new ValidationOptions
			{
				OutputFormat = OutputFormat.Detailed,
				RequireFormatValidation = true
			});
			if (results.IsValid)
			{
				var model = JsonSerializer.Deserialize<Model>(element.GetRawText(), options)!;
				return new JsonPatchTest
				{
					Doc = model.Doc.ValueKind == JsonValueKind.Undefined ? default : model.Doc.Clone(),
					ExpectedValue = model.Expected.ValueKind == JsonValueKind.Undefined ? default : model.Expected.Clone(),
					Error = model.Error,
					Comment = model.Comment,
					Patch = model.Patch,
					Disabled = model.Disabled
				};
			}

			Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions{WriteIndented = true}));
			return null;
		}

		public override void Write(Utf8JsonWriter writer, JsonPatchTest? value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			if (value!.Doc.ValueKind != JsonValueKind.Undefined)
			{
				writer.WritePropertyName("doc");
				value.Doc.WriteTo(writer);
			}
			if (value.ExpectedValue.ValueKind != JsonValueKind.Undefined)
			{
				writer.WritePropertyName("expected");
				value.ExpectedValue.WriteTo(writer);
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
}