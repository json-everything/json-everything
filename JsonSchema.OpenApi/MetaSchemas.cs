using System;

namespace Json.Schema.OpenApi;

/// <summary>
/// Provides meta-schemas defined by OpenAPI.
/// </summary>
public static class MetaSchemas
{
	/// <summary>
	/// The URI ID of the dialect meta-schema.
	/// </summary>
	public static readonly Uri OpenApiDialectId = new("https://spec.openapis.org/oas/3.1/dialect/base");
	/// <summary>
	/// The URI ID of the validation meta-schema.
	/// </summary>
	public static readonly Uri OpenApiMetaId = new("https://spec.openapis.org/oas/3.1/meta/base");

	/// <summary>
	/// The dialect meta-schema.
	/// </summary>
	public static readonly JsonSchema OpenApiDialect =
		new JsonSchemaBuilder()
			.Id(OpenApiDialectId)
			.Schema(Schema.MetaSchemas.Draft202012Id)
			.Title("OpenAPI 3.1 Schema Object Dialect")
			.Description("A JSON Schema dialect describing schemas found in OpenAPI documents")
			.Vocabulary(
				(Schema.Vocabularies.Core202012Id, true),
				(Schema.Vocabularies.Applicator202012Id, true),
				(Schema.Vocabularies.Unevaluated202012Id, true),
				(Schema.Vocabularies.Validation202012Id, true),
				(Schema.Vocabularies.Metadata202012Id, true),
				(Schema.Vocabularies.FormatAnnotation202012Id, true),
				(Schema.Vocabularies.Content202012Id, true),
				(Vocabularies.OpenApiId, true)
			)
			.DynamicAnchor("meta")
			.AllOf(
				new JsonSchemaBuilder().Ref(Schema.MetaSchemas.Draft202012Id),
				new JsonSchemaBuilder().Ref(OpenApiMetaId)
			);

	/// <summary>
	/// The validation meta-schema.
	/// </summary>
	public static readonly JsonSchema OpenApiMeta =
		new JsonSchemaBuilder()
			.Id(OpenApiMetaId)
			.Schema(Schema.MetaSchemas.Draft202012Id)
			.Title("OAS Base vocabulary")
			.Description("A JSON Schema Vocabulary used in the OpenAPI Schema Dialect")
			.Vocabulary(
				(Vocabularies.OpenApiId, true)
			)
			.DynamicAnchor("meta")
			.Type(SchemaValueType.Object | SchemaValueType.Boolean)
			.Properties(
				("example", true),
				("discriminator", new JsonSchemaBuilder().Ref("#/$defs/discriminator")),
				("externalDocs", new JsonSchemaBuilder().Ref("#/$defs/external-docs")),
				("xml", new JsonSchemaBuilder().Ref("#/$defs/xml"))
			)
			.Defs(
				("extensible", new JsonSchemaBuilder()
					.PatternProperties(
						("^x-", true)
					)
				),
				("discriminator", new JsonSchemaBuilder()
					.Ref("#/$defs/extensible")
					.Type(SchemaValueType.Object)
					.Properties(
						("propertyName", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
						),
						("mapping", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder()
								.Type(SchemaValueType.String)
							)
						)
					)
					.Required("propertyName")
					.UnevaluatedProperties(false)
				),
				("external-docs", new JsonSchemaBuilder()
					.Ref("#/$defs/extensible")
					.Type(SchemaValueType.Object)
					.Properties(
						("url", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Formats.UriReference)
						),
						("description", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
						)
					)
					.Required("url")
					.UnevaluatedProperties(false)
				),
				("xml", new JsonSchemaBuilder()
					.Ref("#/$defs/extensible")
					.Type(SchemaValueType.Object)
					.Properties(
						("name", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
						),
						("namespace", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Formats.Uri)
						),
						("prefix", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
						),
						("attribute", new JsonSchemaBuilder()
							.Type(SchemaValueType.Boolean)
						),
						("wrapped", new JsonSchemaBuilder()
							.Type(SchemaValueType.Boolean)
						)
					)
				)
			);
}