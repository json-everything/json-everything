using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Provides a single source of data for validation operations.
	/// </summary>
	public class ValidationContext
	{
		/// <summary>
		/// Consolidates properties from multiple child contexts onto a single parent context.
		/// Generally, a keyword will define how it handles its own consolidation.  This action
		/// must be registered on startup.
		/// </summary>
		/// <param name="sourceContexts">the source (child) contexts.</param>
		/// <param name="destContext">The destination (parent) context.</param>
		public delegate void ContextConsolidator(IList<ValidationContext> sourceContexts, ValidationContext destContext);

		private static readonly List<ContextConsolidator> _consolidationActions = new List<ContextConsolidator>();

		private Dictionary<string, Annotation> _annotations;
		private List<ValidationContext> _nestedContexts;
		private List<ValidationContext> _siblingContexts;
		private bool _isConsolidating;

		/// <summary>
		/// Indicates whether the validation passed or failed.
		/// </summary>
		public bool IsValid { get; set; }
		/// <summary>
		/// Indicates whether this context should be ignored in the validation output.  (e.g. `$defs`)
		/// </summary>
		public bool Ignore { get; set; }
		/// <summary>
		/// Gets or sets error message.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The collection of annotations collected during the validation pass.
		/// </summary>
		public IReadOnlyCollection<Annotation> Annotations => (_annotations ??= new Dictionary<string, Annotation>()).Values;
		/// <summary>
		/// The collection of validation contexts from nested schemas.
		/// </summary>
		/// <remarks>
		/// This property is lazy and will create a value upon first access.  To check
		/// whether there are any nested contexts, use <see cref="HasNestedContexts"/>.
		/// </remarks>
		public List<ValidationContext> NestedContexts => _nestedContexts ??= new List<ValidationContext>();
		/// <summary>
		/// The collection of validation contexts of sibling keywords that have already been processed.
		/// </summary>
		/// <remarks>
		/// This property is lazy and will create a value upon first access.  To check
		/// whether there are any nested contexts, use <see cref="HasSiblingContexts"/>.
		/// </remarks>
		public List<ValidationContext> SiblingContexts => _siblingContexts ??= new List<ValidationContext>();

		/// <summary>
		/// The option set for the validation.
		/// </summary>
		public ValidationOptions Options { get; internal set; }
		/// <summary>
		/// The root schema.
		/// </summary>
		public JsonSchema SchemaRoot { get; internal set; }
		/// <summary>
		/// The current subschema location relative to the schema root.
		/// </summary>
		public JsonPointer SchemaLocation { get; internal set; }
		/// <summary>
		/// The current subschema.
		/// </summary>
		public JsonSchema LocalSchema { get; internal set; }
		/// <summary>
		/// The instance root.
		/// </summary>
		public JsonElement InstanceRoot { get; internal set; }
		/// <summary>
		/// The current instance location relative to the instance root.
		/// </summary>
		public JsonPointer InstanceLocation { get; internal set; }
		/// <summary>
		/// The current instance.
		/// </summary>
		public JsonElement LocalInstance { get; internal set; }
		/// <summary>
		/// The current URI, based on `$id` and `$anchor` keywords present in the schema.
		/// </summary>
		public Uri CurrentUri { get; internal set; }
		/// <summary>
		/// The current URI anchor.
		/// </summary>
		public JsonSchema CurrentAnchor { get; internal set; }

		internal ValidationContext ParentContext { get; set; }
		internal bool RequiredInResult { get; set; }
		internal JsonPointer? Reference { get; set; }

		/// <summary>
		/// Whether processing optimizations can be applied (output format = flag).
		/// </summary>
		public bool ApplyOptimizations => Options.OutputFormat == OutputFormat.Flag;
		/// <summary>
		/// Whether the context has any nested contexts.
		/// </summary>
		public bool HasNestedContexts => _nestedContexts != null && _nestedContexts.Count != 0;
		/// <summary>
		/// Whether the context has any sibling contexts.
		/// </summary>
		public bool HasSiblingContexts => _siblingContexts != null && _siblingContexts.Count != 0;

		internal ValidationContext() { }

		/// <summary>
		/// Creates a new context from an existing one.  Use this for subschema validations.
		/// </summary>
		/// <param name="source">The source context.</param>
		/// <param name="instanceLocation">(optional) Updates the instance location.</param>
		/// <param name="instance">(optional) Updates the instance location.</param>
		/// <param name="subschemaLocation">(optional) Updates the subschema location.</param>
		/// <param name="newUri">(optional) Updates the current URI.</param>
		/// <returns></returns>
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
					CurrentUri = newUri ?? source.CurrentUri,
					Reference = source.Reference
				};
		}

		internal void ImportAnnotations(ValidationContext context)
		{
			_annotations = context?._annotations;
		}

		/// <summary>
		/// Invokes all consolidation actions.  Should be called at the end of processing an applicator keyword.
		/// </summary>
		public void ConsolidateAnnotations()
		{
			if (!HasNestedContexts) return;
			foreach (var consolidationAction in _consolidationActions)
			{
				_isConsolidating = true;
				consolidationAction(NestedContexts, this);
				_isConsolidating = false;
			}
		}

		/// <summary>
		/// Sets an annotation.
		/// </summary>
		/// <param name="owner">The annotation key.  Typically the name of the keyword.</param>
		/// <param name="value">The annotation value.</param>
		public void SetAnnotation(string owner, object value)
		{
			_annotations ??= new Dictionary<string, Annotation>();
			_annotations[owner] = new Annotation(owner, value, SchemaLocation) {WasConsolidated = _isConsolidating};
		}

		/// <summary>
		/// Tries to get an annotation.
		/// </summary>
		/// <param name="key">The annotation key.</param>
		/// <returns>The annotation or null.</returns>
		public object TryGetAnnotation(string key)
		{
			if (_annotations == null) return null;
			return _annotations.TryGetValue(key, out var annotation) ? annotation.Value : null;
		}

		/// <summary>
		/// Registers a consolidation action.
		/// </summary>
		/// <param name="consolidateAnnotations">The action.</param>
		public static void RegisterConsolidationMethod(ContextConsolidator consolidateAnnotations)
		{
			_consolidationActions.Add(consolidateAnnotations);
		}
	}
}