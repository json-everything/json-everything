using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Experiments;

public struct EvaluationContext
{
	public Uri BaseUri { get; private set; }
	public JsonPointer SchemaLocation { get; set; }
	public JsonPointer InstanceLocation { get; set; }
	public JsonPointer EvaluationPath { get; set; }
	public JsonNode? LocalInstance { get; set; }
	public EvaluationOptions Options { get; internal init; }
	public DynamicScope DynamicScope { get; }

	internal Uri? RefUri { get; set; }
	internal Uri EvaluatingAs { get; set; }
	internal HashSet<(JsonNode Schema, JsonNode? Instance)> NavigationRefs { get; } = [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public EvaluationContext()
	{
		DynamicScope = new();
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public EvaluationResults Evaluate(JsonNode? localSchema)
	{
		if (localSchema is JsonValue value)
		{
			var boolSchema = value.GetBool();
			if (!boolSchema.HasValue)
				throw new SchemaValidationException("Schema must be an object or a boolean", this);

			return new EvaluationResults
			{
				Valid = boolSchema.Value,
				InstanceLocation = InstanceLocation,
				EvaluationPath = EvaluationPath
			};
		}

		if (localSchema is not JsonObject objSchema)
			throw new SchemaValidationException("Schema must be an object or a boolean", this);

		var currentBaseUri = BaseUri;

		var lookup = Options.SchemaRegistry.GetUri(objSchema);
		if (lookup is not null)
			BaseUri = lookup;
		else if (RefUri is not null)
			BaseUri = RefUri;

		if (currentBaseUri != BaseUri) 
			DynamicScope.Push(BaseUri);

		var navigation = (localSchema, LocalInstance);
		if (!NavigationRefs.Add(navigation))
			throw new InvalidOperationException($"Encountered circular reference at schema location `{BaseUri}#{SchemaLocation}` and instance location `{InstanceLocation}`");

		EvaluatingAs ??= Options.DefaultMetaSchema;
		var resourceRoot = Options.SchemaRegistry.Get(BaseUri);
		if (resourceRoot.TryGetValue("$schema", out var schemaNode, out _))
		{
			var metaSchemaId = (schemaNode as JsonValue)?.GetString();
			if (metaSchemaId is null || !Uri.TryCreate(metaSchemaId, UriKind.Absolute, out var metaSchemaUri))
				throw new SchemaValidationException("$schema must be a valid URI", this);

			EvaluatingAs = metaSchemaUri;
		}

		var metaSchema = Options.SchemaRegistry.Get(EvaluatingAs);
		var vocabHandlers = Vocabularies.GetHandlersByMetaschema(metaSchema, this);

		IEnumerable<(KeyValuePair<string, JsonNode?> Keyword, IKeywordHandler? Handler)> withHandlers;
		if (objSchema.ContainsKey("$ref") &&
		    (EvaluatingAs == MetaSchemas.Draft6Id || EvaluatingAs == MetaSchemas.Draft7Id))
		{
			if (currentBaseUri is not null && objSchema.ContainsKey("$id"))
				BaseUri = currentBaseUri;
			withHandlers = [(objSchema.Single(x => x.Key == "$ref"), RefKeywordHandler.Instance)];
		}
		else
			withHandlers = KeywordRegistry.GetHandlers(objSchema, vocabHandlers); // also orders the handlers by priority

		var valid = true;
		var evaluations = new List<KeywordEvaluation>();
		var annotations = new Dictionary<string, JsonNode?>();
		foreach (var (keyword, handler) in withHandlers)
		{
			var keywordContext = this;
			keywordContext.RefUri = null;
			var keywordResult = handler?.Handle(keyword.Value, keywordContext, evaluations) ??
			                    new KeywordEvaluation
			                    {
									Valid = true,
									Annotation = keyword.Value,
									HasAnnotation = true
			                    };
			valid &= keywordResult.Valid;
			if (Equals(keywordResult, KeywordEvaluation.Annotate))
				keywordResult = new KeywordEvaluation
				{
					Valid = true,
					Annotation = keyword.Value,
					HasAnnotation = true
				};
			if (!Equals(keywordResult, KeywordEvaluation.Skip))
				keywordResult.Key = keyword.Key;
			evaluations.Add(keywordResult);
			if (keywordResult.HasAnnotation)
				annotations[keyword.Key] = keywordResult.Annotation;
		}

		if (currentBaseUri != BaseUri)
			DynamicScope.Pop();

		NavigationRefs.Remove(navigation);

		return new EvaluationResults
		{
			Valid = valid,
			SchemaLocation = SchemaLocation.Segments.Length != 0
				? new Uri(BaseUri, SchemaLocation.ToString())
				: BaseUri,
			InstanceLocation = InstanceLocation,
			EvaluationPath = EvaluationPath,
			Details = evaluations.Count != 0 ? evaluations.SelectMany(x => x.Children).ToArray() : null,
			Annotations = valid && annotations.Count != 0 ? annotations : null
		};
	}
}