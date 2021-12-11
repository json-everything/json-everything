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
		private JsonSchema? _currentAnchorBackup;

		private readonly Stack<Uri> _currentUris = new Stack<Uri>();
		private readonly Stack<JsonElement> _localInstances = new Stack<JsonElement>();
		private readonly Stack<JsonPointer> _instanceLocations = new Stack<JsonPointer>();
		private readonly Stack<JsonSchema> _localSchemas = new Stack<JsonSchema>();
		private readonly Stack<JsonPointer> _schemaLocations = new Stack<JsonPointer>();
		private readonly Stack<ValidationResults> _localResults = new Stack<ValidationResults>();
		private readonly Stack<bool> _dynamicScopeFlags = new Stack<bool>();

		/// <summary>
		/// The option set for the validation.
		/// </summary>
		public ValidationOptions Options { get; }

		/// <summary>
		/// The root schema.
		/// </summary>
		public JsonSchema SchemaRoot { get; set; }

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

		public ValidationResults LocalResult => _localResults.Peek();

		internal bool UriChanged { get; set; }
		//internal ValidationContext ParentContext { get; set; }
		internal JsonPointer? Reference { get; set; }
		internal IReadOnlyDictionary<Uri, bool>? MetaSchemaVocabs { get; set; }
		internal bool IsNewDynamicScope => _dynamicScopeFlags.Peek();
		internal HashSet<(string, JsonPointer)> NavigatedReferences { get; } = new HashSet<(string, JsonPointer)>();
		internal bool NavigatedByDirectRef { get; set; }

		/// <summary>
		/// Whether processing optimizations can be applied (output format = flag).
		/// </summary>
		public bool ApplyOptimizations => Options.OutputFormat == OutputFormat.Flag;

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
			SchemaRoot = schemaRoot;
			_localInstances.Push(instanceClone);
			_instanceLocations.Push(JsonPointer.Empty);
			_localSchemas.Push(schemaRoot);
			_schemaLocations.Push(JsonPointer.Empty);
			_localResults.Push(new ValidationResults(this));
			_dynamicScopeFlags.Push(true);
		}
#pragma warning restore 8618

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
			_dynamicScopeFlags.Push(false);
		}

		public void Pop()
		{
			_currentUris.Pop();
			_instanceLocations.Pop();
			_localInstances.Pop();
			_schemaLocations.Pop();
			_localSchemas.Pop();
			_localResults.Pop();
			_dynamicScopeFlags.Pop();
		}

		internal void UpdateCurrentUri(Uri newUri)
		{
			_currentUris.Pop();
			_currentUris.Pop();
			_currentUris.Push(newUri);
			_currentUris.Push(newUri);
			_dynamicScopeFlags.Pop();
			_dynamicScopeFlags.Push(true);
		}

		internal void ValidateAnchor()
		{
			CurrentAnchor = _currentAnchorBackup;
		}

		internal IEnumerable<Type>? GetKeywordsToProcess()
		{
			return MetaSchemaVocabs?.Keys
				.SelectMany(x => Options.VocabularyRegistry.Get(x)?.Keywords ??
				                 Enumerable.Empty<Type>());
		}
	}
}