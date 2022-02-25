using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// The results object for validations.
	/// </summary>
	[JsonConverter(typeof(ValidationResultsJsonConverter))]
	public class ValidationResults
	{
		/// <summary>
		/// Consolidates annotations from multiple child results onto a single parent result.
		/// Generally, a keyword will define how it handles its own consolidation.  This action
		/// must be registered on startup.
		/// </summary>
		/// <param name="results">The validation results.</param>
		public delegate void ContextConsolidator(ValidationResults results);

		private static readonly List<ContextConsolidator> _consolidationActions = new List<ContextConsolidator>();

		private Uri _currentUri;
		private Uri? _absoluteUri;
		private JsonPointer? _reference;
		private List<Annotation>? _annotations;
		private List<ValidationResults>? _nestedResults;
		private bool _isConsolidating;

		/// <summary>
		/// Indicates whether the validation passed or failed.
		/// </summary>
		public bool IsValid { get; private set; }
		/// <summary>
		/// The error message, if any.
		/// </summary>
		public string? Message { get; private set; }
		/// <summary>
		/// The schema location that generated this node.
		/// </summary>
		public JsonPointer SchemaLocation { get; private set; }
		/// <summary>
		/// The instance location that was processed.
		/// </summary>
		public JsonPointer InstanceLocation { get; private set; }

		/// <summary>
		/// The absolute schema location.  Only available if the schema had an absolute URI ID.
		/// </summary>
		public Uri? AbsoluteSchemaLocation => _absoluteUri ??= BuildAbsoluteUri();

		/// <summary>
		/// The collection of nested results.
		/// </summary>
		public IReadOnlyList<ValidationResults> NestedResults => _nestedResults ??= new List<ValidationResults>();
	
		/// <summary>
		/// Gets whether there are nested results.
		/// </summary>
		/// <remarks>
		/// Because <see cref="NestedResults"/> is lazily loaded, this property allows the check without
		/// the side effect of allocating a list object.
		/// </remarks>
		public bool HasNestedResults => _nestedResults is not { Count: 0 };

		/// <summary>
		/// The collection of annotations from this node.
		/// </summary>
		public IEnumerable<Annotation> Annotations => _annotations ??= new List<Annotation>();

		/// <summary>
		/// Gets whether there are annotation.
		/// </summary>
		/// <remarks>
		/// Because <see cref="Annotations"/> is lazily loaded, this property allows the check without
		/// the side effect of allocating a list object.
		/// </remarks>
		public bool HasAnnotations => _annotations is not { Count: 0 };

		/// <summary>
		/// Gets the parent result.
		/// </summary>
		public ValidationResults? Parent { get; private set; }

		internal bool Exclude { get; private set; }

		private bool Keep => (Message != null && !IsValid) || Annotations.Any() || NestedResults.Any(r => r.Keep);

		internal ValidationResults(ValidationContext context)
		{
			SchemaLocation = context.SchemaLocation;
			_currentUri = context.CurrentUri;
			InstanceLocation = context.InstanceLocation;
			_reference = context.Reference;
		}

		/// <summary>
		/// Transforms the results to the `details` format.
		/// </summary>
		public void ToDetailed()
		{
			if (!Annotations.Any() && Message == null && NestedResults.Count == 0) return;
			if (NestedResults.Count == 1)
			{
				NestedResults[0].ToDetailed();
				CopyFrom(NestedResults[0]);
				return;
			}

			var condensed = new List<ValidationResults>();
			foreach (var result in NestedResults)
			{
				result.ToDetailed();
				if (result.Keep && result.IsValid == IsValid)
					condensed.Add(result);
			}

			_nestedResults?.Clear();

			if (condensed.Count == 1)
				CopyFrom(condensed[0]);
			else
			{
				_nestedResults ??= new List<ValidationResults>();
				_nestedResults.AddRange(condensed);
			}
		}

		/// <summary>
		/// Transforms the results to the `basic` format.
		/// </summary>
		public void ToBasic()
		{
			ToDetailed();
			var children = GetAllChildren().ToList();
			if (!children.Any()) return;

			children.Remove(this);
			children.ForEach(r => r._annotations?.Clear());
			Prep(ref _nestedResults);
			_nestedResults!.AddRange(children.Where(c => c.Keep));
		}

		/// <summary>
		/// Transforms the results to the `flag` format.
		/// </summary>
		public void ToFlag()
		{
			_nestedResults?.Clear();
			_annotations?.Clear();
		}

		/// <summary>
		/// Invokes all consolidation actions.  Should be called at the end of processing an applicator keyword.
		/// </summary>
		public void ConsolidateAnnotations()
		{
			if (!HasNestedResults) return;
			foreach (var consolidationAction in _consolidationActions)
			{
				_isConsolidating = true;
				consolidationAction(this);
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
			AddAnnotation(new Annotation(owner, value, SchemaLocation) {WasConsolidated = _isConsolidating});
		}

		/// <summary>
		/// Registers a consolidation action.
		/// </summary>
		/// <param name="consolidateAnnotations">The action.</param>
		public static void RegisterConsolidationMethod(ContextConsolidator consolidateAnnotations)
		{
			_consolidationActions.Add(consolidateAnnotations);
		}

		/// <summary>
		/// Tries to get an annotation.
		/// </summary>
		/// <param name="key">The annotation key.</param>
		/// <returns>The annotation or null.</returns>
		public object? TryGetAnnotation(string key)
		{
			if (!HasAnnotations) return null;
			return Annotations.LastOrDefault(x => x.Owner == key)?.Value;
		}

		/// <summary>
		/// Gets all annotations of a particular data type for the current validation level.
		/// </summary>
		/// <typeparam name="T">The data type.</typeparam>
		/// <param name="key">The key under which the annotation is stored.  Typically a keyword.</param>
		/// <returns>The set of all annotations for the current validation level.</returns>
		public IEnumerable<T> GetAllAnnotations<T>(string key)
		{
			if (!HasAnnotations) return Enumerable.Empty<T>();
			return Annotations.Where(x => x.Owner == key && x.Value is T)
				.Select(x => (T) x.Value);
		}

		/// <summary>
		/// Marks the result as valid.
		/// </summary>
		public void Pass()
		{
			IsValid = true;
		}

		/// <summary>
		/// Marks the result as invalid.
		/// </summary>
		/// <param name="message"></param>
		public void Fail(string? message = null)
		{
			IsValid = false;
			Message = message;
		}

		internal void ImportAnnotations(List<Annotation> annotations)
		{
			if (annotations.Count == 0) return;
			AddAnnotations(annotations);
		}

		internal void ConsiderAnnotations(IEnumerable<Annotation> annotations)
		{
			_annotations ??= new List<Annotation>();
			_annotations.AddRange(annotations.Select(Annotation.CreateConsolidated));
		}

		private void AddAnnotation(Annotation annotation)
		{
			_annotations ??= new List<Annotation>();
			_annotations.Add(annotation);
		}

		private void AddAnnotations(IEnumerable<Annotation> annotations)
		{
			_annotations ??= new List<Annotation>();
			_annotations.AddRange(annotations);
		}

		private void CopyFrom(ValidationResults other)
		{
			//IsValid = other.IsValid;
			_annotations = other._annotations;
			Message = other.Message;
			SchemaLocation = other.SchemaLocation;
			_currentUri = other._currentUri;
			InstanceLocation = other.InstanceLocation;
			_nestedResults = other._nestedResults;
			_absoluteUri = other._absoluteUri;
			_reference = other._reference;
		}

		internal void AddNestedResult(ValidationResults results)
		{
			_nestedResults ??= new List<ValidationResults>();
			_nestedResults.Add(results);
			results.Parent = this;
		}

		internal void Ignore()
		{
			IsValid = true;
			Exclude = true;
		}

		internal Uri? BuildAbsoluteUri(JsonPointer pointer)
		{
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (_currentUri == null || !_currentUri.IsAbsoluteUri) return null;
			if (pointer.Segments.All(s => s.Value != RefKeyword.Name &&
			                              s.Value != RecursiveRefKeyword.Name))
				return null;

			var lastIndexOfRef = pointer.Segments
				.Select((s, i) => (s, i))
				.Last(s => s.s.Value == RefKeyword.Name || s.s.Value == RecursiveRefKeyword.Name).i;
			var absoluteSegments = pointer.Segments.Skip(lastIndexOfRef + 1);

			if (_reference != null) 
				absoluteSegments = _reference.Segments.Concat(absoluteSegments);

			return new Uri(_currentUri, JsonPointer.Create(absoluteSegments, true).ToString());
		}

		private Uri? BuildAbsoluteUri()
		{
			return BuildAbsoluteUri(SchemaLocation);
		}

		private IEnumerable<ValidationResults> GetAllChildren()
		{
			var all = new List<ValidationResults>();
			if (Annotations.Any() || Message != null) all.Add(this);
			all.AddRange(NestedResults.SelectMany(r => r.GetAllChildren()));

			_nestedResults?.Clear();

			return all;
		}

		private static void Prep<T>(ref List<T>? list)
		{
			if (list != null) list.Clear();
			else list = new List<T>();
		}
	}

	internal class ValidationResultsJsonConverter : JsonConverter<ValidationResults>
	{
		public override ValidationResults Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, ValidationResults value, JsonSerializerOptions options)
		{
			if (value.Exclude) return;

			writer.WriteStartObject();

			writer.WriteBoolean("valid", value.IsValid);

			writer.WritePropertyName("keywordLocation");
			JsonSerializer.Serialize(writer, value.SchemaLocation);

			if (value.AbsoluteSchemaLocation != null)
			{
				writer.WritePropertyName("absoluteKeywordLocation");
				JsonSerializer.Serialize(writer, value.AbsoluteSchemaLocation);
			}

			writer.WritePropertyName("instanceLocation");
			JsonSerializer.Serialize(writer, value.InstanceLocation);

			if (!value.IsValid)
			{
				if (value.Message != null)
					writer.WriteString("error", value.Message);

				if (value.NestedResults.Any())
				{
					writer.WritePropertyName("errors");
					JsonSerializer.Serialize(writer, value.NestedResults);
				}
			}
			else if (value.Annotations.Any(a => !a.WasConsolidated) || value.NestedResults.Any())
			{
				writer.WritePropertyName("annotations");
				writer.WriteStartArray();

				var annotations = value.Annotations.Where(a => !a.WasConsolidated).ToList();

				foreach (var result in value.NestedResults)
				{
					var annotation = annotations.SingleOrDefault(a => a.Source.Equals(result.SchemaLocation));
					if (annotation != null)
					{
						annotations.Remove(annotation);

						WriteAnnotation(writer, value, annotation);
					}
					else
					{
						JsonSerializer.Serialize(writer, result);
					}
				}

				foreach (var annotation in annotations)
				{
					WriteAnnotation(writer, value, annotation);
				}

				writer.WriteEndArray();
			}

			writer.WriteEndObject();
		}

		private static void WriteAnnotation(Utf8JsonWriter writer, ValidationResults value, Annotation annotation)
		{
			writer.WriteStartObject();

			writer.WriteBoolean("valid", value.IsValid);

			writer.WritePropertyName("keywordLocation");
			JsonSerializer.Serialize(writer, annotation.Source);

			if (value.AbsoluteSchemaLocation != null)
			{
				writer.WritePropertyName("absoluteKeywordLocation");
				JsonSerializer.Serialize(writer, value.BuildAbsoluteUri(annotation.Source));
			}

			writer.WritePropertyName("instanceLocation");
			JsonSerializer.Serialize(writer, value.InstanceLocation);

			writer.WritePropertyName("annotation");
			JsonSerializer.Serialize(writer, annotation.Value);

			writer.WriteEndObject();
		}
	}
}