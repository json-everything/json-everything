using System;
using System.Text.Json;

namespace Json.Pointer
{
	public readonly struct RelativeJsonPointer
	{
		public static readonly RelativeJsonPointer Null = new RelativeJsonPointer(0, JsonPointer.Empty);

		public bool IsIndexQuery { get; }
		public uint ParentSteps { get; }
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

		public static RelativeJsonPointer IndexQuery(uint parentSteps) => 
			new RelativeJsonPointer(parentSteps);
		
		public static RelativeJsonPointer FromPointer(uint parentSteps, JsonPointer pointer) =>
			new RelativeJsonPointer(parentSteps, pointer);

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

		public JsonElement Evaluate(JsonElement element)
		{
			throw new NotImplementedException("Waiting for System.Text.Json to support upward navigation.  See https://github.com/dotnet/runtime/issues/40452");
		}
	}
}