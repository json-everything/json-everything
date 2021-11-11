using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Patch
{
	/// <summary>
	/// Represents a single JSON Patch operation.
	/// </summary>
	[JsonConverter(typeof(PatchOperationJsonConverter))]
	public readonly struct PatchOperation : IEquatable<PatchOperation>
	{
		private readonly IPatchOperationHandler _handler;

		/// <summary>
		/// Gets the operation type.
		/// </summary>
		public OperationType Op { get; }
		/// <summary>
		/// Gets the source path.
		/// </summary>
		public JsonPointer From { get; }
		/// <summary>
		/// Gets the target path.
		/// </summary>
		public JsonPointer Path { get; }
		/// <summary>
		/// Gets the discrete value.
		/// </summary>
		public JsonElement Value { get; }

		private PatchOperation(OperationType op, JsonPointer from, JsonPointer path, JsonElement value, IPatchOperationHandler handler)
		{
			_handler = handler;
			Op = op;
			From = from;
			Path = path;
			Value = value.ValueKind == JsonValueKind.Undefined ? default : value.Clone();
		}

		/// <summary>
		/// Creates an `add` operation.
		/// </summary>
		/// <param name="path">The source path.</param>
		/// <param name="value">The value to add.</param>
		/// <returns>An `add` operation.</returns>
		public static PatchOperation Add(JsonPointer path, JsonElement value)
		{
			return new PatchOperation(OperationType.Add, JsonPointer.Empty, path, value, AddOperationHandler.Instance);
		}

		/// <summary>
		/// Creates an `add` operation.
		/// </summary>
		/// <param name="path">The source path.</param>
		/// <param name="value">The value to add.</param>
		/// <returns>An `add` operation.</returns>
		public static PatchOperation Add(JsonPointer path, JsonElementProxy value)
		{
			return new PatchOperation(OperationType.Add, JsonPointer.Empty, path, value, AddOperationHandler.Instance);
		}

		/// <summary>
		/// Creates an `remove` operation.
		/// </summary>
		/// <param name="path">The source path.</param>
		/// <returns>An `remove` operation.</returns>
		public static PatchOperation Remove(JsonPointer path)
		{
			return new PatchOperation(OperationType.Remove, JsonPointer.Empty, path, default, RemoveOperationHandler.Instance);
		}

		/// <summary>
		/// Creates an `replace` operation.
		/// </summary>
		/// <param name="path">The source path.</param>
		/// <param name="value">The value to add.</param>
		/// <returns>An `replace` operation.</returns>
		public static PatchOperation Replace(JsonPointer path, JsonElement value)
		{
			return new PatchOperation(OperationType.Replace, JsonPointer.Empty, path, value, ReplaceOperationHandler.Instance);
		}

		/// <summary>
		/// Creates an `replace` operation.
		/// </summary>
		/// <param name="path">The source path.</param>
		/// <param name="value">The value to add.</param>
		/// <returns>An `replace` operation.</returns>
		public static PatchOperation Replace(JsonPointer path, JsonElementProxy value)
		{
			return new PatchOperation(OperationType.Replace, JsonPointer.Empty, path, value, ReplaceOperationHandler.Instance);
		}

		/// <summary>
		/// Creates an `move` operation.
		/// </summary>
		/// <param name="path">The target path.</param>
		/// <param name="from">The path to the value to move.</param>
		/// <returns>An `move` operation.</returns>
		public static PatchOperation Move(JsonPointer from, JsonPointer path)
		{
			return new PatchOperation(OperationType.Move, from, path, default, MoveOperationHandler.Instance);
		}

		/// <summary>
		/// Creates an `copy` operation.
		/// </summary>
		/// <param name="path">The target path.</param>
		/// <param name="from">The path to the value to move.</param>
		/// <returns>An `copy` operation.</returns>
		public static PatchOperation Copy(JsonPointer from, JsonPointer path)
		{
			return new PatchOperation(OperationType.Copy, from, path, default, CopyOperationHandler.Instance);
		}

		/// <summary>
		/// Creates an `test` operation.
		/// </summary>
		/// <param name="path">The source path.</param>
		/// <param name="value">The value to match.</param>
		/// <returns>An `test` operation.</returns>
		public static PatchOperation Test(JsonPointer path, JsonElement value)
		{
			return new PatchOperation(OperationType.Test, JsonPointer.Empty, path, value, TestOperationHandler.Instance);
		}

		/// <summary>
		/// Creates an `test` operation.
		/// </summary>
		/// <param name="path">The source path.</param>
		/// <param name="value">The value to match.</param>
		/// <returns>An `test` operation.</returns>
		public static PatchOperation Test(JsonPointer path, JsonElementProxy value)
		{
			return new PatchOperation(OperationType.Test, JsonPointer.Empty, path, value, TestOperationHandler.Instance);
		}

		internal void Handle(PatchContext context)
		{
			_handler.Process(context, this);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(PatchOperation other)
		{
			return Op == other.Op &&
			       From.Equals(other.From) &&
			       Path.Equals(other.Path) &&
			       ((Value.ValueKind == JsonValueKind.Undefined &&
			         other.Value.ValueKind == JsonValueKind.Undefined) ||
			        Value.IsEquivalentTo(other.Value));
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is PatchOperation other && Equals(other);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int) Op;
				hashCode = (hashCode * 397) ^ From.GetHashCode();
				hashCode = (hashCode * 397) ^ Path.GetHashCode();
				hashCode = (hashCode * 397) ^ Value.GetHashCode();
				return hashCode;
			}
		}
	}

	internal class PatchOperationJsonConverter : JsonConverter<PatchOperation>
	{
		private class Model
		{
			public OperationType Op { get; set; }
			public JsonPointer? From { get; set; }
			public JsonPointer? Path { get; set; }
			public JsonElement Value { get; set; }
		}

		public override PatchOperation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var model = JsonSerializer.Deserialize<Model>(ref reader, options);

			if (model.Path == null)
				throw new JsonException($"`{model.Op}` operation requires `path`");

			switch (model.Op)
			{
				case OperationType.Add:
					if (model.Value.ValueKind == JsonValueKind.Undefined)
						throw new JsonException("`add` operation requires `value`");
					return PatchOperation.Add(model.Path, model.Value);
				case OperationType.Remove:
					return PatchOperation.Remove(model.Path);
				case OperationType.Replace:
					if (model.Value.ValueKind == JsonValueKind.Undefined)
						throw new JsonException("`replace` operation requires `value`");
					return PatchOperation.Replace(model.Path, model.Value);
				case OperationType.Move:
					if (model.From == null)
						throw new JsonException("`move` operation requires `from`");
					return PatchOperation.Move(model.From, model.Path);
				case OperationType.Copy:
					if (model.From == null)
						throw new JsonException("`copy` operation requires `from`");
					return PatchOperation.Copy(model.From, model.Path);
				case OperationType.Test:
					if (model.Value.ValueKind == JsonValueKind.Undefined)
						throw new JsonException("`test` operation requires `value`");
					return PatchOperation.Test(model.Path, model.Value);
				case OperationType.Unknown:
				default:
					throw new JsonException();
			}
		}

		public override void Write(Utf8JsonWriter writer, PatchOperation value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();

			writer.WritePropertyName("op");
			JsonSerializer.Serialize(writer, value.Op);

			writer.WritePropertyName("path");
			JsonSerializer.Serialize(writer, value.Path);

			switch (value.Op)
			{
				case OperationType.Add:
					writer.WritePropertyName("value");
					JsonSerializer.Serialize(writer, value.Value);
					break;
				case OperationType.Remove:
					break;
				case OperationType.Replace:
					writer.WritePropertyName("value");
					JsonSerializer.Serialize(writer, value.Value);
					break;
				case OperationType.Move:
					writer.WritePropertyName("from");
					JsonSerializer.Serialize(writer, value.From);
					break;
				case OperationType.Copy:
					writer.WritePropertyName("from");
					JsonSerializer.Serialize(writer, value.From);
					break;
				case OperationType.Test:
					writer.WritePropertyName("value");
					JsonSerializer.Serialize(writer, value.Value);
					break;
				case OperationType.Unknown:
				default:
					throw new ArgumentOutOfRangeException();
			}

			writer.WriteEndObject();
		}
	}
}