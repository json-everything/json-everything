using System.Collections.Generic;
using System.Linq;

namespace Json.Schema
{
	public class ValidationResults
	{
		public static ValidationResults Null { get; } = new ValidationResults(true, null, new ValidationResults[0], null);

		public IEnumerable<ValidationResults> Annotations { get; }
		public IEnumerable<ValidationResults> Errors { get; }
		public string Message { get; }
		public bool IsValid { get; }

		private ValidationResults(bool isValid, string message, ValidationResults[] annotations, ValidationResults[] errors)
		{
			IsValid = isValid;
			Message = message;
			Annotations = annotations;
			Errors = errors;
		}

		public static ValidationResults Success(string message, params ValidationResults[] annotations)
		{
			return new ValidationResults(true, message, annotations, null);
		}

		public static ValidationResults Success(params ValidationResults[] annotations)
		{
			return annotations.Length == 0 ? Null : new ValidationResults(true, null, annotations, null);
		}

		public static ValidationResults Success(string message, IEnumerable<ValidationResults> annotations)
		{
			return new ValidationResults(true, message, annotations.ToArray(), null);
		}

		public static ValidationResults Success(IEnumerable<ValidationResults> annotations)
		{
			var array = annotations.ToArray();
			return array.Length == 0 ? Null : new ValidationResults(true, null, array, null);
		}

		public static ValidationResults Fail(string message, params ValidationResults[] errors)
		{
			return new ValidationResults(false, message, null, errors);
		}

		public static ValidationResults Fail(params ValidationResults[] errors)
		{
			return new ValidationResults(false, null, null, errors);
		}

		public static ValidationResults Fail(string message, IEnumerable<ValidationResults> errors)
		{
			return new ValidationResults(false, message, null, errors.ToArray());
		}

		public static ValidationResults Fail(IEnumerable<ValidationResults> errors)
		{
			return new ValidationResults(false, null, null, errors.ToArray());
		}
	}
}