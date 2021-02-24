using System.Text.Json;
using Json.More;

namespace Json.Schema
{
	internal static class JsonElementExtensions
	{
		public static int GetConsistentHashCode(this JsonElement element)
		{
			unchecked
			{
				var hashCode = element.ValueKind.GetHashCode();
				hashCode = (hashCode * 397) ^ element.ToJsonString().GetHashCode();
				// I'd love to do this, but the .GetRawText() method returns the string with whitespace intact.
				// This means that an object or array value can vary even if there is a difference in
				// whitespace between inner tokens, e.g. [ 1, 2, 3 ] vs [1,2,3].
				//hashCode = (hashCode * 397) ^ element.GetRawText().GetHashCode();
				return hashCode;
			}
		}

	}
}