using System;
using System.Linq;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Models a templated path URI.
/// </summary>
public partial class PathTemplate : IEquatable<PathTemplate>, IEquatable<string>
{
	// /path/{item} and /path/{otherItem} are equal
	// /path/{item} and /{path}/item are not equal, but are ambiguous

	[GeneratedRegex(@"^\{.*\}$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
	private static partial Regex TemplatedSegmentPattern();
	private static readonly Regex _templatedSegmentPattern = TemplatedSegmentPattern();

	/// <summary>
	/// Gets the segments of the path.
	/// </summary>
	public string[] Segments { get; }

	private PathTemplate(string[] segments)
	{
		Segments = segments;
	}

	/// <summary>
	/// Parses a new path template from a string.
	/// </summary>
	/// <param name="source">The string source</param>
	/// <returns>A path template.</returns>
	public static PathTemplate Parse(string source)
	{
		var asPointer = JsonPointer.Parse(source);

		return new PathTemplate(GetSegments(asPointer));
	}

	private static string[] GetSegments(JsonPointer pointer)
	{
		var segments = new string[pointer.SegmentCount];
		for (var i = 0; i < pointer.SegmentCount; i++)
		{
			segments[i] = pointer[i].ToString();
		}

		return segments;
	}

	/// <summary>
	/// Attempts to parse a new path template from a string.
	/// </summary>
	/// <param name="source">The string source</param>
	/// <param name="template">The path template if the parse succeeded, otherwise null</param>
	/// <returns>True if the parse succeeded, otherwise false.</returns>
	public static bool TryParse(string source, out PathTemplate template)
	{
		var asPointer = JsonPointer.Parse(source);

		template = new PathTemplate(GetSegments(asPointer));

		return true;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString()
	{
		return string.Concat(Segments.Select(x => $"/{x}"));
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>
	/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
	public bool Equals(PathTemplate? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;

		if (Segments.Length != other.Segments.Length) return false;

		var zipped = Segments.Zip(other.Segments, (x,y) => (First: x, Second: y));

		return zipped.All(x => x.First == x.Second ||
		                       _templatedSegmentPattern.IsMatch(x.First) && _templatedSegmentPattern.IsMatch(x.Second));
	}

	/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>
	/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
	public bool Equals(string? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (!TryParse(other, out var otherTemplate)) return false;
		return Equals(otherTemplate);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>
	/// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
	public override bool Equals(object? obj)
	{
		return Equals(obj as PathTemplate);
	}

	/// <summary>Serves as the default hash function.</summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		int result = 0;
		unchecked
		{
			foreach (var segment in Segments)
			{
				if (_templatedSegmentPattern.IsMatch(segment)) result += 1;

				result += segment.GetHashCode();
			}
		}
		return result;
	}

	/// <summary>
	/// Implicitly converts a string to a path template via parsing.
	/// </summary>
	/// <param name="source">The string source.</param>
	public static implicit operator PathTemplate(string source)
	{
		return Parse(source);
	}
}