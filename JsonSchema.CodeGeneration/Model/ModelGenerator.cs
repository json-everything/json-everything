using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.Schema.CodeGeneration.Model;

public static class ModelGenerator
{
	private const string _baseId = "schema:json-everything:codegen:base";
	private const string _derivedId = "schema:json-everything:codegen:derived";
	private const string _stringId = "schema:json-everything:codegen:string";
	private const string _integerId = "schema:json-everything:codegen:integer";
	private const string _numberId = "schema:json-everything:codegen:number";
	private const string _booleanId = "schema:json-everything:codegen:boolean";
	private const string _enumId = "schema:json-everything:codegen:enum";
	private const string _arrayId = "schema:json-everything:codegen:array";
	private const string _objectId = "schema:json-everything:codegen:object";
	private const string _dictionaryId = "schema:json-everything:codegen:dictionary";

	private static readonly JsonSchema _baseRequirements =
		new JsonSchemaBuilder()
			.Id(_baseId)
			.Type(SchemaValueType.Object)
			.Properties(
				(TitleKeyword.Name, true) // TODO: this needs to have a pattern
			);

	private static readonly JsonSchema _derivedRequirements =
		new JsonSchemaBuilder()
			.Id(_derivedId)
			.OneOf(
				new JsonSchemaBuilder().Ref(_stringId),
				new JsonSchemaBuilder().Ref(_integerId),
				new JsonSchemaBuilder().Ref(_numberId),
				new JsonSchemaBuilder().Ref(_booleanId),
				new JsonSchemaBuilder().Ref(_enumId),
				new JsonSchemaBuilder().Ref(_arrayId),
				new JsonSchemaBuilder().Ref(_objectId),
				new JsonSchemaBuilder().Ref(_dictionaryId)
			);

	private static readonly JsonSchema _stringRequirements =
		new JsonSchemaBuilder()
			.Id(_stringId)
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("string"))
			)
			.Required(TypeKeyword.Name);

	private static readonly JsonSchema _integerRequirements =
		new JsonSchemaBuilder()
			.Id(_integerId)
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("integer"))
			)
			.Required(TypeKeyword.Name);

	private static readonly JsonSchema _numberRequirements =
		new JsonSchemaBuilder()
			.Id(_numberId)
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("number"))
			)
			.Required(TypeKeyword.Name);

	private static readonly JsonSchema _booleanRequirements =
		new JsonSchemaBuilder()
			.Id(_booleanId)
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("boolean"))
			)
			.Required(TypeKeyword.Name);

	private static readonly JsonSchema _enumMetaSchema =
		new JsonSchemaBuilder()
			.Id(_enumId)
			.Ref(_baseId)
			.Properties(
				(EnumKeyword.Name, true) // TODO: these values need to have a pattern
			)
			.Required(TitleKeyword.Name, EnumKeyword.Name)
			.UnevaluatedProperties(false);

	private static readonly JsonSchema _arrayMetaSchema =
		new JsonSchemaBuilder()
			.Id(_arrayId)
			.Ref(_baseId)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("array")),
				(ItemsKeyword.Name, new JsonSchemaBuilder().Ref(_derivedId))
			)
			.Required(TypeKeyword.Name, ItemsKeyword.Name);

	private static readonly JsonSchema _objectMetaSchema =
		new JsonSchemaBuilder()
			.Id(_objectId)
			.Ref(_baseId)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("object")),
				(PropertiesKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder()
						.Ref(_derivedId))
				),
				(AdditionalPropertiesKeyword.Name, false)
			)
			.Required(TitleKeyword.Name, TypeKeyword.Name, PropertiesKeyword.Name);

	private static readonly JsonSchema _dictionaryMetaSchema =
		new JsonSchemaBuilder()
			.Id(_dictionaryId)
			.Ref(_baseId)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("object")),
				(PropertiesKeyword.Name, false),
				(PropertyNamesKeyword.Name, new JsonSchemaBuilder().Ref(_derivedId)), // TODO: should this be specific to naming things?
				(AdditionalPropertiesKeyword.Name, new JsonSchemaBuilder().Ref(_derivedId))
			)
			.Required(TypeKeyword.Name, AdditionalPropertiesKeyword.Name);
	
	private static readonly EvaluationOptions _options;

	static ModelGenerator()
	{
		_options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			PreserveDroppedAnnotations = true
		};
		_options.SchemaRegistry.Register(_baseRequirements);
		_options.SchemaRegistry.Register(_derivedRequirements);
		_options.SchemaRegistry.Register(_stringRequirements);
		_options.SchemaRegistry.Register(_integerRequirements);
		_options.SchemaRegistry.Register(_numberRequirements);
		_options.SchemaRegistry.Register(_booleanRequirements);
		_options.SchemaRegistry.Register(_enumMetaSchema);
		_options.SchemaRegistry.Register(_arrayMetaSchema);
		_options.SchemaRegistry.Register(_objectMetaSchema);
		_options.SchemaRegistry.Register(_dictionaryMetaSchema);
	}

	public static TypeModel GenerateCodeModel(this JsonSchema schema)
	{
		var json = JsonSerializer.SerializeToNode(schema);

		var name = schema.GetTitle();

		var derivedResults = _derivedRequirements.Evaluate(json, _options);
		if (!derivedResults.IsValid)
		{
#if DEBUG
			Console.WriteLine(JsonSerializer.Serialize(derivedResults, new JsonSerializerOptions{Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping}));
#endif

			derivedResults.TryGetAnnotation(OneOfKeyword.Name, out var validCountNode);

			var validCount = validCountNode?.GetValue<int>();
			if (validCount is null or 0)
				throw new SchemaConversionException("This schema is not in a supported form.");

			throw new SchemaConversionException("This schema matches multiple supported forms.");
		}

		var validSubschemaId = derivedResults.Details.Single(x => x.IsValid).Details[0].SchemaLocation;
		switch (validSubschemaId.OriginalString)
		{
			case _stringId:
				return CommonModels.String;
			case _integerId:
				return CommonModels.Integer;
			case _numberId:
				return CommonModels.Number;
			case _booleanId:
				return CommonModels.Boolean;
			case _enumId:
				var values = schema.GetEnum()!;
				return new EnumModel(name!, values.Select(x => x!.GetValue<string>()));
			case _arrayId:
				var itemsSchema = schema.GetItems()!;
				var items = GenerateCodeModel(itemsSchema);
				return new ArrayModel(items) { Name = name };
			case _objectId:
				var propertiesList = schema.GetProperties()!;
				var properties = propertiesList.Select(kvp => new PropertyModel(kvp.Key, GenerateCodeModel(kvp.Value), true, true));
				return new ObjectModel(name!, properties) { Name = name };
			case _dictionaryId:
				var additionalPropertiesSchema = schema.GetAdditionalProperties()!;
				var additionalProperties = GenerateCodeModel(additionalPropertiesSchema);
				return new DictionaryModel(additionalProperties) { Name = name };
		}

		throw new SchemaConversionException("This schema is not in a supported form.");
	}
}