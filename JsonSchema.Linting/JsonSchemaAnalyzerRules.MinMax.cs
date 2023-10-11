using Json.Schema.Data;

namespace Json.Schema.Linting;

public static partial class JsonSchemaAnalyzerRules
{
	public static readonly JsonSchema MinimumLessThanMaximum =
		new JsonSchemaBuilder()
			.Schema(Data.MetaSchemas.DataId)
			.Id("schema:json-everything:analysis:missing-type")
			.Type(SchemaValueType.Object)
			.AnyOf(
				new JsonSchemaBuilder()
					.Required(MinimumKeyword.Name, ExclusiveMinimumKeyword.Name)
					.DiagnosticMessage("`minimum` and `exclusiveMinimum` should not both be used.")
					.DiagnosticTarget(ExclusiveMinimumKeyword.Name),
				new JsonSchemaBuilder()
					.Required(MaximumKeyword.Name, ExclusiveMaximumKeyword.Name)
					.DiagnosticMessage("`maximum` and `exclusiveMaximum` should not both be used.")
					.DiagnosticTarget(ExclusiveMaximumKeyword.Name),
				new JsonSchemaBuilder()
					.Properties(
						(MaximumKeyword.Name, new JsonSchemaBuilder()
							.Data(
								(ExclusiveMaximumKeyword.Name, "/minimum")
							)
							.DiagnosticMessage("`minimum` should be less than or equal to `maximum`.")
							.DiagnosticTarget(MaximumKeyword.Name))
					)
					.Required(MinimumKeyword.Name, MaximumKeyword.Name),
				new JsonSchemaBuilder()
					.Properties(
						(ExclusiveMaximumKeyword.Name, new JsonSchemaBuilder()
							.Data(
								(MaximumKeyword.Name, "/minimum")
							)
							.DiagnosticMessage("`minimum` should be less than `exclusiveMaximum`.")
							.DiagnosticTarget(ExclusiveMaximumKeyword.Name))
					)
					.Required(MinimumKeyword.Name, ExclusiveMaximumKeyword.Name)
			);
}