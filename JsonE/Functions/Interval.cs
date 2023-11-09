using System;
using System.Text.RegularExpressions;

namespace Json.JsonE.Functions;

internal struct Interval
{
	private static readonly Regex _format =
		new(@"^((?<neg>-)|\+)?\s*((?<year>\d+)\s*(y|yr|years?))?\s*((?<month>\d+)\s*(mo|months?))?\s*((?<week>\d+)\s*(w|wk|weeks?))?\s*((?<day>\d+)\s*(d|days?))?\s*((?<hour>\d+)\s*(h|hr|hours?))?\s*((?<minute>\d+)\s*(m|min|minutes?))?\s*((?<second>\d+)\s*(s|sec|seconds?))?$",
		RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

	public static Interval Zero = new();

	public bool IsNegative { get; private set; }
	public int Years { get; private set; }
	public int Months { get; private set; }
	public int Weeks { get; private set; }
	public int Days { get; private set; }
	public int Hours { get; private set; }
	public int Minutes { get; private set; }
	public int Seconds { get; private set; }

	public static Interval Parse(string source)
	{
		if (string.IsNullOrWhiteSpace(source)) return Zero;

		var match = _format.Match(source);
		if (!match.Success)
			throw new TemplateException("$fromNow expects a string");

		var interval = new Interval
		{
			IsNegative = match.Groups["neg"].Captures.Count != 0,
			Years = match.Groups["year"].Success ? int.Parse(match.Groups["year"].Value) : 0,
			Months = match.Groups["month"].Success ? int.Parse(match.Groups["month"].Value) : 0,
			Weeks = match.Groups["week"].Success ? int.Parse(match.Groups["week"].Value) : 0,
			Days = match.Groups["day"].Success ? int.Parse(match.Groups["day"].Value) : 0,
			Hours = match.Groups["hour"].Success ? int.Parse(match.Groups["hour"].Value) : 0,
			Minutes = match.Groups["minute"].Success ? int.Parse(match.Groups["minute"].Value) : 0,
			Seconds = match.Groups["second"].Success ? int.Parse(match.Groups["second"].Value) : 0
		};

		return interval;
	}

	private void Set(string field, int value)
	{
		switch (field.ToLowerInvariant())
		{
			case "year":
				Years = value;
				break;
			case "month":
				Months = value;
				break;
			case "week":
				Weeks = value;
				break;
			case "day":
				Days = value;
				break;
			case "hour":
				Hours = value;
				break;
			case "minute":
				Minutes = value;
				break;
			case "second":
				Seconds = value;
				break;
		}
	}

	public DateTime AddTo(DateTime basis)
	{
		var totalDays = Years * GetYearLength(basis.Year) +
		                Months * 30 + // See https://github.com/json-e/json-e/issues/487
						Weeks * 7 + 
		                Days;

		var timeSpan = new TimeSpan(totalDays, Hours, Minutes, Seconds);
		if (IsNegative) timeSpan = -timeSpan;

		return basis.Add(timeSpan);
	}

	private static int GetYearLength(int year) => year % 4 == 0 && year % 400 != 0 ? 366 : 365;
}