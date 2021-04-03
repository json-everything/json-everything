using System;

namespace Json.Schema
{
	/// <summary>
	/// Used to log processing details.
	/// </summary>
	public interface ILog
	{
		/// <summary>
		/// Logs a message with a newline.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="indent"></param>
		void Write(Func<string> message, int indent = 0);
	}
}