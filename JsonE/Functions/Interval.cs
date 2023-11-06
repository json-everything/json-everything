using System;

namespace Json.JsonE.Functions;

internal struct Interval
{
	public int? Years { get; private set; }
	public int? Months { get; private set; }
	public int? Weeks { get; private set; }
	public int? Days { get; private set; }
	public int? Hours { get; private set; }
	public int? Minutes { get; private set; }
	public int? Seconds { get; private set; }

	public void Set(string field, int value)
	{
		switch (field.ToLowerInvariant())
		{
			case "year":
			case "years":
				if (Years.HasValue)
					throw new InterpreterException("Cannot set days more than once");

				Years = value;
				break;
			case "mo":
			case "month":
			case "months":
				if (Months.HasValue)
					throw new InterpreterException("Cannot set days more than once");

				Months = value;
				break;
			case "week":
			case "weeks":
				if (Weeks.HasValue)
					throw new InterpreterException("Cannot set days more than once");

				Weeks = value;
				break;
			case "day":
			case "days":
				if (Days.HasValue)
					throw new InterpreterException("Cannot set days more than once");

				Days = value;
				break;
			case "hour":
			case "hours":
				if (Hours.HasValue)
					throw new InterpreterException("Cannot set hours more than once");

				Hours = value;
				break;
			case "minute":
			case "minutes":
				if (Minutes.HasValue)
					throw new InterpreterException("Cannot set minutes more than once");

				Minutes = value;
				break;
			case "second":
			case "seconds":
				if (Seconds.HasValue)
					throw new InterpreterException("Cannot set seconds more than once");

				Seconds = value;
				break;
		}
	}

	private readonly TimeSpan ToTimeSpan()
	{
		return new TimeSpan(Days ?? 0, Hours ?? 0, Minutes ?? 0, Seconds ?? 0);
	}

	public static TimeSpan ParseAndGetTimeSpan(string source)
	{
		var parts = source.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length % 2 != 0)
			throw new InterpreterException("Argument is invalid");

		var interval = new Interval();
		for (int i = 0; i < parts.Length; i += 2)
		{
			if (!int.TryParse(parts[i], out var value))
				throw new InterpreterException("Argument is invalid");

			interval.Set(parts[i + 1], value);
		}

		return interval.ToTimeSpan();
	}
}