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
		/// Adds a message to indicate a keyword has begun processing.
		/// </summary>
		/// <param name="log">The log</param>
		/// <param name="keyword">The keyword name</param>
		public static void EnterKeyword(this ILog log, string keyword)
		{
			log.Write(() => $"Processing {keyword}...");
		}

		/// <summary>
		/// Adds a message to indicate a keyword has finished processing.
		/// </summary>
		/// <param name="log">The log</param>
		/// <param name="keyword">The keyword name</param>
		/// <param name="valid">Whether the validation was successful</param>
		public static void ExitKeyword(this ILog log, string keyword, bool valid)
		{
			log.Write(() => $"{keyword} complete: {(valid ? "valid" : "invalid")}");
		}

		/// <summary>
		/// Adds a message to indicate that a keyword doesn't apply and why.
		/// </summary>
		/// <param name="log">The log</param>
		/// <param name="kind">The value kind</param>
		public static void WrongValueKind(this ILog log, JsonValueKind kind)
		{
			log.Write(() => $"Value type is {kind}. Not applicable.");
		}

		/// <summary>
		/// Adds a message to indicate that a keyword doesn't apply and why.
		/// </summary>
		/// <param name="log">The log</param>
		/// <param name="reason">The reason</param>
		public static void NotApplicable(this ILog log, Func<string> reason)
		{
			log.Write(() => $"{reason()} Not applicable.");
		}

		/// <summary>
		/// Gets the valid or invalid strings.
		/// </summary>
		/// <param name="isValid">The validity.</param>
		/// <returns>"valid" for `true`; "invalid" for `false`.</returns>
		public static string Validity(this bool isValid)
		{
			return isValid ? "valid" : "invalid";
		}
	}
}