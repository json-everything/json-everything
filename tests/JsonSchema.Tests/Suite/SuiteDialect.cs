using System;
using System.Linq;

namespace Json.Schema.Tests.Suite;

/// <summary>
/// Pairs a json-everything <see cref="Dialect"/> with the release number the test suite
/// uses in its <c>compatibility</c> property.
/// </summary>
public class SuiteDialect
{
	private const string _formatAssertionTag = "format-assertion";
	private const string _formatAnnotationTag = "format-annotation";

	// The suite uses 9999 for features that are in the spec but not yet in a yearly release.
	// Implementations are expected to pass these tests, so v1 is evaluated as that placeholder.
	private const int _unreleasedVersion = 9999;

	public static SuiteDialect Draft06 { get; } = new("draft6", Dialect.Draft06, 6);
	public static SuiteDialect Draft07 { get; } = new("draft7", Dialect.Draft07, 7);
	public static SuiteDialect Draft201909 { get; } = new("draft2019-09", Dialect.Draft201909, 2019);
	public static SuiteDialect Draft202012 { get; } = new("draft2020-12", Dialect.Draft202012, 2020);
	public static SuiteDialect V1 { get; } = new("v1", Dialect.V1, _unreleasedVersion);

	public string Name { get; }
	public Dialect Dialect { get; }
	public int Version { get; }

	private SuiteDialect(string name, Dialect dialect, int version)
	{
		Name = name;
		Dialect = dialect;
		Version = version;
	}

	/// <summary>
	/// Implements the compatibility matching algorithm from the suite's README.
	/// </summary>
	public bool IsCompatible(string? compatibility)
	{
		if (compatibility is null or "") return true;

		var isValid = true;
		foreach (var constraint in compatibility.Split(','))
		{
			int constraintVersion;
			if (constraint.StartsWith("<=", StringComparison.Ordinal))
			{
				constraintVersion = int.Parse(constraint.Substring(2));
				isValid = Version <= constraintVersion;
			}
			else if (constraint.StartsWith("=", StringComparison.Ordinal))
			{
				constraintVersion = int.Parse(constraint.Substring(1));
				isValid = Version == constraintVersion;
			}
			else
			{
				constraintVersion = int.Parse(constraint);
				isValid = Version >= constraintVersion;
			}

			if (Version <= constraintVersion) break;
		}

		return isValid;
	}

	/// <summary>
	/// Determines whether this dialect can be configured to exhibit the behavior described by the test case's tags.
	/// </summary>
	public bool SupportsTags(TestCollection collection)
	{
		if (collection.Tags is null) return true;

		foreach (var tag in collection.Tags)
		{
			switch (tag)
			{
				case _formatAssertionTag:
					break;
				case _formatAnnotationTag:
					// v1 requires format assertion and cannot be configured to only annotate
					if (Dialect == Dialect.V1) return false;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(collection), $"Unknown test suite tag '{tag}'");
			}
		}

		return true;
	}

	public static bool RequiresFormatValidation(TestCollection collection) =>
		collection.Tags?.Contains(_formatAssertionTag) ?? false;
}
