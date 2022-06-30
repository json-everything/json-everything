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
[Vocabulary(Vocabularies.Core201909Id)]
[Vocabulary(Vocabularies.Core202012Id)]
[JsonConverter(typeof(RefKeywordJsonConverter))]
public class RefKeyword : IJsonSchemaKeyword, IEquatable<RefKeyword>
{
	internal const string Name = "$ref";

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
	/// Provides validation for the keyword.
	/// </summary>
	/// <param name="context">Contextual details for the validation process.</param>
	public void Validate(ValidationContext context)
	{
		context.EnterKeyword(Name);
		var parts = Reference.OriginalString.Split(new[] { '#' }, StringSplitOptions.None);
		var baseUri = parts[0];
		var fragment = parts.Length > 1 ? parts[1] : null;

		Uri? newUri;
		JsonSchema? baseSchema;
		if (!string.IsNullOrEmpty(baseUri))
		{
			if (Uri.TryCreate(baseUri, UriKind.Absolute, out newUri))
				baseSchema = context.Options.SchemaRegistry.Get(newUri);
			else
			{
				var uriFolder = context.CurrentUri.OriginalString.EndsWith("/")
					? context.CurrentUri
					: context.CurrentUri.GetParentUri();
				newUri = new Uri(uriFolder, baseUri);
				baseSchema = context.Options.SchemaRegistry.Get(newUri);
			}
		}
		else
		{
			newUri = context.CurrentUri;
			baseSchema = context.Options.SchemaRegistry.Get(newUri) ?? context.SchemaRoot;
		}

		var absoluteReference = SchemaRegistry.GetFullReference(newUri, fragment);
		var navigation = (absoluteReference, context.InstanceLocation);
		if (context.NavigatedReferences.Contains(navigation))
		{
			context.LocalResult.Fail(Name, ErrorMessages.RecursiveRef);
			context.ExitKeyword(Name, false);
			return;
		}

		JsonSchema? schema;
		var navigatedByDirectRef = true;
		if (!string.IsNullOrEmpty(fragment) && AnchorKeyword.AnchorPattern.IsMatch(fragment!))
			schema = context.Options.SchemaRegistry.Get(newUri, fragment);
		else
		{
			if (baseSchema == null)
			{
				context.LocalResult.Fail(Name, ErrorMessages.BaseUriResolution, ("uri", newUri!.OriginalString));
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
				navigatedByDirectRef = false;
			}
			else
				schema = baseSchema;
		}

		if (schema == null)
		{
			context.LocalResult.Fail(Name, ErrorMessages.RefResolution, ("uri", Reference.OriginalString));
			context.ExitKeyword(Name, false);
			return;
		}

		context.NavigatedReferences.Add(navigation);

		context.Push(context.EvaluationPath.Combine(Name), schema, newUri);
		context.NavigatedByDirectRef = navigatedByDirectRef;
		if (!string.IsNullOrEmpty(fragment) && JsonPointer.TryParse(fragment!, out var reference))
			context.Reference = reference;
		context.Validate();
		var result = context.LocalResult.IsValid;
		context.Pop();
		if (result)
			context.LocalResult.Pass();
		else
			context.LocalResult.Fail(Name);

		context.NavigatedReferences.Remove(navigation);

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
		return new RefKeyword(new Uri(uri, UriKind.RelativeOrAbsolute));


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