using System.Text.Json;

namespace Json.More
{
	/// <summary>
	/// Provides extension functionality for <see cref="JsonDocument"/>.
	/// </summary>
	public static class JsonDocumentExtensions
	{
		/// <summary>
		/// Determines JSON-compatible equivalence.
		/// </summary>
		/// <param name="a">The first document.</param>
		/// <param name="b">The second document.</param>
		/// <returns><code>true</code> if the documents are equivalent; <code>false</code> otherwise.</returns>
		public static bool IsEquivalentTo(this JsonDocument a, JsonDocument b)
		{
			return a.RootElement.IsEquivalentTo(b.RootElement);
		}
	}
}