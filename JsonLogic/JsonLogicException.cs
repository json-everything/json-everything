using System;

namespace Json.Logic
{
	/// <summary>
	/// Thrown when a rule cannot be processed or deserialized.
	/// </summary>
	public class JsonLogicException : Exception
	{
		/// <summary>
		/// Creates a new instance of the <see cref="JsonLogicException"/> class.
		/// </summary>
		/// <param name="message">The exception message.</param>
		public JsonLogicException(string message)
			: base(message)
		{
		}
	}
}