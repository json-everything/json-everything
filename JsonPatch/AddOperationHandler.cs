using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Patch
{
	internal class AddOperationHandler : IPatchOperationHandler
	{
		public static IPatchOperationHandler Instance { get; } = new AddOperationHandler();

		private AddOperationHandler(){}

		public void Process(PatchContext context, PatchOperation operation)
		{
			var pointer = operation.Path.Value;
			var elementStack = new Stack<(string, JsonElement)>();
			JsonElement? localValue = context.Source;
			foreach (var segment in pointer.Segments.Take(pointer.Segments.Length - 1))
			{
				elementStack.Push((segment.Value, localValue.Value));
				var localPointer = JsonPointer.Create(new[] {segment}, false);
				localValue = localPointer.Evaluate(localValue.Value);
				if (localValue == null)
				{
					context.Message = $"Path `{operation.Path}` could not be reached.";
					return;
				}
			}

			var key = pointer.Segments.Last().Value;
			var target = elementStack.Any() ? elementStack.Last().Item2 : context.Source;
			switch (target.ValueKind)
			{
				case JsonValueKind.Object:
					target = localValue.Value.AddOrReplaceKeyInObject(key, operation.Value.Value);
					break;
				case JsonValueKind.Array:
					int index;
					if (key == "-")
						index = -1;
					else if (!int.TryParse(key, out index))
					{
						context.Message = $"Path `{operation.Path}` leads to an array, but `{key}` is not a valid index.";
						return;
					}
					target = localValue.Value.InsertElementInArray(index, operation.Value.Value);
					break;
				case JsonValueKind.Undefined:
				case JsonValueKind.String:
				case JsonValueKind.Number:
				case JsonValueKind.True:
				case JsonValueKind.False:
				case JsonValueKind.Null:
				default:
					context.Message = $"Path `{operation.Path}` could not be reached.";
					return;
			}

			while (elementStack.Any())
			{
				JsonElement local;
				(key, local) = elementStack.Pop();
				switch (local.ValueKind)
				{
					case JsonValueKind.Object:
						target = local.AddOrReplaceKeyInObject(key, target);
						break;
					case JsonValueKind.Array:
						int index;
						if (key == "-")
							index = -1;
						else if (!int.TryParse(key, out index))
							throw new Exception($"Cannot reassemble JSON element at `{key}`.");
						target = local.InsertElementInArray(index, target);
						break;
					case JsonValueKind.Undefined:
					case JsonValueKind.String:
					case JsonValueKind.Number:
					case JsonValueKind.True:
					case JsonValueKind.False:
					case JsonValueKind.Null:
					default:
						throw new Exception($"Cannot reassemble JSON element at `{key}`.");
				}
			}

			context.Source = target;
		}
	}
}