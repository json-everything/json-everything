using System.Text.Json;

namespace Json.Logic
{
	/// <summary>
	/// Calls <see cref="Rule.Apply(JsonElement)"/> with no data.
	/// </summary>
	public static class RuleExtensions
	{
		private static readonly JsonElement _nullElement;

		static RuleExtensions()
		{
			using var doc = JsonDocument.Parse("null");
			_nullElement = doc.RootElement.Clone();
		}

		/// <summary>
		/// Calls <see cref="Rule.Apply(JsonElement)"/> with no data.
		/// </summary>
		/// <param name="rule">The rule.</param>
		/// <returns>The result.</returns>
		public static JsonElement Apply(this Rule rule)
		{
			return rule.Apply(_nullElement);
		}
	}
}