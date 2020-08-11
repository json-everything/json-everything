using System;
using System.Collections.Generic;
using System.Linq;
using Json.Pointer;

namespace Json.Schema
{
	public class ValidationResults
	{
		private readonly Uri _currentUri;
		private Uri _absoluteUri;
		private IReadOnlyCollection<ValidationResults> _nestedResults;

		public bool IsValid { get; }
		public string Message { get; }
		public JsonPointer SchemaLocation { get; }
		public JsonPointer InstanceLocation { get; }
		public IReadOnlyCollection<ValidationResults> NestedResults => _nestedResults ??= new List<ValidationResults>();
		public Uri AbsoluteSchemaLocation => _absoluteUri ??= _BuildAbsoluteUri();

		private ValidationResults(ValidationContext context, bool isValid, string message)
		{
			// TODO: capture annotations from context (only if valid)
			IsValid = isValid;
			Message = message;
			SchemaLocation = context?.SchemaLocation ?? JsonPointer.Empty;
			_currentUri = context?.CurrentUri;
			InstanceLocation = context?.InstanceLocation ?? JsonPointer.Empty;
		}

		public static ValidationResults Success(ValidationContext context)
		{
			return new ValidationResults(context, true, null);
		}

		public static ValidationResults Fail(ValidationContext context)
		{
			return new ValidationResults(context, false, null);
		}

		public static ValidationResults Fail(ValidationContext context, string message)
		{
			return new ValidationResults(context, false, message);
		}

		private Uri _BuildAbsoluteUri()
		{
			if (_currentUri == null) return null;
			if (SchemaLocation.Segments.All(s => s.Value != "$ref")) return null;

			throw new NotImplementedException();
		}

		public void AddNestedResults(params ValidationResults[] nested)
		{
			_nestedResults = nested ?? throw new ArgumentNullException(nameof(nested));
		}

		public void AddNestedResults(IEnumerable<ValidationResults> nested)
		{
			if (nested == null) throw new ArgumentNullException(nameof(nested));

			_nestedResults = nested as IReadOnlyCollection<ValidationResults> ?? nested.ToArray();
		}
	}
}