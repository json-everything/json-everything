using System;
using System.Text.Json;

namespace Json.Pointer
{
	/// <summary>
	/// Represents a Relative JSON Pointer IAW draft-handrews-relative-json-pointer-02
	/// </summary>
	public readonly struct RelativeJsonPointer
	{
		/// <summary>
		/// The null pointer.  Indicates no navigation should occur.
		/// </summary>
		public static readonly RelativeJsonPointer Null = new RelativeJsonPointer(0, JsonPointer.Empty);

		/// <summary>
		/// Gets whether the pointer is an index query, which returns the index within the parent rather than the value.
		/// </summary>
		public bool IsIndexQuery { get; }
		/// <summary>
		/// Gets the number of parent (root) steps to take.
		/// </summary>
		public uint ParentSteps { get; }
		/// <summary>
		/// Gets the pointer to follow after taking <see cref="ParentSteps"/> steps upward.
		/// </summary>
		public JsonPointer Pointer { get; }

		private RelativeJsonPointer(uint parentSteps)
		{
			IsIndexQuery = true;
			ParentSteps = parentSteps;
			Pointer = JsonPointer.Empty;
		}
		private RelativeJsonPointer(uint parentSteps, JsonPointer pointer)
		{
			IsIndexQuery = false;
			ParentSteps = parentSteps;
			Pointer = pointer;
		}

		/// <summary>
		/// Creates an index query pointer.
		/// </summary>
		/// <param name="parentSteps"></param>
		/// <returns>A Relative JSON Pointer.</returns>
		public static RelativeJsonPointer IndexQuery(uint parentSteps) => 
			new RelativeJsonPointer(parentSteps);

		/// <summary>
		/// Creates a Relative JSON Pointer from a JSON Pointer and a number of parent steps.
		/// </summary>
		/// <param name="parentSteps">The number of parent steps.</param>
		/// <param name="pointer">The JSON Pointer.</param>
		/// <returns>A Relative JSON Pointer.</returns>
		public static RelativeJsonPointer FromPointer(uint parentSteps, JsonPointer pointer) =>
			new RelativeJsonPointer(parentSteps, pointer);

		/// <summary>
		/// Parses a JSON Pointer segment from a string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <returns>A Relative JSON Pointer.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
		/// <exception cref="PointerParseException"><paramref name="source"/> does not contain a valid relative pointer.</exception>
		public static RelativeJsonPointer Parse(string source)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException($"`{nameof(source)}` contains no data");

			var span = source.AsSpan();
			int i = 0;
			while (i < span.Length && char.IsDigit(span[i])) i++;

			if (i == 0) throw new PointerParseException($"`{nameof(source)}` must start with a non-negative integer");

			var number = uint.Parse(span.Slice(0, i).ToString());
			if (i == span.Length) return new RelativeJsonPointer(number, JsonPointer.Empty);
			if (span[i] == '#')
			{
				if (i+1 < span.Length) throw new PointerParseException($"{nameof(source)} cannot contain data after a `#`");
				return new RelativeJsonPointer(number);
			}

			if (span[i] != '/') throw new PointerParseException($"{nameof(source)} must contain either a `#` or a pointer after the initial number");

			var pointer = JsonPointer.Parse(span.Slice(i).ToString());

			return new RelativeJsonPointer(number, pointer);
		}

		/// <summary>
		/// Parses a JSON Pointer from a string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="relativePointer">The resulting relative pointer.</param>
		/// <returns><code>true</code> if the parse was successful; <code>false</code> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
		public static bool TryParse(string source, out RelativeJsonPointer relativePointer)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (string.IsNullOrWhiteSpace(source))
			{
				relativePointer = default;
				return false;
			}

			var span = source.AsSpan();
			int i = 0;
			while (i < span.Length && char.IsDigit(span[i])) i++;

			if (i == 0)
			{
				relativePointer = default;
				return false;
			}

			var number = uint.Parse(span.Slice(0, i).ToString());
			if (i == span.Length)
			{
				relativePointer = new RelativeJsonPointer(number, JsonPointer.Empty);
				return true;
			}
			if (span[i] == '#')
			{
				if (i + 1 < span.Length)
				{
					relativePointer = default;
					return false;
				}
				relativePointer = new RelativeJsonPointer(number);
				return true;
			}

			if (span[i] != '/')
			{
				relativePointer = default;
				return false;
			}

			if (!JsonPointer.TryParse(span.Slice(i).ToString(), out var pointer))
			{
				relativePointer = default;
				return false;
			}

			relativePointer = new RelativeJsonPointer(number, pointer);
			return true;
		}

		/// <summary>
		/// Evaluates the relative pointer over a <see cref="JsonElement"/>.
		/// </summary>
		/// <param name="element">The <see cref="JsonElement"/>.</param>
		/// <returns>The sub-element at the relative pointer's location, or null if the path does not exist.</returns>
		/// <exception cref="NotSupportedException">This method is not yet supported.  Waiting for System.Text.Json to support upward navigation.  See https://github.com/dotnet/runtime/issues/40452</exception>
		[Obsolete("Waiting for System.Text.Json to support upward navigation.  See https://github.com/dotnet/runtime/issues/40452")]
		public JsonElement Evaluate(JsonElement element)
		{
			throw new NotSupportedException("Waiting for System.Text.Json to support upward navigation.  See https://github.com/dotnet/runtime/issues/40452");
		}
	}
}