﻿using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Patch;

/// <summary>
/// Represents a single JSON Patch operation.
/// </summary>
[JsonConverter(typeof(PatchOperationJsonConverter))]
public class PatchOperation : IEquatable<PatchOperation>
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
	public JsonNode? Value { get; }

	private PatchOperation(OperationType op, JsonPointer from, JsonPointer path, JsonNode? value, IPatchOperationHandler handler)
	{
		_handler = handler;
		Op = op;
		From = from;
		Path = path;
		Value = value?.DeepClone();
	}

	/// <summary>
	/// Creates an `add` operation.
	/// </summary>
	/// <param name="path">The source path.</param>
	/// <param name="value">The value to add.</param>
	/// <returns>An `add` operation.</returns>
	public static PatchOperation Add(JsonPointer path, JsonNode? value)
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
	public static PatchOperation Replace(JsonPointer path, JsonNode? value)
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
	public static PatchOperation Test(JsonPointer path, JsonNode? value)
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
	public bool Equals(PatchOperation? other)
	{
		if (other is null) return false;

		return Op == other.Op &&
			   From.Equals(other.From) &&
			   Path.Equals(other.Path) &&
			   Value.IsEquivalentTo(other.Value);
	}

	/// <summary>Indicates whether this instance and a specified object are equal.</summary>
	/// <param name="obj">The object to compare with the current instance.</param>
	/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as PatchOperation);
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = (int)Op;
			hashCode = (hashCode * 397) ^ From.GetHashCode();
			hashCode = (hashCode * 397) ^ Path.GetHashCode();
			hashCode = (hashCode * 397) ^ (Value?.GetHashCode() ?? 0);
			return hashCode;
		}
	}
}

internal class PatchOperationJsonConverter : JsonConverter<PatchOperation>
{
	internal class Model
	{
		[JsonPropertyName("op")]
		public OperationType Op { get; set; }
		[JsonPropertyName("from")]
		public JsonPointer? From { get; set; }
		[JsonPropertyName("path")]
		public JsonPointer? Path { get; set; }
		[JsonPropertyName("value")]
		public JsonElement Value { get; set; }
	}

	public override PatchOperation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var model = options.Read(ref reader, JsonPatchSerializerContext.Default.Model)!;

		if (model.Path == null)
			throw new JsonException($"`{model.Op}` operation requires `path`");

		switch (model.Op)
		{
			case OperationType.Add:
				if (model.Value.ValueKind == JsonValueKind.Undefined)
					throw new JsonException("`add` operation requires `value`");
				return PatchOperation.Add(model.Path.Value, model.Value.AsNode());
			case OperationType.Remove:
				return PatchOperation.Remove(model.Path.Value);
			case OperationType.Replace:
				if (model.Value.ValueKind == JsonValueKind.Undefined)
					throw new JsonException("`replace` operation requires `value`");
				return PatchOperation.Replace(model.Path.Value, model.Value.AsNode());
			case OperationType.Move:
				if (model.From == null)
					throw new JsonException("`move` operation requires `from`");
				return PatchOperation.Move(model.From.Value, model.Path.Value);
			case OperationType.Copy:
				if (model.From == null)
					throw new JsonException("`copy` operation requires `from`");
				return PatchOperation.Copy(model.From.Value, model.Path.Value);
			case OperationType.Test:
				if (model.Value.ValueKind == JsonValueKind.Undefined)
					throw new JsonException("`test` operation requires `value`");
				return PatchOperation.Test(model.Path.Value, model.Value.AsNode());
			case OperationType.Unknown:
			default:
				throw new JsonException($"Unknown operation type: {model.Op}");
		}
	}

	public override void Write(Utf8JsonWriter writer, PatchOperation value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WritePropertyName("op");
		options.Write(writer, value.Op, JsonPatchSerializerContext.Default.OperationType);

		writer.WritePropertyName("path");
		options.Write(writer, value.Path, JsonPatchSerializerContext.Default.JsonPointer);

		switch (value.Op)
		{
			case OperationType.Add:
				writer.WritePropertyName("value");
				options.Write(writer, value.Value, JsonPatchSerializerContext.Default.JsonNode!);
				break;
			case OperationType.Remove:
				break;
			case OperationType.Replace:
				writer.WritePropertyName("value");
				options.Write(writer, value.Value, JsonPatchSerializerContext.Default.JsonNode!);
				break;
			case OperationType.Move:
				writer.WritePropertyName("from");
				options.Write(writer, value.From, JsonPatchSerializerContext.Default.JsonPointer);
				break;
			case OperationType.Copy:
				writer.WritePropertyName("from");
				options.Write(writer, value.From, JsonPatchSerializerContext.Default.JsonPointer);
				break;
			case OperationType.Test:
				writer.WritePropertyName("value");
				options.Write(writer, value.Value, JsonPatchSerializerContext.Default.JsonNode!);
				break;
			case OperationType.Unknown:
			default:
				throw new ArgumentOutOfRangeException("value.Op");
		}

		writer.WriteEndObject();
	}
}