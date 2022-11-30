using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$ref`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft6)]
[SchemaDraft(Draft.Draft7)]
[SchemaDraft(Draft.Draft201909)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(RefKeywordJsonConverter))]
public class RefKeyword : IJsonSchemaKeyword, IEquatable<RefKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$ref";

	/// <summary>
	/// The URI reference.
	/// </summary>
	public Uri Reference { get; }

	/// <summary>
	/// Creates a new <see cref="RefKeyword"/>.
	/// </summary>
	/// <param name="value">The URI reference.</param>
	public RefKeyword(Uri value)
	{
		Reference = value;
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);

		var newUri = new Uri(context.Scope.LocalScope, Reference);
		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		JsonSchema? targetSchema = null;
		if (JsonPointer.TryParse(newUri.Fragment, out var pointerFragment, JsonPointerKind.UriEncoded))
		{
			var targetBase = context.Options.SchemaRegistry.Get(newBaseUri);
			if (targetBase == null)
				throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");
			targetSchema = targetBase.FindSubschema(pointerFragment!, context.Options);
		}
		else
		{
			var anchorFragment = newUri.Fragment.Substring(1);
			if (!AnchorKeyword.AnchorPattern.IsMatch(anchorFragment))
				throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");
			if (context.Options.SchemaRegistry.Get(newBaseUri)?.Anchors.TryGetValue(anchorFragment, out var anchorDefinition) ?? false)
				targetSchema = anchorDefinition.Schema;
		}

		if (targetSchema == null)
			throw new JsonSchemaException($"Cannot resolve schema `{newUri}`");

		context.Push(context.EvaluationPath.Combine(Name), targetSchema);
		context.Evaluate();
		var result = context.LocalResult.IsValid;
		context.Pop();
		if (!result)
			context.LocalResult.Fail();

		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(RefKeyword? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Reference, other.Reference);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		return Equals(obj as RefKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Reference.GetHashCode();
	}
}

internal class RefKeywordJsonConverter : JsonConverter<RefKeyword>
{
	public override RefKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var uri = reader.GetString();
		return new RefKeyword(new Uri(uri!, UriKind.RelativeOrAbsolute));


	}
	public override void Write(Utf8JsonWriter writer, RefKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(RefKeyword.Name);
		JsonSerializer.Serialize(writer, value.Reference, options);
	}
}

public static partial class ErrorMessages
{
	private static string? _recursiveRef;

	/// <summary>
	/// Gets or sets the error message for when a recursive reference is encountered.
	/// </summary>
	/// <remarks>No tokens are supported.</remarks>
	public static string RecursiveRef
	{
		get => _recursiveRef ?? Get();
		set => _recursiveRef = value;
	}

	private static string? _baseUriResolution;

	/// <summary>
	/// Gets or sets the error message for when a base URI cannot be resolved.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[uri]] - the base URI to resolve
	/// </remarks>
	public static string BaseUriResolution
	{
		get => _baseUriResolution ?? Get();
		set => _baseUriResolution = value;
	}

	private static string? _pointerParse;

	/// <summary>
	/// Gets or sets the error message for when a URI fragment cannot be parsed into a JSON Pointer.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[fragment]] - the pointer fragment
	/// </remarks>
	public static string PointerParse
	{
		get => _pointerParse ?? Get();
		set => _pointerParse = value;
	}

	private static string? _refResolution;

	/// <summary>
	/// Gets or sets the error message for when a reference fails to resolve.
	/// </summary>
	/// <remarks>
	///	Available tokens are:
	///   - [[uri]] - the reference to resolve
	/// </remarks>
	public static string RefResolution
	{
		get => _refResolution ?? Get();
		set => _refResolution = value;
	}
}