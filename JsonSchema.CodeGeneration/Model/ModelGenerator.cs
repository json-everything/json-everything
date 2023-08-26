using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Json.More;
using Json.Pointer;

namespace Json.Schema.CodeGeneration.Model;

internal static class ModelGenerator
{
	private const string _baseId = "schema:json-everything:codegen:base";
	private const string _abstractId = "schema:json-everything:codegen:abstract";
	private const string _refId = "schema:json-everything:codegen:ref";
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
			.Defs(
				("convertible-string", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					// support everything that will be transformed away
					.Pattern(@"^[a-zA-Z_-][a-zA-Z0-9 _-]*$")
				)
			)
			.Properties(
				(TitleKeyword.Name, new JsonSchemaBuilder().Ref("#/$defs/convertible-string")),
				(DefsKeyword.Name, true)
			);

	private static readonly JsonSchema _abstractRequirements =
		new JsonSchemaBuilder()
			.Id(_abstractId)
			.OneOf(
				new JsonSchemaBuilder().Ref(_refId),
				new JsonSchemaBuilder().Ref(_stringId),
				new JsonSchemaBuilder().Ref(_integerId),
				new JsonSchemaBuilder().Ref(_numberId),
				new JsonSchemaBuilder().Ref(_booleanId),
				new JsonSchemaBuilder().Ref(_enumId),
				new JsonSchemaBuilder().Ref(_arrayId),
				new JsonSchemaBuilder().Ref(_objectId),
				new JsonSchemaBuilder().Ref(_dictionaryId)
			);

	private static readonly JsonSchema _refSchemaRequirements =
		new JsonSchemaBuilder()
			.Id(_refId)
			.Type(SchemaValueType.Object)
			.Properties(
				(RefKeyword.Name, true)
			)
			.Required(RefKeyword.Name)
			.AdditionalProperties(false);

	private static readonly JsonSchema _stringRequirements =
		new JsonSchemaBuilder()
			.Id(_stringId)
			.Title("string")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("string"))
			)
			.Required(TypeKeyword.Name);

	private static readonly JsonSchema _integerRequirements =
		new JsonSchemaBuilder()
			.Id(_integerId)
			.Title("integer")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("integer"))
			)
			.Required(TypeKeyword.Name);

	private static readonly JsonSchema _numberRequirements =
		new JsonSchemaBuilder()
			.Id(_numberId)
			.Title("number")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("number"))
			)
			.Required(TypeKeyword.Name);

	private static readonly JsonSchema _booleanRequirements =
		new JsonSchemaBuilder()
			.Id(_booleanId)
			.Title("boolean")
			.Type(SchemaValueType.Object)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("boolean"))
			)
			.Required(TypeKeyword.Name);

	private static readonly JsonSchema _enumMetaSchema =
		new JsonSchemaBuilder()
			.Id(_enumId)
			.Title("enumeration")
			.Ref(_baseId)
			.Properties(
				(EnumKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Ref(_baseId + "#/$defs/convertible-string"))
				)
			)
			.Required(TitleKeyword.Name, EnumKeyword.Name)
			.UnevaluatedProperties(false);

	private static readonly JsonSchema _arrayMetaSchema =
		new JsonSchemaBuilder()
			.Id(_arrayId)
			.Title("array")
			.Ref(_baseId)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("array")),
				(ItemsKeyword.Name, new JsonSchemaBuilder().Ref(_abstractId))
			)
			.Required(TypeKeyword.Name, ItemsKeyword.Name);

	private static readonly JsonSchema _objectMetaSchema =
		new JsonSchemaBuilder()
			.Id(_objectId)
			.Title("custom-object")
			.Ref(_baseId)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("object")),
				(PropertiesKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.PropertyNames(new JsonSchemaBuilder().Ref(_baseId + "#/$defs/convertible-string"))
					.AdditionalProperties(new JsonSchemaBuilder()
						.Not(new JsonSchemaBuilder()
							.Properties(
								(ReadOnlyKeyword.Name, new JsonSchemaBuilder().Const(true)),
								(WriteOnlyKeyword.Name, new JsonSchemaBuilder().Const(true))
							)
							.Required(ReadOnlyKeyword.Name, WriteOnlyKeyword.Name)
						)
						.Ref(_abstractId))
				),
				(AdditionalPropertiesKeyword.Name, false)
			)
			.Required(TitleKeyword.Name, TypeKeyword.Name, PropertiesKeyword.Name);

	private static readonly JsonSchema _dictionaryMetaSchema =
		new JsonSchemaBuilder()
			.Id(_dictionaryId)
			.Title("dictionary")
			.Ref(_baseId)
			.Properties(
				(TypeKeyword.Name, new JsonSchemaBuilder().Const("object")),
				(PropertiesKeyword.Name, false),
				(PropertyNamesKeyword.Name, new JsonSchemaBuilder()
					.Properties((TypeKeyword.Name, new JsonSchemaBuilder().Const("string")))
					.Required(TypeKeyword.Name)
				),
				(AdditionalPropertiesKeyword.Name, new JsonSchemaBuilder().Ref(_abstractId))
			)
			.Required(TypeKeyword.Name, AdditionalPropertiesKeyword.Name);
	
	private static readonly EvaluationOptions _options;

	static ModelGenerator()
	{
		_options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List,
			PreserveDroppedAnnotations = true,
			EvaluateAs = SpecVersion.Draft202012
		};
		_options.SchemaRegistry.Register(_baseRequirements);
		_options.SchemaRegistry.Register(_abstractRequirements);
		_options.SchemaRegistry.Register(_refSchemaRequirements);
		_options.SchemaRegistry.Register(_stringRequirements);
		_options.SchemaRegistry.Register(_integerRequirements);
		_options.SchemaRegistry.Register(_numberRequirements);
		_options.SchemaRegistry.Register(_booleanRequirements);
		_options.SchemaRegistry.Register(_enumMetaSchema);
		_options.SchemaRegistry.Register(_arrayMetaSchema);
		_options.SchemaRegistry.Register(_objectMetaSchema);
		_options.SchemaRegistry.Register(_dictionaryMetaSchema);
	}

	public static TypeModel GenerateCodeModel(this JsonSchema schema, EvaluationOptions options, GenerationCache cache)
	{
		var generated = cache.FirstOrDefault(x => x.Schema == schema);
		if (generated != null) return generated.Model;

		generated = new GenerationCacheItem(schema);
		cache.Add(generated);

		var json = JsonSerializer.SerializeToNode(schema);

		var abstractResults = _abstractRequirements.Evaluate(json, _options);
#if DEBUG
		// this appears in local test runs and is quite useful
		//Console.WriteLine(JsonSerializer.Serialize(abstractResults, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
#endif
		if (!abstractResults.IsValid)
		{
			abstractResults.TryGetAnnotation(OneOfKeyword.Name, out var validCountNode);

			var validCount = validCountNode?.GetValue<int>();
			if (validCount is null or 0)
				throw new UnsupportedSchemaException("This schema is not in a supported form.");

			var validSubschemas = abstractResults.GetAllAnnotations(TitleKeyword.Name).ToJsonArray();
			throw new UnsupportedSchemaException($"This schema matches multiple supported forms: {validSubschemas.AsJsonString()}.");
		}

		TypeModel? typeModel = null;
		var name = schema.GetTitle();
		var validSubschemaResult = abstractResults.Details.Single(x => x.IsValid &&
		                                                               x.InstanceLocation == JsonPointer.Empty &&
		                                                               x.EvaluationPath.Segments.Length == 3); // e.g. /oneof/7/$ref
		var validSubschemaId = validSubschemaResult.SchemaLocation;
		switch (validSubschemaId.OriginalString)
		{
			case _refId:
				var refKeyword = (RefKeyword) schema[RefKeyword.Name]!;
				var targetSchema = ResolveRef(schema.BaseUri, refKeyword.Reference, options);
				typeModel = GenerateCodeModel(targetSchema, options, cache);
				break;
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
				typeModel = new EnumModel(name!, values.Select(x => x!.GetValue<string>()));
				break;
			case _arrayId:
				var itemsSchema = schema.GetItems()!;
				var items = GenerateCodeModel(itemsSchema, options, cache);
				typeModel = new ArrayModel(name, items);
				break;
			case _objectId:
				var propertiesList = schema.GetProperties()!;
				var properties = propertiesList.Select(kvp =>
				{
					kvp.Value.TryGetKeyword<WriteOnlyKeyword>(out var writeOnlyKeyword);
					kvp.Value.TryGetKeyword<ReadOnlyKeyword>(out var readOnlyKeyword);
					var canRead = !(writeOnlyKeyword?.Value ?? false);
					var canWrite = !(readOnlyKeyword?.Value ?? false);
					return new PropertyModel(kvp.Key, GenerateCodeModel(kvp.Value, options, cache), canRead, canWrite);
				});
				typeModel = new ObjectModel(name!, properties);
				break;
			case _dictionaryId:
				var additionalPropertiesSchema = schema.GetAdditionalProperties()!;
				var additionalProperties = GenerateCodeModel(additionalPropertiesSchema, options, cache);
				typeModel = new DictionaryModel(name, additionalProperties);
				break;
		}

		if (typeModel == null)
			throw new UnsupportedSchemaException("This basically shouldn't happen because of the earlier validation.");

		generated.Model = typeModel;
		return typeModel;
	}

	private  static readonly Regex _anchorPattern = new("^[A-Za-z][-A-Za-z0-9.:_]*$");

	private static JsonSchema ResolveRef(Uri baseUri, Uri reference, EvaluationOptions options)
	{
		var newUri = new Uri(baseUri, reference);
		var fragment = newUri.Fragment;

		//var instanceLocation = schemaConstraint.BaseInstanceLocation.Combine(schemaConstraint.RelativeInstanceLocation);
		//var navigation = (newUri.OriginalString, InstanceLocation: instanceLocation);
		//if (context.NavigatedReferences.Contains(navigation))
		//	throw new JsonSchemaException($"Encountered circular reference at schema location `{newUri}` and instance location `{schemaConstraint.RelativeInstanceLocation}`");

		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		JsonSchema? targetSchema = null;
		var targetBase = options.SchemaRegistry.Get(newBaseUri) ??
		                 throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

		if (JsonPointer.TryParse(fragment, out var pointerFragment))
		{
			if (targetBase == null)
				throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

			targetSchema = targetBase.FindSubschema(pointerFragment!, options);
		}
		else
		{
			var anchorFragment = fragment.Substring(1);
			if (!_anchorPattern.IsMatch(anchorFragment))
				throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");

			if (targetBase is JsonSchema targetBaseSchema)
				targetSchema = targetBaseSchema.GetAnchor(anchorFragment);
		}

		if (targetSchema == null)
			throw new JsonSchemaException($"Cannot resolve schema `{newUri}`");

		return targetSchema;
	}
}

internal class GenerationCacheItem
{
	public Guid Id { get; }
	public JsonSchema Schema { get; }
	public TypeModel Model { get; set; }

	public GenerationCacheItem(JsonSchema schema)
	{
		Schema = schema;
		Id = Guid.NewGuid();
		Model = new PlaceholderModel(Id);
	}
}

internal class GenerationCache : List<GenerationCacheItem>
{
	public void FillPlaceholders()
	{
		foreach (var item in this)
		{
			item.Model.FillPlaceholders(this);
		}
	}
}