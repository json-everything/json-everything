using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$dynamicRef`.
/// </summary>
[SchemaKeyword(Name)]
[SchemaDraft(Draft.Draft202012)]
[SchemaDraft(Draft.DraftNext)]
[Vocabulary(Vocabularies.Core202012Id)]
[Vocabulary(Vocabularies.CoreNextId)]
[JsonConverter(typeof(DynamicRefKeywordJsonConverter))]
public class DynamicRefKeyword : IJsonSchemaKeyword, IEquatable<DynamicRefKeyword>
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public const string Name = "$dynamicRef";

	/// <summary>
	/// The URI reference.
	/// </summary>
	public Uri Reference { get; }

	/// <summary>
	/// Creates a new <see cref="DynamicRefKeyword"/>.
	/// </summary>
	/// <param name="value"></param>
	public DynamicRefKeyword(Uri value)
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
		var parts = Reference.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
		var baseUri = parts[0];
		var fragment = parts.Length > 1 ? parts[1] : null;


		JsonSchema? GetSchema(Uri? uri, string? anchor)
		{
			var uriScope = uri ?? context.CurrentUri;
			var currentScopeDefinesDynamicAnchor =
				!string.IsNullOrEmpty(fragment) &&
				context.Options.SchemaRegistry.DynamicScopeDefinesAnchor(uriScope, fragment!);

			return currentScopeDefinesDynamicAnchor
				? context.Options.SchemaRegistry.GetDynamic(uri, anchor)
				: context.Options.SchemaRegistry.Get(uri, anchor);
		}

		Uri? newUri;
		JsonSchema? baseSchema = null;
		if (!string.IsNullOrEmpty(baseUri))
		{
			if (Uri.TryCreate(baseUri, UriKind.Absolute, out newUri))
				baseSchema = GetSchema(newUri, fragment);
			else
			{
				var uriFolder = context.CurrentUri.OriginalString.EndsWith("/")
					? context.CurrentUri
					: context.CurrentUri.GetParentUri();
				newUri = new Uri(uriFolder, baseUri);
				baseSchema = GetSchema(newUri, fragment);
			}
		}
		else
		{
			newUri = context.CurrentUri;
			baseSchema ??= GetSchema(newUri, fragment) ?? context.SchemaRoot;
			newUri = baseSchema.BaseUri;
		}

		var absoluteReference = SchemaRegistry.GetFullReference(newUri, fragment);
		var navigation = (absoluteReference, context.InstanceLocation);
		if (context.NavigatedReferences.Contains(navigation))
		{
			context.LocalResult.Fail(Name, ErrorMessages.BaseUriResolution, ("uri", newUri!.OriginalString));
			context.ExitKeyword(Name, false);
			return;
		}

		JsonSchema? schema;
		if (!string.IsNullOrEmpty(fragment) && AnchorKeyword.AnchorPattern.IsMatch(fragment!))
			schema = GetSchema(newUri, fragment);
		else
		{
			if (baseSchema == null)
			{
				context.LocalResult.Fail(Name, ErrorMessages.PointerParse, ("fragment", fragment!));
				context.ExitKeyword(Name, false);
				return;
			}

			if (!string.IsNullOrEmpty(fragment))
			{
				fragment = $"#{fragment}";
				if (!JsonPointer.TryParse(fragment, out var pointer))
				{
					context.LocalResult.Fail(Name, ErrorMessages.PointerParse, ("fragment", fragment));
					context.ExitKeyword(Name, false);
					return;
				}

				(schema, newUri) = baseSchema.FindSubschema(pointer!, newUri);
			}
			else
				schema = baseSchema;
		}

		if (schema == null)
		{
			context.LocalResult.Fail(Name, ErrorMessages.RefResolution, ("uri", Reference));
			context.ExitKeyword(Name, false);
			return;
		}

		context.NavigatedReferences.Add(navigation);
		context.Push(context.EvaluationPath.Combine(Name), schema, schema.BaseUri ?? newUri);
		context.Evaluate();
		var result = context.LocalResult.IsValid;
		context.Pop();
		if (!result)
			context.LocalResult.Fail();
		context.ExitKeyword(Name, context.LocalResult.IsValid);
	}

	public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, DynamicScope scope, JsonPointer instanceLocation, EvaluationOptions options)
	{
		var newUri = new Uri(scope.LocalScope, Reference);
		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		JsonSchema? targetSchema = null;
		if (JsonPointer.TryParse(newUri.Fragment, out var pointerFragment, JsonPointerKind.UriEncoded))
		{
			var targetBase = SchemaRegistry.Global.Get(newBaseUri);
			if (targetBase == null)
				throw new JsonSchemaException($"Cannot resolve base schema from `{newUri}`");
			targetSchema = targetBase.FindSubschema(pointerFragment!, newBaseUri).Item1;
		}
		else
		{
			var anchorFragment = newUri.Fragment.Substring(1);
			if (!AnchorKeyword.AnchorPattern.IsMatch(anchorFragment))
				throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");
			foreach (var id in scope)
			{
				var targetBase = SchemaRegistry.Global.Get(id);
				if (targetBase == null)
					throw new JsonSchemaException($"Cannot resolve base schema `{id}`");
				if (targetBase.Anchors.TryGetValue(anchorFragment, out var anchor) && anchor.IsDynamic)
				{
					targetSchema = anchor.Schema;
					break;
				}
			}
			// if we don't find a dynamic anchor, use the $ref behavior
			if (targetSchema == null &&
			    (SchemaRegistry.Global.Get(newBaseUri)?.Anchors.TryGetValue(anchorFragment, out var anchorDefinition) ?? false))
				targetSchema = anchorDefinition.Schema;
		}

		if (targetSchema == null)
			throw new JsonSchemaException($"Cannot resolve schema `{newUri}`");

		var newScope = scope.Append(newUri);
		foreach (var requirement in targetSchema.GenerateRequirements(newScope, subschemaPath.Combine(Name), instanceLocation, options))
		{
			yield return requirement;
		}

		yield return new Requirement(subschemaPath, instanceLocation,
			(_, cache, _) =>
			{
				return new KeywordResult(Name, subschemaPath, scope.LocalScope, instanceLocation)
				{
					ValidationResult = cache.GetLocalResults(subschemaPath, Name).All(x => x.ValidationResult != false)
				};
			});
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
	public bool Equals(DynamicRefKeyword? other)
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
		return Equals(obj as DynamicRefKeyword);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Reference.GetHashCode();
	}
}

internal class DynamicRefKeywordJsonConverter : JsonConverter<DynamicRefKeyword>
{
	public override DynamicRefKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var uri = reader.GetString()!;
		return new DynamicRefKeyword(new Uri(uri, UriKind.RelativeOrAbsolute));


	}
	public override void Write(Utf8JsonWriter writer, DynamicRefKeyword value, JsonSerializerOptions options)
	{
		writer.WritePropertyName(DynamicRefKeyword.Name);
		JsonSerializer.Serialize(writer, value.Reference, options);
	}
}