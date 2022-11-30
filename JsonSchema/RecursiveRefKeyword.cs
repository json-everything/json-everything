using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$recursiveRef`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft201909)]
[Vocabulary(Vocabularies.Core201909Id)]
[JsonConverter(typeof(RecursiveRefKeywordJsonConverter))]
public class RecursiveRefKeyword : IJsonSchemaKeyword, IEquatable<RecursiveRefKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$recursiveRef";

	/// <summary>
	/// The URI reference.
	/// </summary>
	public Uri Reference { get; }

	/// <summary>
	/// Creates a new <see cref="RecursiveRefKeyword"/>.
	/// </summary>
	/// <param name="value">The URI.</param>
	public RecursiveRefKeyword(Uri value)
	{
		Reference = value ?? throw new ArgumentNullException(nameof(value));
	}

	/// <summary>
	/// Performs evaluation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the evaluation process.</param>
	public void Evaluate(EvaluationContext context)
	{
		context.EnterKeyword(Name);

		JsonSchema? targetSchema = null;
		foreach (var uri in context.Scope.Reverse())
		{
			var scopeRoot = context.Options.SchemaRegistry.Get(uri);
			if (scopeRoot == null)
				throw new Exception("This shouldn't happen");
			if (scopeRoot.RecursiveAnchor != null)
			{
				var localResource = context.LocalSchema.IsResourceRoot
					? context.LocalSchema
					: context.Options.SchemaRegistry.Get(context.LocalSchema.BaseUri);
				if (localResource!.RecursiveAnchor == null) break;

				targetSchema = scopeRoot.RecursiveAnchor;
				break;
			}
		}

		if (targetSchema == null)
		{
			var newUri = new Uri(context.Scope.LocalScope, Reference);
			var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

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
		}

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
	public bool Equals(RecursiveRefKeyword? other)
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
		return Equals(obj as RecursiveRefKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Reference.GetHashCode();
	}
}

internal class RecursiveRefKeywordJsonConverter : JsonConverter<RecursiveRefKeyword>
{
	public override RecursiveRefKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var uri = reader.GetString()!;
		return new RecursiveRefKeyword(new Uri(uri, UriKind.RelativeOrAbsolute));


	}
	public override void Write(Utf8JsonWriter writer, RecursiveRefKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(RecursiveRefKeyword.Name);
		JsonSerializer.Serialize(writer, value.Reference, options);
	}
}