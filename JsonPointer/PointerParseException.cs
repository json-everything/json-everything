using System;

namespace Json.Pointer
{
	public class PointerParseException : Exception
	{
		public PointerParseException() { }

		public PointerParseException(string message)
			: base(message) { }

		public PointerParseException(string message, Exception inner)
			: base(message, inner) { }
	}
}