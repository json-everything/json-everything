using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema.Data
{
	[SchemaKeyword(Name)]
	[SchemaPriority(int.MinValue)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.DataId)]
	[JsonConverter(typeof(DataKeywordJsonConverter))]
	public class DataKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "data";

		public IReadOnlyDictionary<string, Uri> References { get; }

		public DataKeyword(IReadOnlyDictionary<string, Uri> references)
		{
			References = references;
		}

		public void Validate(ValidationContext context)
		{
			// resolve data
			
			// serialize values
			
			// deserialize as schema

			// return evaluation

			throw new NotImplementedException();
		}
	}

	internal class DataKeywordJsonConverter : JsonConverter<DataKeyword>
	{
		public override DataKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var references = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options)
				.ToDictionary(kvp => kvp.Key, kvp => new Uri(kvp.Value));
			return new DataKeyword(references);
		}

		public override void Write(Utf8JsonWriter writer, DataKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DataKeyword.Name);
			writer.WriteStartObject();
			foreach (var kvp in value.References)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}
