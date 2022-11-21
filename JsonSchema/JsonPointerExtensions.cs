using System;
using System.Linq;
using Json.Pointer;

namespace Json.Schema;

public static class JsonPointerExtensions
{
	public static JsonPointer GetSibling(this JsonPointer pointer, PointerSegment newSegment)
	{
		return JsonPointer.Create(pointer.Segments.Take(pointer.Segments.Length - 1).Append(newSegment), false);
	}

	public static Uri Resolve(this JsonPointer pointer, Uri baseUri)
	{
		string pointerFragment;
		var lastRef = pointer.Segments.LastOrDefault(x => x.Value is RefKeyword.Name or DynamicRefKeyword.Name or RecursiveRefKeyword.Name);
		if (lastRef is not null)
		{
			var lastRefIndex = Array.IndexOf(pointer.Segments, lastRef);
			pointerFragment = JsonPointer.Create(pointer.Segments.Skip(lastRefIndex + 1), true).ToString();
		}
		else
			pointerFragment = JsonPointer.Create(pointer.Segments, true).ToString();

		return pointerFragment != "#"
			? new Uri(baseUri, pointerFragment)
			: baseUri;
	}

	public static bool StartsWith(this JsonPointer pointer, JsonPointer compare)
	{
		return pointer.Segments.Length >= compare.Segments.Length &&
		       compare.Segments.Zip(pointer.Segments, (x, y) => x.Value == y.Value).All(x => x);
	}
}