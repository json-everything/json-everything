using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(TypeKeywordJsonConverter))]
	public class TypeKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "type";

		public SchemaValueType Type { get; }

		public TypeKeyword(SchemaValueType type)
		{
			Type = type;
		}

		public TypeKeyword(params SchemaValueType[] types)
		{
			// TODO: protect input

			Type = types.Aggregate((x, y) => x | y);
		}

		public TypeKeyword(IEnumerable<SchemaValueType> types)
		{
			// TODO: protect input

			Type = types.Aggregate((x, y) => x | y);
		}

		public ValidationResults Validate(ValidationContext context)
		{
			bool valid;
			switch (context.Instance.ValueKind)
			{
				case JsonValueKind.Object:
					valid = Type.HasFlag(SchemaValueType.Object);
					break;
				case JsonValueKind.Array:
					valid = Type.HasFlag(SchemaValueType.Array);
					break;
				case JsonValueKind.String:
					valid = Type.HasFlag(SchemaValueType.String);
					break;
				case JsonValueKind.Number:
					if (Type.HasFlag(SchemaValueType.Number))
						valid = true;
					else if (Type.HasFlag(SchemaValueType.Integer))
					{
						var number = context.Instance.GetDecimal();
						valid = number == Math.Truncate(number);
					}
					else
						valid = false;
					break;
				case JsonValueKind.True:
				case JsonValueKind.False:
					valid = Type.HasFlag(SchemaValueType.Boolean);
					break;
				case JsonValueKind.Null:
					valid = Type.HasFlag(SchemaValueType.Null);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			var found = context.Instance.ValueKind.ToString().ToLower();
			var expected = Type.ToString().ToLower();
			return valid
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context, $"Value is {found} but should be {expected}");
		}
	}

	public class TypeKeywordJsonConverter : JsonConverter<TypeKeyword>
	{
		public override TypeKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var types = JsonSerializer.Deserialize<List<SchemaValueType>>(ref reader);
				return new TypeKeyword(types);
			}

			var type = JsonSerializer.Deserialize<SchemaValueType>(ref reader);

			return new TypeKeyword(type);
		}
		public override void Write(Utf8JsonWriter writer, TypeKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(TypeKeyword.Name);
			JsonSerializer.Serialize(writer, value.Type);
		}
	}
}