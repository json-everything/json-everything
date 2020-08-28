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

		private Dictionary<string, Annotation> _annotations;
		private List<ValidationContext> _nestedContexts;
		private List<ValidationContext> _siblingContexts;

		public bool IsValid { get; set; }
		public bool Ignore { get; set; }
		public string Message { get; set; }
		public IReadOnlyCollection<Annotation> Annotations => (_annotations ??= new Dictionary<string, Annotation>()).Values;
		public List<ValidationContext> NestedContexts => _nestedContexts ??= new List<ValidationContext>();
		internal List<ValidationContext> SiblingContexts => _siblingContexts ??= new List<ValidationContext>();

		public ValidationOptions Options { get; internal set; }

		public JsonSchema SchemaRoot { get; internal set; }
		public JsonPointer SchemaLocation { get; internal set; }
		public JsonSchema LocalSchema { get; internal set; }
		public JsonElement InstanceRoot { get; internal set; }
		public JsonPointer InstanceLocation { get; internal set; }
		public JsonElement LocalInstance { get; internal set; }
		public Uri CurrentUri { get; internal set; }
		public JsonSchema CurrentAnchor { get; internal set; }
		internal ValidationContext ParentContext { get; set; }
		internal bool RequiredInResult { get; set; }

		public bool ApplyOptimizations => Options.OutputFormat == OutputFormat.Flag;
		public bool HasNestedContexts => _nestedContexts != null && _nestedContexts.Count != 0;
		internal bool HasSiblingContexts => _siblingContexts != null && _siblingContexts.Count != 0;

		internal ValidationContext() { }

		public static ValidationContext From(ValidationContext source,
		                                     in JsonPointer? instanceLocation = null,
		                                     in JsonElement? instance = null,
		                                     in JsonPointer? subschemaLocation = null,
		                                     Uri newUri = null)
		{
			return new ValidationContext
				{
					Options = source.Options,
					InstanceRoot = source.InstanceRoot,
					SchemaRoot = source.SchemaRoot,
					SchemaLocation = subschemaLocation ?? source.SchemaLocation,
					LocalSchema = source.LocalSchema,
					InstanceLocation = instanceLocation ?? source.InstanceLocation,
					LocalInstance = instance?.Clone() ?? source.LocalInstance.Clone(),
					CurrentAnchor = source.CurrentAnchor,
					CurrentUri = newUri ?? source.CurrentUri
				};
		}

		internal void ImportAnnotations(ValidationContext context)
		{
			_annotations = context?._annotations;
		}

		public void ConsolidateAnnotations()
		{
			if (!HasNestedContexts) return;
			foreach (var consolidationAction in _consolidationActions)
			{
				consolidationAction(NestedContexts, this);
			}
		}

		public void SetAnnotation(string owner, object value)
		{
			_annotations ??= new Dictionary<string, Annotation>();
			_annotations[owner] = new Annotation(owner, value, SchemaLocation);
		}

		public object TryGetAnnotation(string key)
		{
			if (_annotations == null) return null;
			return _annotations.TryGetValue(key, out var annotation) ? annotation.Value : null;
		}

		public static void RegisterConsolidationMethod(ContextConsolidator consolidateAnnotations)
		{
			_consolidationActions.Add(consolidateAnnotations);
		}
	}

	public class Annotation
	{
		public string Owner { get; }
		public object Value { get; }
		public JsonPointer Source { get; }

		public Annotation(string owner, object value, in JsonPointer source)
		{
			Owner = owner;
			Value = value;
			Source = source;
		}
	}
}