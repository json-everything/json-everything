using System;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema
{
	public class ValidationContext
	{
		public SchemaRegistry Registry { get; internal set; }
		public JsonSchema SchemaRoot { get; private set; }
		public JsonElement InstanceRoot { get; internal set; }

		public JsonPointer InstanceLocation { get; internal set; }
		public JsonElement Instance { get; internal set; }
		public JsonPointer SchemaLocation { get; internal set; }

		public Uri CurrentUri { get; internal set; }

		internal ValidationContext() { }

		public static ValidationContext From(ValidationContext source,
		                                     JsonPointer? instanceLocation = null,
		                                     JsonElement? instance = null,
		                                     JsonPointer? subschemaLocation = null)
		{
			return new ValidationContext
				{
					Registry = source.Registry,
					InstanceRoot = source.InstanceRoot,
					SchemaRoot = source.SchemaRoot,
					InstanceLocation = instanceLocation ?? source.InstanceLocation,
					Instance = instance?.Clone() ?? source.Instance.Clone(),
					SchemaLocation = subschemaLocation ?? source.SchemaLocation,
				};
		}
	}
}