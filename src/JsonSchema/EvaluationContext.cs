using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Provides contextual information used during schema evaluation, including options, scope, instance data, and
/// evaluation paths.
/// </summary>
/// <remarks>The EvaluationContext struct encapsulates all relevant data required for evaluating a JSON schema
/// against an instance. It includes evaluation options, dynamic scope for reference resolution, pointers to the current
/// evaluation and instance locations, and access to the schema registry.  When building custom keywords that use
/// subschemas, create a new context by copying this one and modifying the <see cref="EvaluationPath"/>, <see cref="Instance"/>,
/// and/or <see cref="InstanceLocation"/> properties as appropriate.</remarks>
public struct EvaluationContext
{
	/// <summary>
	/// Gets the evaluation options.
	/// </summary>
	/// <remarks>
	/// This may be different per run, so it's important this not be captured by constraints.
	/// </remarks>
	public EvaluationOptions Options { get; internal init; }
	
	/// <summary>
	/// Gets the dynamic scope.
	/// </summary>
	/// <remarks>
	/// The dynamic scope is the collection of URIs that evaluation has passed through to get
	/// to the current location.  This is important when processing references.
	/// </remarks>
	public DynamicScope Scope { get; internal init; }

	/// <summary>
	/// Gets the JSON element that represents the instance data root.
	/// </summary>
	public JsonElement InstanceRoot { get; init; }

	/// <summary>
	/// Gets the JSON element that represents the instance data.
	/// </summary>
	public JsonElement Instance { get; init; }

	/// <summary>
	/// Gets or sets a JSON Pointer representing the path through the schemas and across references to the currently evaluated subschema.
	/// </summary>
	public JsonPointer EvaluationPath { get; init; }

	/// <summary>
	/// Gets or sets the JSON Pointer indicating the location of the instance within the JSON document.
	/// </summary>
	public JsonPointer InstanceLocation { get; init; }

	/// <summary>
	/// Gets the schema registry used to manage and retrieve schema resources.
	/// </summary>
	public SchemaRegistry SchemaRegistry { get; internal init; }

	/// <summary>
	/// Gets a value indicating whether reference resolution ignores sibling keywords during processing.
	/// </summary>
	public bool RefIgnoresSiblingKeywords { get; internal init; }

	/// <summary>
	/// Gets the collection of keyword evaluations that have been performed for the current instance.
	/// </summary>
	/// <remarks>This property is set internally and may be null if no keyword evaluations have occurred. The
	/// returned list reflects the latest evaluation results and is updated after each evaluation process.
	/// Keywords are guaranteed to be processed in the correct sequence if the <see cref="DependsOnAnnotationsFromAttribute"/>
	/// attribute is used properly.</remarks>
	public List<KeywordEvaluation>? EvaluatedKeywords { get; internal set; }
}
