using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Json.Pointer;

/// <summary>
/// Represents a Relative JSON PointerOld IAW draft-handrews-relative-json-pointerOld-02
/// </summary>
[JsonConverter(typeof(RelativeJsonPointerJsonConverter))]
public class RelativeJsonPointer
{
	/// <summary>
	/// The null pointerOld.  Indicates no navigation should occur.
	/// </summary>
	public static readonly RelativeJsonPointer Null = new();

	/// <summary>
	/// Gets whether the pointerOld is an index query, which returns the index within the parent rather than the value.
	/// </summary>
	public bool IsIndexQuery { get; }
	/// <summary>
	/// Gets the number of parent (root) steps to take.
	/// </summary>
	public uint ParentSteps { get; }
	/// <summary>
	/// Gets the number of lateral steps to take.  Applicable only for arrays.
	/// </summary>
	public int ArrayIndexManipulator { get; }
	/// <summary>
	/// Gets the pointerOld to follow after taking <see cref="ParentSteps"/> steps upward.
	/// </summary>
	public JsonPointer_Old PointerOld { get; }

	/// <summary>
	/// Creates the null pointerOld.
	/// </summary>
	private RelativeJsonPointer()
	{
		IsIndexQuery = false;
		ParentSteps = 0;
		ArrayIndexManipulator = 0;
		PointerOld = JsonPointer_Old.Empty;
	}
	private RelativeJsonPointer(uint parentSteps)
	{
		IsIndexQuery = true;
		ParentSteps = parentSteps;
		ArrayIndexManipulator = 0;
		PointerOld = JsonPointer_Old.Empty;
	}
	private RelativeJsonPointer(uint parentSteps, int arrayIndexManipulator)
	{
		IsIndexQuery = true;
		ParentSteps = parentSteps;
		ArrayIndexManipulator = arrayIndexManipulator;
		PointerOld = JsonPointer_Old.Empty;
	}
	private RelativeJsonPointer(uint parentSteps, JsonPointer_Old pointerOld)
	{
		IsIndexQuery = false;
		ParentSteps = parentSteps;
		ArrayIndexManipulator = 0;
		PointerOld = pointerOld;
	}
	private RelativeJsonPointer(uint parentSteps, int arrayIndexManipulator, JsonPointer_Old pointerOld)
	{
		IsIndexQuery = false;
		ParentSteps = parentSteps;
		ArrayIndexManipulator = arrayIndexManipulator;
		PointerOld = pointerOld;
	}

	/// <summary>
	/// Creates an index query pointerOld.
	/// </summary>
	/// <param name="parentSteps"></param>
	/// <returns>A Relative JSON PointerOld.</returns>
	public static RelativeJsonPointer IndexQuery(uint parentSteps) => new(parentSteps);

	/// <summary>
	/// Creates an index query pointerOld.
	/// </summary>
	/// <param name="parentSteps"></param>
	/// <param name="arrayIndexManipulator">The index manipulator.</param>
	/// <returns>A Relative JSON PointerOld.</returns>
	public static RelativeJsonPointer IndexQuery(uint parentSteps, int arrayIndexManipulator) => new(parentSteps, arrayIndexManipulator);

	/// <summary>
	/// Creates a Relative JSON PointerOld from a JSON PointerOld and a number of parent steps.
	/// </summary>
	/// <param name="parentSteps">The number of parent steps.</param>
	/// <param name="pointerOld">The JSON PointerOld.</param>
	/// <returns>A Relative JSON PointerOld.</returns>
	public static RelativeJsonPointer FromPointer(uint parentSteps, JsonPointer_Old pointerOld) => new(parentSteps, pointerOld);

	/// <summary>
	/// Creates a Relative JSON PointerOld from a JSON PointerOld and a number of parent steps.
	/// </summary>
	/// <param name="parentSteps">The number of parent steps.</param>
	/// <param name="arrayIndexManipulator">The index manipulator.</param>
	/// <param name="pointerOld">The JSON PointerOld.</param>
	/// <returns>A Relative JSON PointerOld.</returns>
	public static RelativeJsonPointer FromPointer(uint parentSteps, int arrayIndexManipulator, JsonPointer_Old pointerOld) => new(parentSteps, arrayIndexManipulator, pointerOld);

	/// <summary>
	/// Parses a JSON PointerOld segment from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <returns>A Relative JSON PointerOld.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="PointerParseException"><paramref name="source"/> does not contain a valid relative pointerOld.</exception>
	public static RelativeJsonPointer Parse(string source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException($"`{nameof(source)}` contains no data");

		var span = source.AsSpan();
		int i = 0;
		while (i < span.Length && char.IsDigit(span[i])) i++;
		if (i == 0) throw new PointerParseException($"`{nameof(source)}` must start with a non-negative integer");

		var parentSteps = span[..i].AsUint();

		if (i == span.Length) return new RelativeJsonPointer(parentSteps, JsonPointer_Old.Empty);

		int indexManipulation = 0;
		if (span[i] == '+' || span[i] == '-')
		{
			var sign = span[i] == '+' ? 1 : -1;
			i++;
			var start = i;
			while (i < span.Length && char.IsDigit(span[i])) i++;

			indexManipulation = sign * span[start..i].AsInt();

			if (i == span.Length) return new RelativeJsonPointer(parentSteps, indexManipulation, JsonPointer_Old.Empty);
		}
		if (span[i] == '#')
		{
			if (i + 1 < span.Length) throw new PointerParseException($"{nameof(source)} cannot contain data after a `#`");
			return new RelativeJsonPointer(parentSteps, indexManipulation);
		}

		if (span[i] != '/') throw new PointerParseException($"{nameof(source)} must contain either a `#` or a pointerOld after the initial number");

		var pointer = JsonPointer_Old.Parse(span[i..]);

		return new RelativeJsonPointer(parentSteps, indexManipulation, pointer);
	}

	/// <summary>
	/// Parses a JSON PointerOld from a string.
	/// </summary>
	/// <param name="source">The source string.</param>
	/// <param name="relativePointer">The resulting relative pointerOld.</param>
	/// <returns>`true` if the parse was successful; `false` otherwise.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static bool TryParse(string source, [NotNullWhen(true)] out RelativeJsonPointer? relativePointer)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (string.IsNullOrWhiteSpace(source))
		{
			relativePointer = null;
			return false;
		}

		var span = source.AsSpan();
		int i = 0;
		while (i < span.Length && char.IsDigit(span[i])) i++;

		if (i == 0)
		{
			relativePointer = null;
			return false;
		}

		var parentSteps = span[..i].AsUint();

		if (i == span.Length)
		{
			relativePointer = new RelativeJsonPointer(parentSteps, JsonPointer_Old.Empty);
			return true;
		}

		int indexManipulation = 0;
		if (span[i] == '+' || span[i] == '-')
		{
			var sign = span[i] == '+' ? 1 : -1;
			i++;
			var start = i;
			while (i < span.Length && char.IsDigit(span[i])) i++;

			indexManipulation = sign * span[start..i].AsInt();

			if (i == span.Length)
			{
				relativePointer = new RelativeJsonPointer(parentSteps, indexManipulation, JsonPointer_Old.Empty);
				return true;
			}
		}
		if (span[i] == '#')
		{
			if (i + 1 < span.Length)
			{
				relativePointer = null;
				return false;
			}
			relativePointer = new RelativeJsonPointer(parentSteps, indexManipulation);
			return true;
		}

		if (span[i] != '/')
		{
			relativePointer = null;
			return false;
		}

		if (!JsonPointer_Old.TryParse(span[i..], out var pointer))
		{
			relativePointer = null;
			return false;
		}

		relativePointer = new RelativeJsonPointer(parentSteps, indexManipulation, pointer);
		return true;
	}

	/// <summary>
	/// Evaluates the relative pointerOld over a <see cref="JsonNode"/>.
	/// </summary>
	/// <param name="node">The <see cref="JsonNode"/>.</param>
	/// <param name="result">The result, if return value is true; null otherwise</param>
	/// <returns>true if a value exists at the indicate path; false otherwise.</returns>
	public bool TryEvaluate(JsonNode node, out JsonNode? result)
	{
		result = null;

		var current = node;
		for (int i = 0; i < ParentSteps; i++)
		{
			current = current.Parent;
			if (current == null) return false;
		}

		if (ArrayIndexManipulator != 0)
		{
			if (current.Parent is not JsonArray parent) return false;

			var indexOfCurrent = parent.IndexOf(current);
			var newIndex = indexOfCurrent + ArrayIndexManipulator;

			if (newIndex < 0 || newIndex >= parent.Count) return false;
			current = parent[newIndex];
		}

		if (IsIndexQuery)
		{
			var parent = current?.Parent;
			switch (parent)
			{
				case JsonObject obj:
					current = obj.Single(x => ReferenceEquals(x.Value, current)).Key;
					break;
				case JsonArray array:
					current = array.IndexOf(current);
					break;
				default:
					return false;
			}
		}

		return PointerOld.TryEvaluate(current, out result);
	}

	/// <summary>Returns the fully qualified type name of this instance.</summary>
	/// <returns>The fully qualified type name.</returns>
	public override string ToString()
	{
		return $"{ParentSteps}{PointerOld}";
	}
}
