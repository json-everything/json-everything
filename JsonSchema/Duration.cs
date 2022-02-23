using System;

namespace Json.Schema
{
	/// <summary>
	/// Represents an ISO 8601 ABNF duration value.
	/// </summary>
	public readonly struct Duration
	{
		/// <summary>
		/// The number of years.
		/// </summary>
		public uint Years { get; }
		/// <summary>
		/// The number of months.
		/// </summary>
		public uint Months { get; }
		/// <summary>
		/// The number of weeks.  Incompatible with <see cref="Years"/>, <see cref="Months"/>, and <see cref="Days"/>.
		/// </summary>
		public uint Weeks { get; }
		/// <summary>
		/// The number of days.
		/// </summary>
		public uint Days { get; }
		/// <summary>
		/// The number of hours.
		/// </summary>
		public uint Hours { get; }
		/// <summary>
		/// The number of minutes.
		/// </summary>
		public uint Minutes { get; }
		/// <summary>
		/// The number of seconds.
		/// </summary>
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

		/// <summary>
		/// Parses a <see cref="Duration"/> from a string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <returns>A duration.</returns>
		/// <exception cref="ArgumentException"><paramref name="source"/> does not contains a valid duration string.</exception>
		public static Duration Parse(string source)
		{
			if (TryParse(source, out var duration)) return duration;

			throw new ArgumentException("Source string does not contain a valid duration", nameof(source));
		}

		/// <summary>
		/// Parses a <see cref="Duration"/> from a string.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <param name="duration">The resulting duration.</param>
		/// <returns><code>true</code> if the parse was successful; <code>false</code> otherwise.</returns>
		public static bool TryParse(string source, out Duration duration)
		{
			duration = default;
			source = source.Trim();
			if (string.IsNullOrWhiteSpace(source)) return false;

			// ReSharper disable once InlineOutVariableDeclaration
			uint year = 0, month = 0, week, day = 0, hour = 0, minute = 0, second = 0;
			var index = 0;
			if (!Require(source, ref index, 'P')) return false;
			var gotDaily = TryGetComponent(source, ref index, out week, 'W');
			if (!gotDaily)
			{
				gotDaily = TryGetComponent(source, ref index, out year, 'Y') |
				           TryGetComponent(source, ref index, out month, 'M') |
				           TryGetComponent(source, ref index, out day, 'D');
			}

			if (!Require(source, ref index, 'T'))
			{
				if (!gotDaily) return false;
				if (index != source.Length) return false;
			}
			else
			{
				var gotTime = TryGetComponent(source, ref index, out hour, 'H') |
				              TryGetComponent(source, ref index, out minute, 'M') |
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