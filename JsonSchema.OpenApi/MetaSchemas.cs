using System;
using System.Text.Json.Nodes;

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

	public static readonly Uri OpenApiDocumentSchemaId = new("https://spec.openapis.org/oas/3.1/schema/2022-02-27");

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
							.Format(Schema.Formats.UriReference)
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
							.Format(Schema.Formats.Uri)
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

	public static readonly JsonSchema DocumentSchema =
		new JsonSchemaBuilder()
			.Id(OpenApiDocumentSchemaId)
			.Schema(Schema.MetaSchemas.Draft202012Id)
			.Description("The description of OpenAPI v3.1.x documents without schema validation, as defined by https://spec.openapis.org/oas/v3.1.0")
			.Type(SchemaValueType.Object)
			.Properties(
				("openapi", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Pattern("^3\\.1\\.\\d+(-.+)?$")
				),
				("info", new JsonSchemaBuilder().Ref("#/$defs/info")),
				("jsonSchemaDialect", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Schema.Formats.Uri)
					.Default("https://spec.openapis.org/oas/3.1/dialect/base")
				),
				("servers", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Ref("#/$defs/server"))
					.Default(new JsonArray(new JsonObject { ["url"] = "/" }))
				),
				("paths", new JsonSchemaBuilder().Ref("#/$defs/paths")),
				("webhooks", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/path-item-or-reference"))
				),
				("components", new JsonSchemaBuilder().Ref("#/$defs/components")),
				("security", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Ref("#/$defs/security-requirement"))
				),
				("tags", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Ref("#/$defs/tag"))
				),
				("externalDocs", new JsonSchemaBuilder().Ref("#/$defs/external-documentation"))
			)
			.Required("openapi", "info")
			.AnyOf(
				new JsonSchemaBuilder().Required("paths"),
				new JsonSchemaBuilder().Required("components"),
				new JsonSchemaBuilder().Required("webhooks")
			)
			.Ref("#/$defs/specification-extensions")
			.UnevaluatedProperties(false)
			.Defs(
				("info", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#info-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("title", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("summary", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("termsOfService", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.Uri)
						),
						("contact", new JsonSchemaBuilder().Ref("#/$defs/contact")),
						("license", new JsonSchemaBuilder().Ref("#/$defs/license")),
						("version", new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
					.Required("title", "version")
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("contact", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#contact-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("name", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("url", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.Uri)
						),
						("email", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.Email)
						)
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("license", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#license-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("name", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("identifier", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("url", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.Uri)
						)
					)
					.Required("name")
					.OneOf(
						new JsonSchemaBuilder().Required("identifier"),
						new JsonSchemaBuilder().Required("url")
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("server", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#server-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("url", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.UriReference)
						),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("variables", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/server-variable"))
						)
					)
					.Required("url")
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("server-variable", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#server-variable-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("enum", new JsonSchemaBuilder()
							.Type(SchemaValueType.Array)
							.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
							.MinItems(1)
						),
						("default", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
					.Required("default")
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("components", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#components-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("schemas", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().DynamicRef("#meta"))
						),
						("responses", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/response-or-reference"))
						),
						("parameters", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/parameter-or-reference"))
						),
						("examples", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/example-or-reference"))
						),
						("requestBodies", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/request-body-or-reference"))
						),
						("headers", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/header-or-reference"))
						),
						("securitySchemes", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/security-scheme-or-reference"))
						),
						("links", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/link-or-reference"))
						),
						("callbacks", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/callbacks-or-reference"))
						),
						("pathItems", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/path-item-or-reference"))
						)
					)
					.PatternProperties(
						("^(schemas|responses|parameters|examples|requestBodies|headers|securitySchemes|links|callbacks|pathItems)$",
							new JsonSchemaBuilder()
								.Comment("Enumerating all of the property names in the regex above is necessary for unevaluatedProperties to work as expected")
								.PropertyNames(new JsonSchemaBuilder().Pattern("^[a-zA-Z0-9._-]+$"))
						)
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("paths", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#paths-object")
					.Type(SchemaValueType.Object)
					.PatternProperties(
						("^/", new JsonSchemaBuilder().Ref("#/$defs/path-item"))
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("path-item", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#path-item-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("summary", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("servers", new JsonSchemaBuilder()
							.Type(SchemaValueType.Array)
							.Items(new JsonSchemaBuilder().Ref("#/$defs/server"))
						),
						("parameters", new JsonSchemaBuilder()
							.Type(SchemaValueType.Array)
							.Items(new JsonSchemaBuilder().Ref("#/$defs/parameter-or-reference"))
						)
					)
					.PatternProperties(
						("^(get|put|post|delete|options|head|patch|trace)$",
							new JsonSchemaBuilder().Ref("#/$defs/operation")
						)
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("path-item-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/path-item"))
				),
				("operation", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#operation-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("tags", new JsonSchemaBuilder()
							.Type(SchemaValueType.Array)
							.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
						),
						("summary", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("externalDocs", new JsonSchemaBuilder().Ref("#/$defs/external-documentation")),
						("operationId", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("parameters", new JsonSchemaBuilder()
							.Type(SchemaValueType.Array)
							.Items(new JsonSchemaBuilder().Ref("#/$defs/parameter-or-reference"))
						),
						("requestBody", new JsonSchemaBuilder().Ref("#/$defs/request-body-or-reference")),
						("responses", new JsonSchemaBuilder().Ref("#/$defs/responses")),
						("callbacks", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/callbacks-or-reference"))
						),
						("deprecated", new JsonSchemaBuilder()
							.Default(false)
							.Type(SchemaValueType.Boolean)
						),
						("security", new JsonSchemaBuilder()
							.Type(SchemaValueType.Array)
							.Items(new JsonSchemaBuilder().Ref("#/$defs/security-requirement"))
						),
						("servers", new JsonSchemaBuilder()
							.Type(SchemaValueType.Array)
							.Items(new JsonSchemaBuilder().Ref("#/$defs/server"))
						)
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("external-documentation", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#external-documentation-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("url", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.Uri)
						)
					)
					.Required("url")
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("parameter", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#parameter-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("name", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("in", new JsonSchemaBuilder().Enum("query", "header", "path", "cookie")),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("required", new JsonSchemaBuilder()
							.Default(false)
							.Type(SchemaValueType.Boolean)
						),
						("deprecated", new JsonSchemaBuilder()
							.Default(false)
							.Type(SchemaValueType.Boolean)
						),
						("schema", new JsonSchemaBuilder().DynamicRef("#meta")),
						("content", new JsonSchemaBuilder()
							.Ref("#/$defs/content")
							.MinProperties(1)
							.MaxProperties(1)
						)
					)
					.Required("name", "in")
					.OneOf(
						new JsonSchemaBuilder().Required("schema"),
						new JsonSchemaBuilder().Required("content")
					)
					.If(new JsonSchemaBuilder()
						.Properties(
							("in", new JsonSchemaBuilder().Const("query"))
						)
						.Required("in")
					)
					.Then(new JsonSchemaBuilder()
						.Properties(
							("allowEmptyValue", new JsonSchemaBuilder()
								.Default(false)
								.Type(SchemaValueType.Boolean)
							)
						)
					)
					.DependentSchemas(
						("schema", new JsonSchemaBuilder()
							.Properties(
								("style", new JsonSchemaBuilder().Type(SchemaValueType.String)),
								("explode", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
							)
							.AllOf(
								new JsonSchemaBuilder().Ref("#/$defs/examples"),
								new JsonSchemaBuilder().Ref("#/$defs/parameter/dependentSchemas/schema/$defs/styles-for-path"),
								new JsonSchemaBuilder().Ref("#/$defs/parameter/dependentSchemas/schema/$defs/styles-for-header"),
								new JsonSchemaBuilder().Ref("#/$defs/parameter/dependentSchemas/schema/$defs/styles-for-query"),
								new JsonSchemaBuilder().Ref("#/$defs/parameter/dependentSchemas/schema/$defs/styles-for-cookie"),
								new JsonSchemaBuilder().Ref("#/$defs/parameter/dependentSchemas/schema/$defs/styles-for-form")
							)
							.Defs(
								("styles-for-path", new JsonSchemaBuilder()
									.If(new JsonSchemaBuilder()
										.Properties(
											("in", new JsonSchemaBuilder().Const("path"))
										)
										.Required("in")
									)
									.Then(new JsonSchemaBuilder()
										.Properties(
											("name", new JsonSchemaBuilder().Pattern("[^/#?]+$")),
											("style", new JsonSchemaBuilder()
												.Default("simple")
												.Enum("matrix", "label", "simple")
											),
											("required", new JsonSchemaBuilder().Const(true))
										)
										.Required("required")
									)
								),
								("styles-for-header", new JsonSchemaBuilder()
									.If(new JsonSchemaBuilder()
										.Properties(
											("in", new JsonSchemaBuilder().Const("header"))
										)
										.Required("in")
									)
									.Then(new JsonSchemaBuilder()
										.Properties(
											("style", new JsonSchemaBuilder()
												.Default("simple")
												.Const("simple")
											)
										)
									)
								),
								("styles-for-query", new JsonSchemaBuilder()
									.If(new JsonSchemaBuilder()
										.Properties(
											("in", new JsonSchemaBuilder().Const("query"))
										)
										.Required("in")
									)
									.Then(new JsonSchemaBuilder()
										.Properties(
											("style", new JsonSchemaBuilder()
												.Default("form")
												.Enum("form", "spaceDelimited", "pipeDelimited", "deepObject")
											),
											("allowReserved", new JsonSchemaBuilder()
												.Default(false)
												.Type(SchemaValueType.Boolean)
											)
										)
									)
								),
								("styles-for-cookie", new JsonSchemaBuilder()
									.If(new JsonSchemaBuilder()
										.Properties(
											("in", new JsonSchemaBuilder().Const("cookie"))
										)
										.Required("in")
									)
									.Then(new JsonSchemaBuilder()
										.Properties(
											("style", new JsonSchemaBuilder()
												.Default("form")
												.Const("form")
											)
										)
									)
								),
								("styles-for-form", new JsonSchemaBuilder()
									.If(new JsonSchemaBuilder()
										.Properties(
											("style", new JsonSchemaBuilder().Const("form"))
										)
										.Required("style")
									)
									.Then(new JsonSchemaBuilder()
										.Properties(
											("explode", new JsonSchemaBuilder().Default(true))
										)
									)
									.Else(new JsonSchemaBuilder()
										.Properties(
											("explode", new JsonSchemaBuilder().Default(false))
										)
									)
								)
							)
						)
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("parameter-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/parameter"))
				),
				("request-body", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#request-body-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("content", new JsonSchemaBuilder().Ref("#/$defs/content")),
						("required", new JsonSchemaBuilder()
							.Default(false)
							.Type(SchemaValueType.Boolean)
						)
					)
					.Required("content")
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("request-body-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/request-body"))
				),
				("content", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#fixed-fields-10")
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/media-type"))
					.PropertyNames(new JsonSchemaBuilder().Format(Formats.MediaRange))
				),
				("media-type", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#media-type-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("schema", new JsonSchemaBuilder().DynamicRef("#meta")),
						("encoding", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/encoding"))
						)
					)
					.AllOf(
						new JsonSchemaBuilder().Ref("#/$defs/specification-extensions"),
						new JsonSchemaBuilder().Ref("#/$defs/examples")
					)
					.UnevaluatedProperties(false)
				),
				("encoding", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#encoding-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("contentType", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Formats.MediaRange)
						),
						("headers", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/header-or-reference"))
						),
						("style", new JsonSchemaBuilder()
							.Default("form")
							.Enum("form", "spaceDelimited", "pipeDelimited", "deepObject")
						),
						("explode", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
						("allowReserved", new JsonSchemaBuilder()
							.Default(false)
							.Type(SchemaValueType.Boolean)
						)
					)
					.AllOf(
						new JsonSchemaBuilder().Ref("#/$defs/specification-extensions"),
						new JsonSchemaBuilder().Ref("#/$defs/encoding/$defs/explode-default")
					)
					.UnevaluatedProperties(false)
					.Defs(
						("explode-default", new JsonSchemaBuilder()
							.If(new JsonSchemaBuilder()
								.Properties(
									("style", new JsonSchemaBuilder().Const("form"))
								)
								.Required("style")
							)
							.Then(new JsonSchemaBuilder()
								.Properties(
									("explode", new JsonSchemaBuilder().Default(true))
								)
							)
							.Else(new JsonSchemaBuilder()
								.Properties(
									("explode", new JsonSchemaBuilder().Default(false))
								)
							)
						)
					)
				),
				("responses", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#responses-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("default", new JsonSchemaBuilder().Ref("#/$defs/response-or-reference"))
					)
					.PatternProperties(
						("^[1-5](?:[0-9]{2}|XX)$", new JsonSchemaBuilder().Ref("#/$defs/response-or-reference"))
					)
					.MinProperties(1)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("response", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#response-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("headers", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/header-or-reference"))
						),
						("content", new JsonSchemaBuilder().Ref("#/$defs/content")),
						("links", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/link-or-reference"))
						)
					)
					.Required("description")
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("response-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/response"))
				),
				("callbacks", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#callback-object")
					.Type(SchemaValueType.Object)
					.Ref("#/$defs/specification-extensions")
					.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/path-item-or-reference"))
				),
				("callbacks-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/callbacks"))
				),
				("example", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#example-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("summary", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("value", true),
						("externalValue", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.Uri)
						)
					)
					.Not(new JsonSchemaBuilder().Required("value", "externalValue"))
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("example-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/example"))
				),
				("link", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#link-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("operationRef", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.UriReference)
						),
						("operationId", true),
						("parameters", new JsonSchemaBuilder().Ref("#/$defs/map-of-strings")),
						("requestBody", true),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("body", new JsonSchemaBuilder().Ref("#/$defs/server"))
					)
					.OneOf(
						new JsonSchemaBuilder().Required("operationRef"),
						new JsonSchemaBuilder().Required("operationId")
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("link-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/link"))
				),
				("header", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#header-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("required", new JsonSchemaBuilder()
							.Default(false)
							.Type(SchemaValueType.Boolean)
						),
						("deprecated", new JsonSchemaBuilder()
							.Default(false)
							.Type(SchemaValueType.Boolean)
						),
						("schema", new JsonSchemaBuilder().DynamicRef("#meta")),
						("content", new JsonSchemaBuilder()
							.Ref("#/$defs/content")
							.MinProperties(1)
							.MaxProperties(1)
						)
					)
					.OneOf(
						new JsonSchemaBuilder().Required("schema"),
						new JsonSchemaBuilder().Required("content")
					)
					.DependentSchemas(
						("schema", new JsonSchemaBuilder()
							.Properties(
								("style", new JsonSchemaBuilder()
									.Default("simple")
									.Const("simple")
								),
								("explode", new JsonSchemaBuilder()
									.Default(false)
									.Type(SchemaValueType.Boolean)
								)
							)
							.Ref("#/$defs/examples")
						)
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("header-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/header"))
				),
				("tag", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#tag-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("name", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("externalDocs", new JsonSchemaBuilder().Ref("#/$defs/external-documentation"))
					)
					.Required("name")
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
				),
				("reference", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#reference-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("$ref", new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.Format(Schema.Formats.UriReference)
						),
						("summary", new JsonSchemaBuilder().Type(SchemaValueType.String)),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
					.UnevaluatedProperties(false)
				),
				("schema", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#schema-object")
					.DynamicAnchor("meta")
					.Type(SchemaValueType.Object | SchemaValueType.Boolean)
				),
				("security-scheme", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#security-scheme-object")
					.Type(SchemaValueType.Object)
					.Properties(
						("type", new JsonSchemaBuilder().Enum("apiKey", "http", "mutualTLS", "oauth2", "openIdConnect")),
						("description", new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
					.Required("type")
					.AllOf(
						new JsonSchemaBuilder().Ref("#/$defs/specification-extensions"),
						new JsonSchemaBuilder().Ref("#/$defs/security-scheme/$defs/type-apikey"),
						new JsonSchemaBuilder().Ref("#/$defs/security-scheme/$defs/type-http"),
						new JsonSchemaBuilder().Ref("#/$defs/security-scheme/$defs/type-http-bearer"),
						new JsonSchemaBuilder().Ref("#/$defs/security-scheme/$defs/type-oauth2"),
						new JsonSchemaBuilder().Ref("#/$defs/security-scheme/$defs/type-oidc")
					)
					.UnevaluatedProperties(false)
					.Defs(
						("type-apikey", new JsonSchemaBuilder()
							.If(new JsonSchemaBuilder()
								.Properties(
									("type", new JsonSchemaBuilder().Const("apiKey"))
								)
								.Required("type")
							)
							.Then(new JsonSchemaBuilder()
								.Properties(
									("name", new JsonSchemaBuilder().Type(SchemaValueType.String)),
									("in", new JsonSchemaBuilder().Enum("query", "header", "cookie"))
								)
								.Required("name", "in")
							)
						),
						("type-http", new JsonSchemaBuilder()
							.If(new JsonSchemaBuilder()
								.Properties(
									("type", new JsonSchemaBuilder().Const("http"))
								)
								.Required("type")
							)
							.Then(new JsonSchemaBuilder()
								.Properties(
									("scheme", new JsonSchemaBuilder().Type(SchemaValueType.String))
								)
								.Required("scheme")
							)
						),
						("type-http-bearer", new JsonSchemaBuilder()
							.If(new JsonSchemaBuilder()
								.Properties(
									("type", new JsonSchemaBuilder().Const("http")),
									("scheme", new JsonSchemaBuilder()
										.Type(SchemaValueType.String)
										.Pattern("^[Bb][Ee][Aa][Rr][Ee][Rr]$")
									)
								)
								.Required("type", "scheme")
							)
							.Then(new JsonSchemaBuilder()
								.Properties(
									("bearerFormat", new JsonSchemaBuilder().Type(SchemaValueType.String))
								)
							)
						),
						("type-oauth2", new JsonSchemaBuilder()
							.If(new JsonSchemaBuilder()
								.Properties(
									("type", new JsonSchemaBuilder().Const("oauth2"))
								)
								.Required("type")
							)
							.Then(new JsonSchemaBuilder()
								.Properties(
									("flows", new JsonSchemaBuilder().Ref("#/$defs/oauth-flows"))
								)
								.Required("flows")
							)
						),
						("type-oidc", new JsonSchemaBuilder()
							.If(new JsonSchemaBuilder()
								.Properties(
									("type", new JsonSchemaBuilder().Const("openIdConnect"))
								)
								.Required("type")
							)
							.Then(new JsonSchemaBuilder()
								.Properties(
									("openIdConnectUrl", new JsonSchemaBuilder()
										.Type(SchemaValueType.String)
										.Format(Schema.Formats.Uri)
									)
								)
								.Required("openIdConnectUrl")
							)
						)
					)
				),
				("security-scheme-or-reference", new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Type(SchemaValueType.Object)
						.Required("$ref")
					)
					.Then(new JsonSchemaBuilder().Ref("#/$defs/reference"))
					.Else(new JsonSchemaBuilder().Ref("#/$defs/security-scheme"))
				),
				("oauth-flows", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.Properties(
						("implicit", new JsonSchemaBuilder().Ref("#/$defs/oauth-flows/$defs/implicit")),
						("password", new JsonSchemaBuilder().Ref("#/$defs/oauth-flows/$defs/password")),
						("clientCredentials", new JsonSchemaBuilder().Ref("#/$defs/oauth-flows/$defs/client-credentials")),
						("authorizationCode", new JsonSchemaBuilder().Ref("#/$defs/oauth-flows/$defs/authorization-code"))
					)
					.Ref("#/$defs/specification-extensions")
					.UnevaluatedProperties(false)
					.Defs(
						("implicit", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.Properties(
								("authorizationUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("refreshUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("scopes", new JsonSchemaBuilder().Ref("#/$defs/map-of-strings"))
							)
							.Required("authorizationUrl", "scopes")
							.Ref("#/$defs/specification-extensions")
							.UnevaluatedProperties(false)
						),
						("password", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.Properties(
								("tokenUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("refreshUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("scopes", new JsonSchemaBuilder().Ref("#/$defs/map-of-strings"))
							)
							.Required("tokenUrl", "scopes")
							.Ref("#/$defs/specification-extensions")
							.UnevaluatedProperties(false)
						),
						("client-credentials", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.Properties(
								("tokenUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("refreshUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("scopes", new JsonSchemaBuilder().Ref("#/$defs/map-of-strings"))
							)
							.Required("tokenUrl", "scopes")
							.Ref("#/$defs/specification-extensions")
							.UnevaluatedProperties(false)
						),
						("authorization-code", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.Properties(
								("authorizationUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("tokenUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("refreshUrl", new JsonSchemaBuilder()
									.Type(SchemaValueType.String)
									.Format(Schema.Formats.Uri)
								),
								("scopes", new JsonSchemaBuilder().Ref("#/$defs/map-of-strings"))
							)
							.Required("authorizationUrl", "tokenUrl", "scopes")
							.Ref("#/$defs/specification-extensions")
							.UnevaluatedProperties(false)
						)
					)
				),
				("security-requirement", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#security-requirement-object")
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
					)
				),
				("specification-extensions", new JsonSchemaBuilder()
					.Comment("https://spec.openapis.org/oas/v3.1.0#specification-extensions")
					.PatternProperties(
						("^x-", true)
					)
				),
				("examples", new JsonSchemaBuilder()
					.Properties(
						("example", true),
						("examples", new JsonSchemaBuilder()
							.Type(SchemaValueType.Object)
							.AdditionalProperties(new JsonSchemaBuilder().Ref("#/$defs/example-or-reference"))
						)
					)
				),
				("map-of-strings", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
			);
}