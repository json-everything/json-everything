using System;

namespace Json.Pointer
{
	public class PointerEvaluationException : Exception
	{
		public PointerEvaluationException() { }

		public PointerEvaluationException(string message)
			: base(message) { }

		public PointerEvaluationException(string message, Exception inner)
			: base(message, inner) { }
	}
}