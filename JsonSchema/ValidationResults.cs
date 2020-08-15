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

		public bool IsValid { get; }
		public string Message { get; }
		public JsonPointer SchemaLocation { get; }
		public JsonPointer InstanceLocation { get; }
		public IReadOnlyCollection<ValidationResults> NestedResults { get; }
		public Uri AbsoluteSchemaLocation => _absoluteUri ??= _BuildAbsoluteUri();

		internal ValidationResults(ValidationContext context)
		{
			// TODO: capture annotations from context (only if valid)
			IsValid = context.IsValid;
			Message = context.Message;
			SchemaLocation = context.SchemaLocation;
			_currentUri = context.CurrentUri;
			InstanceLocation = context.InstanceLocation;
			if (context.HasNestedContexts)
				NestedResults = context.NestedContexts.Select(c => new ValidationResults(c)).ToList();
		}

		private Uri _BuildAbsoluteUri()
		{
			if (_currentUri == null) return null;
			if (SchemaLocation.Segments.All(s => s.Value != "$ref")) return null;

			throw new NotImplementedException();
		}
	}
}