using System;
using System.Linq;

namespace Json.Path;

/// <summary>
/// Provides extended functionality for <see cref="JsonPath"/>.
/// </summary>
public static class JsonPathExtensions
{
	/// <summary>
	/// Renders a Singular Path as a JSON Pointer.
	/// </summary>
	/// <param name="path">A JSON Path which is a Singular Path.</param>
	/// <returns>A string containing a JSON Pointer.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the path is not singular.</exception>
	public static string AsJsonPointer(this JsonPath path)
	{
		return string.Concat(path.Segments.Select(x =>
		{
			if (x.Selectors.Length == 1 && !x.IsRecursive)
			{
				var segment = x.Selectors[0] switch
				{
					IndexSelector index => $"/{index}",
					NameSelector name => $"/{PointerEncode(name.Name)}",
					// ReSharper disable once NotResolvedInText
					_ => null
				};
				if (segment != null) return segment;
			}

			if (x.Selectors.Length == 2)
			{
				var index = x.Selectors.OfType<IndexSelector>().FirstOrDefault();
				var name = x.Selectors.OfType<NameSelector>().FirstOrDefault();

				if (index != null && name != null && name.Name == index.Index.ToString())
					return $"/{PointerEncode(name.Name)}";
			}

			throw new InvalidOperationException("This path cannot be represented as a JSON Pointer");
		}));
	}

	private static string PointerEncode(string segment)
	{
		return segment.Replace("~", "~0").Replace("/", "~1");
	}
}