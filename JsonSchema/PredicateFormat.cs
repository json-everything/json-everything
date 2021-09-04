using System;
using System.Text.Json;

namespace Json.Schema
{
	/// <summary>
	/// A <see cref="Format"/> that uses a predicate for evaluation.
	/// </summary>
	public class PredicateFormat : Format
	{
		/// <summary>
		/// A predicate that also supplies an `out` error message parameter.
		/// </summary>
		/// <param name="element">The element to validate.</param>
		/// <param name="errorMessage">An error message.</param>
		/// <returns><code>true</code>.  Override to return another value.</returns>
		public delegate bool PredicateWithErrorMessage(JsonElement element, out string? errorMessage);

		private readonly Func<JsonElement, bool>? _predicate;
		private readonly PredicateWithErrorMessage? _predicateWithErrorMessage;

		/// <summary>
		/// Creates a new <see cref="PredicateFormat"/>.
		/// </summary>
		/// <param name="key">The format key.</param>
		/// <param name="predicate">The predicate.</param>
		public PredicateFormat(string key, Func<JsonElement, bool> predicate)
			: base(key)
		{
			_predicate = predicate;
		}

		/// <summary>
		/// Creates a new <see cref="PredicateFormat"/>.
		/// </summary>
		/// <param name="key">The format key.</param>
		/// <param name="predicate">The predicate.</param>
		public PredicateFormat(string key, PredicateWithErrorMessage predicate)
			: base(key)
		{
			_predicateWithErrorMessage = predicate;
		}

		/// <summary>
		/// Validates an instance against a format.
		/// </summary>
		/// <param name="element">The element to validate.</param>
		/// <returns>The result of the predicate.</returns>
		public override bool Validate(JsonElement element)
		{
			return Validate(element, out _);
		}

		/// <summary>
		/// Validates an instance against a format and provides an error message.
		/// </summary>
		/// <param name="element">The element to validate.</param>
		/// <param name="errorMessage"></param>
		/// <returns><code>true</code>.  Override to return another value.</returns>
		public override bool Validate(JsonElement element, out string? errorMessage)
		{
			if (_predicateWithErrorMessage != null)
				return _predicateWithErrorMessage(element, out errorMessage);

			errorMessage = null;
			return _predicate!(element);
		}
	}
}