using System;

namespace Json.Schema
{
	public readonly struct Duration
	{
		public uint Years { get; }
		public uint Months { get; }
		public uint Weeks { get; }
		public uint Days { get; }
		public uint Hours { get; }
		public uint Minutes { get; }
		public uint Seconds { get; }

		private Duration(uint years, uint months, uint weeks, uint days, uint hours, uint minutes, uint seconds)
		{
			Years = years;
			Months = months;
			Weeks = weeks;
			Days = days;
			Hours = hours;
			Minutes = minutes;
			Seconds = seconds;
		}

		public static Duration Parse(string source)
		{
			if (TryParse(source, out var duration)) return duration;

			throw new ArgumentException("Source string does not contain a valid duration", nameof(source));
		}

		public static bool TryParse(string source, out Duration duration)
		{
			duration = default;
			source = source.Trim();
			if (string.IsNullOrWhiteSpace(source)) return false;

			uint year = 0, month = 0, week = 0, day = 0, hour = 0, minute = 0, second = 0;
			var index = 0;
			if (!Require(source, ref index, 'P')) return false;
			var gotDaily = TryGetComponent(source, ref index, out week, 'W');
			if (!gotDaily)
			{
				gotDaily = TryGetComponent(source, ref index, out year, 'Y') ||
				           TryGetComponent(source, ref index, out month, 'M') ||
				           TryGetComponent(source, ref index, out day, 'D');
			}

			if (!Require(source, ref index, 'T'))
			{
				if (!gotDaily) return false;
				if (index != source.Length) return false;
			}
			else
			{
				var gotTime = TryGetComponent(source, ref index, out hour, 'H') ||
				              TryGetComponent(source, ref index, out minute, 'M') ||
				              TryGetComponent(source, ref index, out second, 'S');

				if (!gotTime) return false;
			}

			duration = new Duration(year, month, week, day, hour, minute, second);
			return true;
		}

		private static bool Require(string source, ref int index, char ch)
		{
			if (index < source.Length && source[index] == ch)
			{
				index++;
				return true;
			}

			return false;
		}

		private static bool TryGetComponent(string source, ref int index, out uint number, char ch)
		{
			number = 0;
			var tempIndex = index;
			while (tempIndex < source.Length && '0' <= source[tempIndex] && source[tempIndex] <= '9')
			{
				number = number * 10 + (uint)(source[tempIndex] - '0');
				tempIndex++;
			}

			if (tempIndex < source.Length && source[tempIndex] == ch)
			{
				index = tempIndex + 1;
				return true;
			}

			number = 0;
			return false;
		}
	}
}