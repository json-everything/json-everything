using Json.Schema;

namespace TryJsonEverything.Services
{
	public static class InputValidationSchemas
	{
		public static readonly JsonSchema SchemaInputSchema =
			new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://json-everything.net/schemas/schema")
				.Type(SchemaValueType.Object)
				.Defs(
					("validationOptions", new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Properties(
							("outputFormat", new JsonSchemaBuilder()
								.OneOf(
									new JsonSchemaBuilder()
										.Enum(nameof(OutputFormat.Flag),
											nameof(OutputFormat.Basic),
											nameof(OutputFormat.Detailed),
											nameof(OutputFormat.Verbose)
										),
									new JsonSchemaBuilder().Type(SchemaValueType.Null)
								)
							),
							("validateAs", new JsonSchemaBuilder()
								.OneOf(
									new JsonSchemaBuilder()
										.Enum(6, 7, "6", "7", "2019-09", "2020-12"),
									new JsonSchemaBuilder().Type(SchemaValueType.Null)
								)
							),
							("defaultBaseUri", new JsonSchemaBuilder()
								.Type(SchemaValueType.String | SchemaValueType.Null)
								.Format(Formats.Uri)
							),
							("requireFormatValidation", new JsonSchemaBuilder().Type(SchemaValueType.Boolean | SchemaValueType.Null))
						)
					)
				)
				.Properties(
					("schema", new JsonSchemaBuilder()
						.Type(SchemaValueType.Object | SchemaValueType.Boolean)
					),
					("options", new JsonSchemaBuilder().Ref("#/$defs/validationOptions"))
				)
				.Required("schema", "instance");

		public static readonly JsonSchema SchemaDataGenerationInputSchema =
			new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://json-everything.net/schemas/schema-datagen")
				.Type(SchemaValueType.Object)
				.Properties(
					("schema", new JsonSchemaBuilder()
						.Type(SchemaValueType.Object | SchemaValueType.Boolean)
					)
				)
				.Required("schema");

		public static readonly JsonSchema PointerInputSchema =
			new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://json-everything.net/schemas/pointer")
				.Type(SchemaValueType.Object)
				.Properties(
					("pointer", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(Formats.JsonPointer)
					)
				)
				.Required("pointer", "data");

		public static readonly JsonSchema PathInputSchema =
			new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://json-everything.net/schemas/path")
				.Type(SchemaValueType.Object)
				.Properties(
					("path", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Format(CustomFormats.JsonPath)
					)
				)
				.Required("path", "data");

		public static readonly JsonSchema ApplyPatchInputSchema =
			new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://json-everything.net/schemas/patch")
				.Type(SchemaValueType.Object)
				.Properties(
					("patch", new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder().Ref("#/$defs/operation"))
					)
				)
				.Required("patch", "data")
				.Defs(
					("operation", new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("op", "path")
						.AllOf(new JsonSchemaBuilder().Ref("#/$defs/path"))
						.OneOf(
							new JsonSchemaBuilder()
								.Properties(
									("op", new JsonSchemaBuilder()
										.Description("The operation to perform")
										.Type(SchemaValueType.String)
										.Enum("add", "replace", "test")
									),
									("value", new JsonSchemaBuilder()
										.Description("The value to add, replace or test.")
									))
								.Required("value"),
							new JsonSchemaBuilder()
								.Properties(
									("op", new JsonSchemaBuilder()
										.Description("The operation to perform")
										.Type(SchemaValueType.String)
										.Enum("remove")
									)
								),
							new JsonSchemaBuilder()
								.Properties(
									("op", new JsonSchemaBuilder()
										.Description("The operation to perform")
										.Type(SchemaValueType.String)
										.Enum("move", "copy")
									),
									("from", new JsonSchemaBuilder()
										.Description("A JSON Pointer path pointing to the location to move/copy from.")
										.Required("from")
									)
								)
						)
					),
					("path", new JsonSchemaBuilder()
						.Properties(("path", new JsonSchemaBuilder()
								.Description("A JSON Pointer path.")
								.Type(SchemaValueType.String))
						)
					)
				);

		public static readonly JsonSchema GeneratePatchInputSchema =
			new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://json-everything.net/schemas/patch-gen")
				.Type(SchemaValueType.Object)
				.Required("start", "target");

		public static readonly JsonSchema LogicInputSchema =
			new JsonSchemaBuilder()
				.Schema(MetaSchemas.Draft202012Id)
				.Id("https://json-everything.net/schemas/logic")
				.Type(SchemaValueType.Object)
				.Properties(
					("logic", new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
					),
					("data", true)
				)
				.Required("logic", "data");
	}
}