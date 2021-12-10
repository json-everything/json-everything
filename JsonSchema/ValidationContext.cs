using System;
using System.Collections.Generic;
using System.Linq;
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

		private List<ValidationContext>? _nestedContexts;
		private List<ValidationContext>? _siblingContexts;
		private Dictionary<string, JsonSchema>? _dynamicAnchors;
		private JsonSchema? _currentAnchorBackup;
		private bool _isConsolidating;
		private HashSet<string>? _navigatedReferences;

		private readonly Stack<Uri> _currentUris = new Stack<Uri>();
		private readonly Stack<JsonElement> _localInstances = new Stack<JsonElement>();
		private readonly Stack<JsonPointer> _instanceLocations = new Stack<JsonPointer>();
		private readonly Stack<JsonSchema> _localSchemas = new Stack<JsonSchema>();
		private readonly Stack<JsonPointer> _schemaLocations = new Stack<JsonPointer>();
		private readonly Stack<ValidationResults> _localResults = new Stack<ValidationResults>();
		private readonly Stack<bool> _dynamicScopeFlags = new Stack<bool>();
		private readonly Stack<JsonSchema> _schemaRoots = new Stack<JsonSchema>();

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
		public ValidationOptions Options { get; }

		/// <summary>
		/// The root schema.
		/// </summary>
		public JsonSchema SchemaRoot => _schemaRoots.Peek();

		/// <summary>
		/// The current subschema location relative to the schema root.
		/// </summary>
		public JsonPointer SchemaLocation => _schemaLocations.Peek();

		/// <summary>
		/// The current subschema.
		/// </summary>
		public JsonSchema LocalSchema => _localSchemas.Peek();
		/// <summary>
		/// The instance root.
		/// </summary>
		public JsonElement InstanceRoot { get; }

		/// <summary>
		/// The current instance location relative to the instance root.
		/// </summary>
		public JsonPointer InstanceLocation => _instanceLocations.Peek();

		/// <summary>
		/// The current instance.
		/// </summary>
		public JsonElement LocalInstance => _localInstances.Peek();

		/// <summary>
		/// The current URI, based on `$id` and `$anchor` keywords present in the schema.
		/// </summary>
		public Uri CurrentUri => _currentUris.Peek();
		/// <summary>
		/// The current URI anchor.
		/// </summary>
		public JsonSchema? CurrentAnchor { get; internal set; }
		/// <summary>
		/// (Obsolete) Get the set of defined dynamic anchors.
		/// </summary>
		[Obsolete("This is no longer used. Dynamic anchors are tracked with the registry now.")]
		public Dictionary<string, JsonSchema> DynamicAnchors => _dynamicAnchors ??= new Dictionary<string, JsonSchema>();

		public ValidationResults LocalResult => _localResults.Peek();

		internal bool UriChanged { get; set; }
		//internal ValidationContext ParentContext { get; set; }
		internal JsonPointer? Reference { get; set; }
		internal IReadOnlyDictionary<Uri, bool>? MetaSchemaVocabs { get; set; }
		internal bool IsNewDynamicScope { get; set; }
		internal HashSet<string> NavigatedReferences => _navigatedReferences ??= new HashSet<string>();
		internal bool NavigatedByDirectRef { get; set; }

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

#pragma warning disable 8618
		internal ValidationContext(ValidationOptions options,
			Uri currentUri,
			JsonElement instanceRoot,
			JsonSchema schemaRoot)
		{
			Options = options;
			_currentUris.Push(currentUri);
			var instanceClone = instanceRoot.Clone();
			InstanceRoot = instanceClone;
			_localInstances.Push(instanceClone);
			_schemaRoots.Push(schemaRoot);
			_localSchemas.Push(schemaRoot);
			_localResults.Push(new ValidationResults(this));
			_dynamicScopeFlags.Push(IsNewDynamicScope);
		}
#pragma warning restore 8618

		///// <summary>
		///// Creates a new context from an existing one.  Use this for subschema validations.
		///// </summary>
		///// <param name="source">The source context.</param>
		///// <param name="instanceLocation">(optional) Updates the instance location.</param>
		///// <param name="instance">(optional) Updates the instance location.</param>
		///// <param name="subschemaLocation">(optional) Updates the subschema location.</param>
		///// <param name="newUri">(optional) Updates the current URI.</param>
		///// <returns></returns>
		//public static ValidationContext From(ValidationContext source,
		//                                     in JsonPointer? instanceLocation = null,
		//                                     in JsonElement? instance = null,
		//                                     in JsonPointer? subschemaLocation = null,
		//                                     Uri? newUri = null)
		//{
		//	return new ValidationContext(source.Options)
		//	{
		//		InstanceRoot = source.InstanceRoot,
		//		SchemaRoot = source.SchemaRoot,
		//		SchemaLocation = subschemaLocation ?? source.SchemaLocation,
		//		LocalSchema = source.LocalSchema,
		//		InstanceLocation = instanceLocation ?? source.InstanceLocation,
		//		LocalInstance = instance?.Clone() ?? source.LocalInstance.Clone(),
		//		_currentAnchorBackup = source.CurrentAnchor,
		//		CurrentAnchor = source.CurrentAnchor,
		//		CurrentUri = newUri ?? source.CurrentUri,
		//		Reference = source.Reference,
		//		UriChanged = source.UriChanged || source.CurrentUri != newUri,
		//		_navigatedReferences = source._navigatedReferences == null || instance != null
		//			? null
		//			: new HashSet<string>(source._navigatedReferences),
		//		MetaSchemaVocabs = source.MetaSchemaVocabs
		//	};
		//}

		public void Push(in JsonPointer? instanceLocation = null,
			in JsonElement? instance = null,
			in JsonPointer? subschemaLocation = null,
			in JsonSchema? subschema = null,
			Uri? newUri = null)
		{
			_currentUris.Push(newUri ?? CurrentUri);
			_instanceLocations.Push(instanceLocation ?? InstanceLocation);
			_localInstances.Push(instance ?? LocalInstance);
			_schemaLocations.Push(subschemaLocation ?? SchemaLocation);
			_localSchemas.Push(subschema ?? LocalSchema);
			var newResult = new ValidationResults(this);
			LocalResult.AddNestedResult(newResult);
			_localResults.Push(newResult);
		}

		public void Pop()
		{
			_currentUris.Pop();
			_instanceLocations.Pop();
			_localInstances.Pop();
			_schemaLocations.Pop();
			_localSchemas.Pop();
			_localResults.Pop();
			IsNewDynamicScope = _dynamicScopeFlags.Pop();
		}

		public void AddResult(ValidationResults results)
		{
			LocalResult.AddNestedResult(results);
		}

		internal void ImportAnnotations(List<Annotation> annotations)
		{
			if (annotations.Count == 0) return;
			LocalResult.AddAnnotations(annotations);
		}

		internal void ValidateAnchor()
		{
			CurrentAnchor = _currentAnchorBackup;
		}

		internal void PushSchemaRoot(JsonSchema schema)
		{
			_schemaRoots.Push(schema);
		}

		internal void PopSchemaRoot()
		{
			_schemaRoots.Pop();
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
			LocalResult.AddAnnotation(new Annotation(owner, value, SchemaLocation) {WasConsolidated = _isConsolidating});
		}

		/// <summary>
		/// Tries to get an annotation.
		/// </summary>
		/// <param name="key">The annotation key.</param>
		/// <returns>The annotation or null.</returns>
		public object? TryGetAnnotation(string key)
		{
			if (!LocalResult.HasAnnotations) return null;
			return LocalResult.Annotations.FirstOrDefault(x => x.Owner == key)?.Value;
		}

		/// <summary>
		/// Registers a consolidation action.
		/// </summary>
		/// <param name="consolidateAnnotations">The action.</param>
		public static void RegisterConsolidationMethod(ContextConsolidator consolidateAnnotations)
		{
			_consolidationActions.Add(consolidateAnnotations);
		}

		internal IEnumerable<Type>? GetKeywordsToProcess()
		{
			return MetaSchemaVocabs?.Keys
				.SelectMany(x => Options.VocabularyRegistry.Get(x)?.Keywords ??
				                 Enumerable.Empty<Type>());
		}
	}
}