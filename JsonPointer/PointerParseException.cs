using System;

namespace Json.Pointer
{
	/// <summary>
	/// Thrown during parsing when the source string contains invalid JSON Pointer data.
	/// </summary>
	public class PointerParseException : Exception
	{
		/// <summary>
		/// Creates a <see cref="PointerParseException"/>.
		/// </summary>
		public PointerParseException() { }

		/// <summary>
		/// Creates a <see cref="PointerParseException"/>.
		/// </summary>
		public PointerParseException(string message)
			: base(message) { }

		/// <summary>
		/// Creates a <see cref="PointerParseException"/>.
		/// </summary>
		public PointerParseException(string message, Exception inner)
			: base(message, inner) { }
	}
}