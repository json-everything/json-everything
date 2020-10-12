using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Patch
{
	[JsonConverter(typeof(PatchOperationJsonConverter))]
	public readonly struct PatchOperation
	{
		private readonly IPatchOperationHandler _handler;

		public OperationType Op { get; }
		public JsonPointer? From { get; }
		public JsonPointer? Path { get; }
		public JsonElement? Value { get; }

		private PatchOperation(OperationType op, JsonPointer? from, JsonPointer? path, JsonElement? value, IPatchOperationHandler handler)
		{
			_handler = handler;
			Op = op;
			From = from;
			Path = path;
			Value = value?.Clone();
		}

		public static PatchOperation Add(JsonPointer path, JsonElement value)
		{
			return new PatchOperation(OperationType.Add, null, path, value, AddOperationHandler.Instance);
		}

		public static PatchOperation Remove(JsonPointer path)
		{
			return new PatchOperation(OperationType.Remove, null, path, null, RemoveOperationHandler.Instance);
		}

		public static PatchOperation Replace(JsonPointer path, JsonElement value)
		{
			return new PatchOperation(OperationType.Replace, null, path, value, ReplaceOperationHandler.Instance);
		}

		public static PatchOperation Move(JsonPointer from, JsonPointer path)
		{
			return new PatchOperation(OperationType.Move, from, path, null, MoveOperationHandler.Instance);
		}

		public static PatchOperation Copy(JsonPointer from, JsonPointer path)
		{
			return new PatchOperation(OperationType.Copy, from, path, null, CopyOperationHandler.Instance);
		}

		public static PatchOperation Test(JsonPointer path, JsonElement value)
		{
			return new PatchOperation(OperationType.Test, null, path, value, TestOperationHandler.Instance);
		}

		internal void Handle(PatchContext context)
		{
			_handler.Process(context);
		}
	}

	internal class PatchOperationJsonConverter : JsonConverter<PatchOperation>
	{
		private class Model
		{
			public OperationType Op { get; set; }
			public JsonPointer? From { get; set; }
			public JsonPointer? Path { get; set; }
			public JsonElement? Value { get; set; }
		}

		public override PatchOperation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var model = JsonSerializer.Deserialize<Model>(ref reader, options);

			switch (model.Op)
			{
				case OperationType.Add:
					if (!model.Path.HasValue)
						throw new JsonException("`add` operation requires `path`");
					if (!model.Value.HasValue)
						throw new JsonException("`add` operation requires `value`");
					return PatchOperation.Add(model.Path.Value, model.Value.Value);
				case OperationType.Remove:
					if (!model.Path.HasValue)
						throw new JsonException("`remove` operation requires `path`");
					return PatchOperation.Remove(model.Path.Value);
				case OperationType.Replace:
					if (!model.Path.HasValue)
						throw new JsonException("`replace` operation requires `path`");
					if (!model.Value.HasValue)
						throw new JsonException("`replace` operation requires `value`");
					return PatchOperation.Replace(model.Path.Value, model.Value.Value);
				case OperationType.Move:
					if (!model.From.HasValue)
						throw new JsonException("`move` operation requires `from`");
					if (!model.Path.HasValue)
						throw new JsonException("`move` operation requires `path`");
					return PatchOperation.Move(model.Path.Value, model.Path.Value);
				case OperationType.Copy:
					if (!model.From.HasValue)
						throw new JsonException("`copy` operation requires `from`");
					if (!model.Path.HasValue)
						throw new JsonException("`copy` operation requires `path`");
					return PatchOperation.Copy(model.From.Value, model.Path.Value);
				case OperationType.Test:
					if (!model.Path.HasValue)
						throw new JsonException("`test` operation requires `path`");
					if (!model.Value.HasValue)
						throw new JsonException("`test` operation requires `value`");
					return PatchOperation.Test(model.Path.Value, model.Value.Value);
				case OperationType.Unknown:
				default:
					throw new JsonException();
			}
		}

		public override void Write(Utf8JsonWriter writer, PatchOperation value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}