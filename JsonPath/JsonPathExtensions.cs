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
		if (!path.IsSingular)
			throw new InvalidOperationException("Only a Singular Path can be written as a JSON Pointer");

		return string.Concat(path.Segments.Select(x =>
		{
			var segment = x.Selectors[0] switch
			{
				IndexSelector index => $"/{index}",
				NameSelector name => $"/{PointerEncode(name.Name)}",
				// ReSharper disable once NotResolvedInText
				_ => throw new ArgumentOutOfRangeException("selector", "Selector is not of the right type for conversion to JSON Pointer.  This shouldn't happen.")
			};

			return segment;
		}));
	}

	private static string PointerEncode(string segment)
	{
		return segment.Replace("~", "~0").Replace("/", "~1");
	}
}