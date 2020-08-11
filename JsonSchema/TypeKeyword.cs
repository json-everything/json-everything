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
			switch (Type)
			{
				case SchemaValueType.Object:
					valid = context.Instance.ValueKind == JsonValueKind.Object;
					break;
				case SchemaValueType.Array:
					valid = context.Instance.ValueKind == JsonValueKind.Array;
					break;
				case SchemaValueType.Boolean:
					valid = context.Instance.ValueKind == JsonValueKind.False ||
					        context.Instance.ValueKind == JsonValueKind.True;
					break;
				case SchemaValueType.String:
					valid = context.Instance.ValueKind == JsonValueKind.String;
					break;
				case SchemaValueType.Number:
					valid = context.Instance.ValueKind == JsonValueKind.Number;
					break;
				case SchemaValueType.Integer:
					valid = context.Instance.ValueKind == JsonValueKind.Number;
					if (valid)
					{
						var number = context.Instance.GetDecimal();
						valid &= number == Math.Truncate(number);
					}
					break;
				case SchemaValueType.Null:
					valid = context.Instance.ValueKind == JsonValueKind.Null;
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