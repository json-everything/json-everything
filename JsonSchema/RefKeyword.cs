using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Core201909Id)]
	[JsonConverter(typeof(RefKeywordJsonConverter))]
	public class RefKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$ref";

		public Uri Reference { get; }

		public RefKeyword(Uri value)
		{
			Reference = value;
		}

		public void Validate(ValidationContext context)
		{
			var parts = Reference.OriginalString.Split(new []{'#'}, StringSplitOptions.None);
			var baseUri = parts[0];
			var fragment = parts.Length > 1 ? parts[1] : null;

			Uri newUri;
			JsonSchema baseSchema = null;
			if (!string.IsNullOrEmpty(baseUri))
			{
				if (Uri.TryCreate(baseUri, UriKind.Absolute, out newUri))
					baseSchema = context.Registry.Get(newUri);
				else if (context.CurrentUri != null)
				{
					var uriFolder = context.CurrentUri.OriginalString.EndsWith("/")
						? context.CurrentUri
						: context.CurrentUri.GetParentUri();
					newUri = uriFolder;
					var newBaseUri = new Uri(uriFolder, baseUri);
					if (!string.IsNullOrEmpty(fragment))
						newUri = newBaseUri;
					baseSchema = context.Registry.Get(newBaseUri);
				}
			}
			else
			{
				baseSchema = context.SchemaRoot;
				newUri = context.CurrentUri;
			}

			JsonSchema schema;
			if (!string.IsNullOrEmpty(fragment) && AnchorKeyword.AnchorPattern.IsMatch(fragment))
				schema = context.Registry.Get(newUri, fragment);
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
			if (!ReferenceEquals(baseSchema, context.SchemaRoot)) 
				subContext.SchemaRoot = baseSchema;
			schema.ValidateSubschema(subContext);
			context.NestedContexts.Add(subContext);
			context.ConsolidateAnnotations();
			context.IsValid = subContext.IsValid;
		}
	}

	public class RefKeywordJsonConverter : JsonConverter<RefKeyword>
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