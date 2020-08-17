using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema
{
	public class ValidationContext
	{
		public delegate void ContextConsolidator(IList<ValidationContext> sourceContexts, ValidationContext destContext);

		private static readonly List<ContextConsolidator> _consolidationActions = new List<ContextConsolidator>();

		private Dictionary<string, object> _annotations;
		private List<ValidationContext> _nestedContexts;
		private List<ValidationContext> _siblingContexts;

		public bool IsValid { get; set; }
		public string Message { get; set; }
		public Dictionary<string, object> Annotations => _annotations ??= new Dictionary<string, object>();
		public List<ValidationContext> NestedContexts => _nestedContexts ??= new List<ValidationContext>();
		internal List<ValidationContext> SiblingContexts => _siblingContexts ??= new List<ValidationContext>();

		public SchemaRegistry Registry { get; internal set; }
		public JsonSchema SchemaRoot { get; internal set; }
		public JsonPointer SchemaLocation { get; internal set; }
		public JsonSchema LocalSchema { get; internal set; }
		public JsonElement InstanceRoot { get; internal set; }
		public JsonPointer InstanceLocation { get; internal set; }
		public JsonElement LocalInstance { get; internal set; }
		public Uri CurrentUri { get; internal set; }
		internal ValidationContext ParentContext { get; set; }

		public bool HasNestedContexts => _nestedContexts != null && _nestedContexts.Count != 0;
		internal bool HasSiblingContexts => _siblingContexts != null && _siblingContexts.Count != 0;

		internal ValidationContext() { }

		public static ValidationContext From(ValidationContext source,
		                                     JsonPointer? instanceLocation = null,
		                                     JsonElement? instance = null,
		                                     JsonPointer? subschemaLocation = null,
		                                     Uri newUri = null)
		{
			return new ValidationContext
				{
					Registry = source.Registry,
					InstanceRoot = source.InstanceRoot,
					SchemaRoot = source.SchemaRoot,
					SchemaLocation = subschemaLocation ?? source.SchemaLocation,
					LocalSchema = source.LocalSchema,
					InstanceLocation = instanceLocation ?? source.InstanceLocation,
					LocalInstance = instance?.Clone() ?? source.LocalInstance.Clone(),
					CurrentUri = newUri ?? source.CurrentUri
				};
		}

		internal void ImportAnnotations(ValidationContext context)
		{
			_annotations = context?._annotations;
		}

		internal void ConsolidateAnnotations()
		{
			if (!HasNestedContexts) return;
			foreach (var consolidationAction in _consolidationActions)
			{
				consolidationAction(NestedContexts, this);
			}
		}

		internal object TryGetAnnotation(string key)
		{
			if (_annotations == null) return null;
			return _annotations.TryGetValue(key, out var annotation) ? annotation : null;
		}

		public static void RegisterConsolidationMethod(ContextConsolidator consolidateAnnotations)
		{
			_consolidationActions.Add(consolidateAnnotations);
		}
	}
}