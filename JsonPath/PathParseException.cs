using System;

namespace Json.Path
{
	/// <summary>
	/// Thrown when an error occurs attempting to parse a JSON Path.
	/// </summary>
	public class PathParseException : Exception
	{
		/// <summary>
		/// The index at which the error occurred.
		/// </summary>
		public int Index { get; }

		internal PathParseException(int index, string message)
			: base(message)
		{
			Index = index;
		}
	}
}