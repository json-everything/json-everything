using System.Text.Json;
using Json.Pointer;

namespace Json.Path
{
	/// <summary>
	/// Represents a single match.
	/// </summary>
	public class PathMatch
	{
		/// <summary>
		/// The value at the matching location.
		/// </summary>
		public JsonElement Value { get; }
		/// <summary>
		/// The location where the value was found.
		/// </summary>
		public JsonPointer Location { get; }

		internal PathMatch(in JsonElement value, in JsonPointer location)
		{
			Value = value;
			Location = location;
		}
	}
}