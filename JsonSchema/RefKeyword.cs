using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
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
			var pointerString = parts.Length > 1 ? parts[1] : null;

			JsonSchema baseSchema = null;
			if (!string.IsNullOrEmpty(baseUri))
			{
				if (Uri.TryCreate(baseUri, UriKind.Absolute, out var newBase))
					baseSchema = context.Registry.Get(newBase);
				else if (context.CurrentUri != null)
				{
					var uriFolder = context.CurrentUri.OriginalString.EndsWith("/") ? context.CurrentUri : context.CurrentUri.GetParentUri();
					newBase = new Uri(uriFolder, baseUri);
					baseSchema = context.Registry.Get(newBase);
				}
			}
			else
				baseSchema = context.SchemaRoot;
			if (baseSchema == null)
			{
				context.IsValid = false;
				context.Message = $"Could not resolve base URI `{baseUri}`";
				return;
			}

			JsonSchema schema;
			if (!string.IsNullOrEmpty(pointerString))
			{
				pointerString = $"#{pointerString}";
				if (!JsonPointer.TryParse(pointerString, out var pointer))
				{
					context.IsValid = false;
					context.Message = $"Could not parse pointer `{pointerString}`";
					return;
				}
				schema = baseSchema.FindSubschema(pointer);
			}
			else
				schema = baseSchema;

			if (schema == null)
			{
				context.IsValid = false;
				context.Message = $"Could not resolve reference `{Reference}`";
				return;
			}

			var subContext = ValidationContext.From(context);
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