using System;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$ref`.
/// </summary>
/// <remarks>
/// This keyword is used to reference another schema.
/// </remarks>
public class RefKeyword : IKeywordHandler
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="RefKeyword"/>.
	/// </summary>
	public static RefKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public virtual string Name => "$ref";

	/// <summary>
	/// Initializes a new instance of the <see cref="RefKeyword"/> class.
	/// </summary>
	protected RefKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		if (!Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' value must be a valid URI");

		return uri;
	}

	/// <summary>
	/// Builds and registers subschemas based on the specified keyword data within the provided build context.
	/// </summary>
	/// <param name="keyword">The keyword data used to determine which subschemas to build. Cannot be null.</param>
	/// <param name="context">The context in which subschemas are constructed and registered. Cannot be null.</param>
	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var reference = (Uri)keyword.Value!;
		var newUri = new Uri(context.BaseUri, reference);

		keyword.Value = newUri;
	}

	internal virtual void TryResolve(KeywordData keyword, BuildContext context)
	{
		var newUri = (Uri?)keyword.Value;
		if (newUri is null) return;

		var fragment = newUri.Fragment;

		JsonSchemaNode? targetSchema;
		if (JsonPointer.TryParse(fragment, out var pointerFragment))
		{
			var targetBase = context.Options.SchemaRegistry.Get(newUri);

			targetSchema = targetBase?.FindSubschema(pointerFragment, context);
		}
		else
		{
			var anchorFragment = fragment[1..]; // drop #
			targetSchema = context.Options.SchemaRegistry.Get(newUri, anchorFragment);
		}

		if (targetSchema is not null)
			keyword.Subschemas = [targetSchema];
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var newUri = (Uri)keyword.Value!;
		var subschema = keyword.Subschemas.FirstOrDefault();
		if (subschema is null)
			throw new RefResolutionException(newUri);

		var refContext = context with
		{
			EvaluationPath = context.EvaluationPath.Combine(Name)
		};

		var subschemaEvaluation = subschema.Evaluate(refContext);
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = subschemaEvaluation.IsValid,
			Details = [subschemaEvaluation]
		};
	}
}
