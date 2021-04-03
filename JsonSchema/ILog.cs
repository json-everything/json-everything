using System;

namespace Json.Schema
{
	/// <summary>
	/// Used to log processing details.
	/// </summary>
	public interface ILog
	{
		/// <summary>
		/// Gets or sets the indent level.
		/// </summary>
		/// <remarks>
		/// Implementations are not required to make use of this, but it can be useful to human log readers.
		/// </remarks>
		int Indent { get; set; }

		/// <summary>
		/// Logs a message with a newline.
		/// </summary>
		/// <param name="message">The message.</param>
		void Write(Func<string> message);
	}
}