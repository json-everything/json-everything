using System;

namespace JsonPath
{
	public readonly struct IndexOrRange
	{
		public Range Range { get; }
		public Index Index { get; }
		public bool IsRange { get; }

		public IndexOrRange(Range range)
		{
			Range = range;
			Index = 0;
			IsRange = true;
		}
		public IndexOrRange(Index index)
		{
			Range = default;
			Index = index;
			IsRange = false;
		}

		public static implicit operator IndexOrRange(Range range)
		{
			return new IndexOrRange(range);
		}

		public static implicit operator IndexOrRange(Index index)
		{
			return new IndexOrRange(index);
		}

		public static implicit operator IndexOrRange(int index)
		{
			return new IndexOrRange(index);
		}

		public static implicit operator IndexOrRange(short index)
		{
			return new IndexOrRange(index);
		}
	}
}