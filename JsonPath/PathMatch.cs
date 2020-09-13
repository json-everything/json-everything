using System.Text.Json;
using Json.Pointer;

namespace Json.Path
{
	public class PathMatch
	{
		public JsonElement Value { get; }
		public JsonPointer Location { get; }

		public PathMatch(in JsonElement value, in JsonPointer location)
		{
			Value = value;
			Location = location;
		}
	}
}