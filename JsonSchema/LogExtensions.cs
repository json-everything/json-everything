using System;
using System.Text.Json;

namespace Json.Schema
{
	/// <summary>
	/// Provides common log messages.
	/// </summary>
	public static class LogExtensions
	{
		/// <summary>
		/// Logs a message.
		/// </summary>
		/// <param name="context">The validation context.</param>
		/// <param name="message">The message.</param>
		public static void Log(this ValidationContext context, Func<string> message)
		{
			context.Options.Log.Write(message, context.Options.LogIndentLevel);
		}

		/// <summary>
		/// Adds a message to indicate a keyword has begun processing.  Increments indention.
		/// </summary>
		/// <param name="context">The validation context.</param>
		/// <param name="keyword">The keyword name</param>
		public static void EnterKeyword(this ValidationContext context, string keyword)
		{
			context.Options.LogIndentLevel++;
			context.Log(() => $"Processing {keyword}...");
		}

		/// <summary>
		/// Adds a message to indicate a keyword has finished processing.  Decrements indention.
		/// </summary>
		/// <param name="context">The validation context.</param>
		/// <param name="keyword">The keyword name</param>
		public static void ExitKeyword(this ValidationContext context, string keyword)
		{
			context.Log(() => $"{keyword} complete: {(context.LocalResult.IsValid ? "valid" : "invalid")}");
			context.Options.LogIndentLevel--;
		}

		/// <summary>
		/// Adds a message to indicate a keyword has finished processing.  Decrements indention.
		/// </summary>
		/// <param name="context">The validation context.</param>
		/// <param name="keyword">The keyword name</param>
		/// <param name="valid">Whether the validation was successful</param>
		public static void ExitKeyword(this ValidationContext context, string keyword, bool valid)
		{
			context.Log(() => $"{keyword} complete: {(valid ? "valid" : "invalid")}");
			context.Options.LogIndentLevel--;
		}

		/// <summary>
		/// Adds a message to indicate that a keyword doesn't apply and why.  Decrements indention.
		/// </summary>
		/// <param name="context">The validation context.</param>
		/// <param name="kind">The value kind</param>
		public static void WrongValueKind(this ValidationContext context, JsonValueKind kind)
		{
			context.Log(() => $"Value type is {kind}. Not applicable.");
			context.Options.LogIndentLevel--;
		}

		/// <summary>
		/// Adds a message to indicate that a keyword doesn't apply and why.  Decrements indention.
		/// </summary>
		/// <param name="context">The validation context.</param>
		/// <param name="reason">The reason</param>
		public static void NotApplicable(this ValidationContext context, Func<string> reason)
		{
			context.Log(() => $"{reason()} Not applicable.");
			context.Options.LogIndentLevel--;
		}

		/// <summary>
		/// Gets the valid or invalid strings.
		/// </summary>
		/// <param name="isValid">The validity.</param>
		/// <returns>"valid" for `true`; "invalid" for `false`.</returns>
		public static string GetValidityString(this bool isValid)
		{
			return isValid ? "valid" : "invalid";
		}
	}
}