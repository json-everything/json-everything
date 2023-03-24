using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$ref`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaSpecVersion(SpecVersion.Draft6)]
[SchemaSpecVersion(SpecVersion.Draft7)]
[SchemaSpecVersion(SpecVersion.Draft201909)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[SchemaSpecVersion(SpecVersion.DraftNext)]
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

		var getSchema = (Uri newUri) =>
		{
			var navigation = (newUri.OriginalString, context.InstanceLocation);
			if (context.NavigatedReferences.Contains(navigation))
				throw new JsonSchemaException($"Encountered circular reference at schema location `{newUri}` and instance location `{context.InstanceLocation}`");

			var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

			JsonSchema? targetSchema = null;
			var targetBase = context.Options.SchemaRegistry.Get(newBaseUri) ??
			                 throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

			if (JsonPointer.TryParse(newUri.Fragment, out var pointerFragment))
			{
				if (targetBase == null)
					throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");

				targetSchema = targetBase.FindSubschema(pointerFragment!, context.Options);
			}
			else
			{
				var anchorFragment = newUri.Fragment.Substring(1);
				if (!AnchorKeyword.AnchorPattern.IsMatch(anchorFragment))
					throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");

				if (targetBase is JsonSchema targetBaseSchema &&
				    targetBaseSchema.Anchors.TryGetValue(anchorFragment, out var anchorDefinition))
					targetSchema = anchorDefinition.Schema;
			}

			if (targetSchema == null)
				throw new JsonSchemaException($"Cannot resolve schema `{newUri}`");
			return (targetSchema, navigation, pointerFragment);
		};
		//Make two attempts to get the schema. Once using the default resolver and once using a resolver designed to find local file references
		var tryGetSchema = () =>
		{
			try { return getSchema(newUri); }
			catch (Exception e)
			{
				//If the Uri is a local path remove any of the internal generated scopes from things like Allof an OneOf blocks if the reference isn't a local one
				var baseUri = context.Scope.FirstOrDefault(x => !x.OriginalString.StartsWith("https://json-everything.net"));
				if (baseUri != null) newUri = new Uri(baseUri, Reference);
				return getSchema(newUri);
			}
		};
		var (targetSchema, navigation, pointerFragment) = tryGetSchema();
		context.NavigatedReferences.Add(navigation);
		context.Push(context.EvaluationPath.Combine(Name), targetSchema);
		if (pointerFragment != null)
			context.LocalResult.SetSchemaReference(pointerFragment);
		context.Evaluate();
		var result = context.LocalResult.IsValid;
		context.Pop();
		context.NavigatedReferences.Remove(navigation);
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