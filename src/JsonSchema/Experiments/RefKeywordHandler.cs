using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Experiments;

public class RefKeywordHandler : IKeywordHandler
{
	public static RefKeywordHandler Instance { get; } = new();

	public string Name => "$ref";
	public string[]? Dependencies { get; }

	private RefKeywordHandler() { }

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		var reference = (keywordValue as JsonValue)?.GetString();
		if (reference is null || !Uri.TryCreate(reference, UriKind.RelativeOrAbsolute, out _))
			throw new SchemaValidationException("$ref must contain a valid URI reference", context);

		var newUri = new Uri(context.BaseUri, reference);
		var fragment = newUri.Fragment;

		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		JsonNode? target;
		if (JsonPointer.TryParse(fragment, out var pointer))
		{
			var targetBase = context.Options.SchemaRegistry.Get(newBaseUri);
			if (!pointer.TryEvaluate(targetBase, out target))
				throw new RefResolutionException(newBaseUri, pointer);
		}
		else
		{
			var allowLegacy = context.EvaluatingAs == MetaSchemas.Draft6Id || context.EvaluatingAs == MetaSchemas.Draft7Id;
			var anchor = fragment[1..];
			if (!SchemaRegistry.AnchorPattern202012.IsMatch(anchor))
				throw new SchemaValidationException($"Unrecognized fragment type `{newUri}`", context);

			target = context.Options.SchemaRegistry.Get(newBaseUri, anchor, allowLegacy);
		}

		var localContext = context;
		if (newBaseUri.OriginalString != context.BaseUri.OriginalString)
			localContext.RefUri = newBaseUri;
		localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name);
		localContext.SchemaLocation = pointer ?? JsonPointer.Empty;

		var result = localContext.Evaluate(target);

		return new KeywordEvaluation
		{
			Valid = result.Valid,
			Children = [result]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}