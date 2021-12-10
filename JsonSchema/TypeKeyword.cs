using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `type`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(TypeKeywordJsonConverter))]
	public class TypeKeyword : IJsonSchemaKeyword, IEquatable<TypeKeyword>
	{
		internal const string Name = "type";

		/// <summary>
		/// The expected type.
		/// </summary>
		public SchemaValueType Type { get; }

		/// <summary>
		/// Creates a new <see cref="TypeKeyword"/>.
		/// </summary>
		/// <param name="type">The expected type.</param>
		public TypeKeyword(SchemaValueType type)
		{
			Type = type;
		}

		/// <summary>
		/// Creates a new <see cref="TypeKeyword"/>.
		/// </summary>
		/// <param name="types">The expected types.</param>
		public TypeKeyword(params SchemaValueType[] types)
		{
			// TODO: protect input

			Type = types.Aggregate((x, y) => x | y);
		}

		/// <summary>
		/// Creates a new <see cref="TypeKeyword"/>.
		/// </summary>
		/// <param name="types">The expected types.</param>
		public TypeKeyword(IEnumerable<SchemaValueType> types)
		{
			// TODO: protect input

			Type = types.Aggregate((x, y) => x | y);
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			bool isValid;
			switch (context.LocalInstance.ValueKind)
			{
				case JsonValueKind.Object:
					isValid = Type.HasFlag(SchemaValueType.Object);
					break;
				case JsonValueKind.Array:
					isValid = Type.HasFlag(SchemaValueType.Array);
					break;
				case JsonValueKind.String:
					isValid = Type.HasFlag(SchemaValueType.String);
					break;
				case JsonValueKind.Number:
					if (Type.HasFlag(SchemaValueType.Number))
						isValid = true;
					else if (Type.HasFlag(SchemaValueType.Integer))
					{
						var number = context.LocalInstance.GetDecimal();
						isValid = number == Math.Truncate(number);
					}
					else
						isValid = false;
					break;
				case JsonValueKind.True:
				case JsonValueKind.False:
					isValid = Type.HasFlag(SchemaValueType.Boolean);
					break;
				case JsonValueKind.Null:
					isValid = Type.HasFlag(SchemaValueType.Null);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			var found = context.LocalInstance.ValueKind.ToString().ToLower();
			var expected = Type.ToString().ToLower();
			if (isValid)
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail($"Value is {found} but should be {expected}");
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(TypeKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Type == other.Type;
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as TypeKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return (int) Type;
		}
	}

	internal class TypeKeywordJsonConverter : JsonConverter<TypeKeyword>
	{
		public override TypeKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var type = JsonSerializer.Deserialize<SchemaValueType>(ref reader, options);

			return new TypeKeyword(type);
		}
		public override void Write(Utf8JsonWriter writer, TypeKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(TypeKeyword.Name);
			JsonSerializer.Serialize(writer, value.Type, options);
		}
	}
}