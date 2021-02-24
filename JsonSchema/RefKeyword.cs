using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
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
			var parts = Reference.OriginalString.Split(new []{'#'}, StringSplitOptions.None);
			var baseUri = parts[0];
			var fragment = parts.Length > 1 ? parts[1] : null;

			Uri? newUri;
			JsonSchema? baseSchema = null;
			if (!string.IsNullOrEmpty(baseUri))
			{
				if (Uri.TryCreate(baseUri, UriKind.Absolute, out newUri))
					baseSchema = context.Options.SchemaRegistry.Get(newUri);
				else if (context.CurrentUri != null)
				{
					var uriFolder = context.CurrentUri.OriginalString.EndsWith("/")
						? context.CurrentUri
						: context.CurrentUri.GetParentUri();
					newUri = uriFolder;
					var newBaseUri = new Uri(uriFolder, baseUri);
					if (!string.IsNullOrEmpty(fragment))
						newUri = newBaseUri;
					baseSchema = context.Options.SchemaRegistry.Get(newBaseUri);
				}
			}
			else
			{
				newUri = context.CurrentUri;
				baseSchema = context.Options.SchemaRegistry.Get(newUri) ?? context.SchemaRoot;
			}

			JsonSchema? schema;
			if (!string.IsNullOrEmpty(fragment) && AnchorKeyword.AnchorPattern.IsMatch(fragment!))
				schema = context.Options.SchemaRegistry.Get(newUri, fragment);
			else
			{
				if (baseSchema == null)
				{
					context.IsValid = false;
					context.Message = $"Could not resolve base URI `{baseUri}`";
					return;
				}

				if (!string.IsNullOrEmpty(fragment))
				{
					fragment = $"#{fragment}";
					if (!JsonPointer.TryParse(fragment, out var pointer))
					{
						context.IsValid = false;
						context.Message = $"Could not parse pointer `{fragment}`";
						return;
					}
					(schema, newUri) = baseSchema.FindSubschema(pointer, newUri);
				}
				else
					schema = baseSchema;
			}

			if (schema == null)
			{
				context.IsValid = false;
				context.Message = $"Could not resolve reference `{Reference}`";
				return;
			}

			var subContext = ValidationContext.From(context, newUri: newUri);
			if (!string.IsNullOrEmpty(fragment) && JsonPointer.TryParse(fragment!, out var reference)) 
				subContext.Reference = reference;
			if (!ReferenceEquals(baseSchema, context.SchemaRoot)) 
				subContext.SchemaRoot = baseSchema!;
			schema.ValidateSubschema(subContext);
			context.NestedContexts.Add(subContext);
			context.ConsolidateAnnotations();
			context.IsValid = subContext.IsValid;
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

	// Source: https://github.com/WebDAVSharp/WebDAVSharp.Server/blob/1d2086a502937936ebc6bfe19cfa15d855be1c31/WebDAVExtensions.cs
}