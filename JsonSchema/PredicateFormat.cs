using System;
using System.Text.Json;

namespace Json.Schema
{
	/// <summary>
	/// A <see cref="Format"/> that uses a predicate for evaluation.
	/// </summary>
	public class PredicateFormat : Format
	{
		private readonly Func<JsonElement, bool> _predicate;

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
		/// Validates an instance against a format.
		/// </summary>
		/// <param name="element">The element to validate.</param>
		/// <returns>The result of the predicate.</returns>
		public override bool Validate(JsonElement element)
		{
			return _predicate(element);
		}
	}
}