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
		private List<ValidationResults> _nestedResults;
		private List<Annotation> _annotations;
		private Uri _currentUri;
		private Uri? _absoluteUri;
		private JsonPointer? _reference;

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
		public IReadOnlyList<ValidationResults> NestedResults => _nestedResults;
		/// <summary>
		/// The collection of annotations from this node.
		/// </summary>
		public IReadOnlyList<Annotation> Annotations => _annotations;

		private bool Keep => Message != null || Annotations.Any() || NestedResults.Any(r => r.Keep);

		internal ValidationResults(ValidationContext context)
		{
			IsValid = context.IsValid;
			_annotations = context.IsValid
				? context.Annotations.ToList()
				: new List<Annotation>();
			Message = context.Message;
			SchemaLocation = context.SchemaLocation;
			_currentUri = context.CurrentUri!;
			InstanceLocation = context.InstanceLocation;
			_nestedResults = context.HasNestedContexts
				? context.NestedContexts.Select(c => new ValidationResults(c)).ToList()
				: new List<ValidationResults>();
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

			_nestedResults.Clear();

			if (condensed.Count == 1)
				CopyFrom(condensed[0]);
			else
				_nestedResults.AddRange(condensed);
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
			children.ForEach(r => r._annotations.Clear());
			_nestedResults.Clear();
			_nestedResults.AddRange(children.Where(c => c.Keep));
		}

		/// <summary>
		/// Transforms the results to the `flag` format.
		/// </summary>
		public void ToFlag()
		{
			_nestedResults.Clear();
			_annotations.Clear();
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

			_nestedResults.Clear();

			return all;
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