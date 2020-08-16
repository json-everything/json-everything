using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[SchemaPriority(long.MinValue)]
	[JsonConverter(typeof(IdKeywordJsonConverter))]
	public class IdKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "$id";

		public Uri Id { get; }

		public IdKeyword(Uri id)
		{
			Id = id;
		}

		public void Validate(ValidationContext context)
		{
			context.ParentContext.CurrentUri = context.CurrentUri == null ? Id : new Uri(context.CurrentUri, Id);
			context.IsValid = true;

			if (Id.IsAbsoluteUri)
				context.Registry.Register(Id, context.LocalSchema);
		}
	}

	public class IdKeywordJsonConverter : JsonConverter<IdKeyword>
	{
		public override IdKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var uriString = reader.GetString();
			if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
				throw new JsonException("Expected absolute URI");

			return new IdKeyword(uri);
		}

		public override void Write(Utf8JsonWriter writer, IdKeyword value, JsonSerializerOptions options)
		{
			writer.WriteString(IdKeyword.Name, value.Id.OriginalString);
		}
	}
}