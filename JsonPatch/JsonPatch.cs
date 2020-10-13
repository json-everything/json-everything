using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Patch
{
	[JsonConverter(typeof(PatchJsonConverter))]
	public class JsonPatch
	{
		public IReadOnlyList<PatchOperation> Operations { get; }

		public JsonPatch(IEnumerable<PatchOperation> operations)
		{
			Operations = operations.ToList().AsReadOnly();
		}

		public PatchResult Process(JsonElement source)
		{
			var context = new PatchContext{Source = source};

			foreach (var operation in Operations)
			{
				operation.Handle(context);
				if (context.Message != null) break;
			}

			return new PatchResult(context);
		}
	}

	internal class PatchJsonConverter : JsonConverter<JsonPatch>
	{
		public override JsonPatch Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var operations = JsonSerializer.Deserialize<List<PatchOperation>>(ref reader, new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase});

			return new JsonPatch(operations);
		}

		public override void Write(Utf8JsonWriter writer, JsonPatch value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}