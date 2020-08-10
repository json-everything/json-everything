using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema
{
	public class ValidationResults
	{
		private readonly Uri _currentUri;
		private Uri _absoluteUri;

		public static ValidationResults Null { get; } = new ValidationResults();

		public IEnumerable<ValidationResults> Annotations { get; }
		public IEnumerable<ValidationResults> Errors { get; }
		public string Message { get; }
		public bool IsValid { get; }
		public JsonPointer SchemaLocation { get; }
		public JsonPointer InstanceLocation { get; }
		public JsonElement Value { get; }
		public Uri AbsoluteSchemaLocation => _absoluteUri ??= _BuildAbsoluteUri();

		private Uri _BuildAbsoluteUri()
		{
			if (_currentUri == null) return null;
			if (SchemaLocation.Segments.All(s => s.Value != "$ref")) return null;

			throw new NotImplementedException();
		}

		private ValidationResults()
		{
			IsValid = true;
		}
		private ValidationResults(ValidationContext context, JsonElement value)
		{
			IsValid = true;
			SchemaLocation = context?.SchemaLocation ?? JsonPointer.Empty;
			_currentUri = context?.CurrentUri;
			InstanceLocation = context?.InstanceLocation ?? JsonPointer.Empty; 
			Value = value.Clone();
		}

		private ValidationResults(ValidationContext context, bool isValid, string message, ValidationResults[] annotations, ValidationResults[] errors)
		{
			IsValid = isValid;
			Message = message;
			Annotations = annotations;
			Errors = errors;
			SchemaLocation = context?.SchemaLocation ?? JsonPointer.Empty;
			_currentUri = context?.CurrentUri;
			InstanceLocation = context?.InstanceLocation ?? JsonPointer.Empty;
		}

		public static ValidationResults Annotation(ValidationContext context, JsonElement value)
		{
			return new ValidationResults(context, value);
		}

		public static ValidationResults Success(ValidationContext context, string message, params ValidationResults[] annotations)
		{
			return new ValidationResults(context, true, message, annotations, null);
		}

		public static ValidationResults Success(ValidationContext context, params ValidationResults[] annotations)
		{
			return annotations.Length == 0 ? Null : new ValidationResults(context, true, null, annotations, null);
		}

		public static ValidationResults Success(ValidationContext context, string message, IEnumerable<ValidationResults> annotations)
		{
			return new ValidationResults(context, true, message, annotations.ToArray(), null);
		}

		public static ValidationResults Success(ValidationContext context, IEnumerable<ValidationResults> annotations)
		{
			var array = annotations.ToArray();
			return array.Length == 0 ? Null : new ValidationResults(context, true, null, array, null);
		}

		public static ValidationResults Fail(ValidationContext context, string message, params ValidationResults[] errors)
		{
			return new ValidationResults(context, false, message, null, errors);
		}

		public static ValidationResults Fail(ValidationContext context, params ValidationResults[] errors)
		{
			return new ValidationResults(context, false, null, null, errors);
		}

		public static ValidationResults Fail(ValidationContext context, string message, IEnumerable<ValidationResults> errors)
		{
			return new ValidationResults(context, false, message, null, errors.ToArray());
		}

		public static ValidationResults Fail(ValidationContext context, IEnumerable<ValidationResults> errors)
		{
			return new ValidationResults(context, false, null, null, errors.ToArray());
		}
	}
}