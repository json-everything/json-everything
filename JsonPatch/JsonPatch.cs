using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Patch
{
	/// <summary>
	/// Models a JSON Patch document.
	/// </summary>
	[JsonConverter(typeof(PatchJsonConverter))]
	public class JsonPatch : IEquatable<JsonPatch>
	{
		/// <summary>
		/// Gets the collection of operations.
		/// </summary>
		public IReadOnlyList<PatchOperation> Operations { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="JsonPatch"/> class.
		/// </summary>
		/// <param name="operations">The collection of operations.</param>
		public JsonPatch(params PatchOperation[] operations)
		{
			Operations = operations.ToList().AsReadOnly();
		}

		/// <summary>
		/// Creates a new instance of the <see cref="JsonPatch"/> class.
		/// </summary>
		/// <param name="operations">The collection of operations.</param>
		public JsonPatch(IEnumerable<PatchOperation> operations)
		{
			Operations = operations.ToList().AsReadOnly();
		}

		/// <summary>
		/// Applies the patch to a JSON document.
		/// </summary>
		/// <param name="source">The JSON document.</param>
		/// <returns>A result object containing the output JSON and a possible error message.</returns>
		public PatchResult Apply(JsonElement source)
		{
			var context = new PatchContext(new EditableJsonElement(source));

			foreach (var operation in Operations)
			{
				operation.Handle(context);
				if (context.Message != null) break;
				context.Index++;
			}

			return new PatchResult(context);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(JsonPatch? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Operations.SequenceEqual(other.Operations);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as JsonPatch);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Operations.GetCollectionHashCode();
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
			JsonSerializer.Serialize(writer, value.Operations);
		}
	}
}