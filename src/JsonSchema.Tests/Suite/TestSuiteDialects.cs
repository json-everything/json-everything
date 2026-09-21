using System;

namespace Json.Schema.Tests.Suite;

/// <summary>
/// Maps the unified test suite's release tokens onto the dialects this library supports and evaluates
/// the `compatibility` constraints described in the suite's validation README.
/// </summary>
public static class TestSuiteDialects
{
	public enum Release
	{
		Unknown,
		Draft3,
		Draft4,
		Draft5,
		Draft6,
		Draft7,
		Draft2019,
		Draft2020,
		V1
	}

	/// <summary>
	/// The dialects under test, keyed by the release number the suite uses to identify them.
	/// Draft 3 and draft 4 are not supported by this library, so they're absent and any test case
	/// restricted to them is skipped.
	/// </summary>
	public static readonly (string Name, Release Release, Dialect Dialect)[] Supported =
	[
		("draft6", Release.Draft6, Dialect.Draft06),
		("draft7", Release.Draft7, Dialect.Draft07),
		("draft2019-09", Release.Draft2019, Dialect.Draft201909),
		("draft2020-12", Release.Draft2020, Dialect.Draft202012),
		("v1", Release.V1, Dialect.V1)
	];

	/// <summary>
	/// Determines whether a test case's `compatibility` value includes the release under test.
	/// </summary>
	/// <param name="compatibility">The `compatibility` value, or null when the case applies to every dialect.</param>
	/// <param name="releaseUnderTest">The release number of the dialect being tested.</param>
	/// <remarks>
	/// This follows the algorithm given in the suite's validation README: each constraint is evaluated in
	/// order, and evaluation stops at the first constraint whose release is at or above the release under
	/// test. The constraints are ordered smallest to largest, so the surviving result describes the range
	/// that the release under test falls into.
	/// </remarks>
	public static bool IsCompatible(string? compatibility, Release releaseUnderTest)
	{
		if (string.IsNullOrWhiteSpace(compatibility)) return true;

		var isValid = true;
		foreach (var constraint in compatibility!.Split(','))
		{
			var (op, release) = Parse(constraint);

			isValid = op switch
			{
				"" => releaseUnderTest >= release,
				"<=" => releaseUnderTest <= release,
				"=" => releaseUnderTest == release,
				_ => throw new ArgumentOutOfRangeException(nameof(compatibility), $"Unrecognized compatibility operator '{op}'")
			};

			if (releaseUnderTest <= release) break;
		}

		return isValid;
	}

	private static (string Operator, Release Release) Parse(string constraint)
	{
		constraint = constraint.Trim();

		if (constraint.StartsWith("<=", StringComparison.Ordinal))
			return ("<=", FromString(constraint.Substring(2)));
		if (constraint.StartsWith("=", StringComparison.Ordinal))
			return ("=", FromString(constraint.Substring(1)));

		return ("", FromString(constraint));
	}

	private static Release FromString(string identifier) =>
		identifier switch
		{
			"3" => Release.Draft3,
			"4" => Release.Draft4,
			"5" => Release.Draft5,
			"6" => Release.Draft6,
			"7" => Release.Draft7,
			"2019" => Release.Draft2019,
			"2020" => Release.Draft2020,
			"9999" => Release.V1,
			_ => default
		};
}
