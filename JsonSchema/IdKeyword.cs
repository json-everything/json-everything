using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `$id`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaPriority(long.MinValue + 1)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[Vocabulary(Vocabularies.Core202012Id)]
	[JsonConverter(typeof(IdKeywordJsonConverter))]
	public class IdKeyword : IJsonSchemaKeyword, IEquatable<IdKeyword>
	{
		internal const string Name = "$id";

		/// <summary>
		/// The ID.
		/// </summary>
		public Uri Id { get; }

		/// <summary>
		/// Creates a new <see cref="IdKeyword"/>.
		/// </summary>
		/// <param name="id">The ID.</param>
		public IdKeyword(Uri id)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			if (context.LocalSchema.Keywords!.OfType<RefKeyword>().Any() &&
			    (context.Options.ValidatingAs == Draft.Draft6 || context.Options.ValidatingAs == Draft.Draft7))
			{
				context.LocalResult.Pass();
				context.NotApplicable(() => "$ref present; ignoring");
				return;
			}

			var newUri = context.NavigatedByDirectRef ? context.CurrentUri : UpdateUri(context.CurrentUri);
			context.UriChanged |= context.CurrentUri != newUri;
			if (context.UriChanged) 
				context.CurrentAnchor = null;
			context.Options.SchemaRegistry.EnteringUriScope(newUri!);
			context.UpdateCurrentUri(newUri);
			context.LocalSchema.UpdateBaseUri(newUri);
			context.LocalResult.Pass();
			context.ExitKeyword(Name, true);
		}

		internal Uri UpdateUri(Uri? currentUri)
		{
			if (currentUri == null || Id.IsAbsoluteUri) return Id;

			var idHasBase = Id.OriginalString.IndexOf('#') != 0;
			var baseUri = currentUri;
			if (idHasBase && currentUri.Segments.Length > 1 && currentUri.IsFile)
				baseUri = baseUri.GetParentUri();

			return new Uri(baseUri, Id);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(IdKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Id, other.Id);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as IdKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	internal class IdKeywordJsonConverter : JsonConverter<IdKeyword>
	{
		public override IdKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var uriString = reader.GetString();
			if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
				throw new JsonException("Expected URI");

			return new IdKeyword(uri);
		}

		public override void Write(Utf8JsonWriter writer, IdKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(IdKeyword.Name, value.Id.OriginalString);
		}
	}
}
