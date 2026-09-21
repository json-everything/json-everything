using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Json.Schema.Keywords;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests.Suite;

/// <summary>
/// Discovers test cases from the unified validation suite. Both validation runners share this
/// discovery so they stay in step; they differ only in the output format they evaluate with.
/// </summary>
public static class ValidationTestLoader
{
	private const string _testCasesPath = @"../../../../../ref-repos/JSON-Schema-Test-Suite/validation/tests";
	private const string _externalTestCasesPath = @"../../../../../../JSON-Schema-Test-Suite/validation/tests";

	// Tests to explicitly ignore: [fileName, collectionDescription, testCaseDescription ("*" for all)]
	private static readonly HashSet<(string, string, string)> _ignoredTests =
	[
		("idn-email", "validation of an internationalized e-mail addresses", "a local part with a lone UTF-16 surrogate is invalid"),
		("hostname", "validation of A-label (punycode) host names", "a valid host name (example.test in Hangul)"),
		("hostname", "validation of A-label (punycode) host names", "Arabic-Indic digits not mixed with Extended Arabic-Indic digits"),
		("hostname", "validation of A-label (punycode) host names", "Exceptions that are PVALID, left-to-right chars"),
		("hostname", "validation of A-label (punycode) host names", "Exceptions that are PVALID, right-to-left chars"),
		("hostname", "validation of A-label (punycode) host names", "Extended Arabic-Indic digits not mixed with Arabic-Indic digits"),
		("hostname", "validation of A-label (punycode) host names", "Greek KERAIA followed by Greek"),
		("hostname", "validation of A-label (punycode) host names", "Hebrew GERESH preceded by Hebrew"),
		("hostname", "validation of A-label (punycode) host names", "Hebrew GERSHAYIM preceded by Hebrew"),
		("hostname", "validation of A-label (punycode) host names", "KATAKANA MIDDLE DOT with Han"),
		("hostname", "validation of A-label (punycode) host names", "KATAKANA MIDDLE DOT with Hiragana"),
		("hostname", "validation of A-label (punycode) host names", "KATAKANA MIDDLE DOT with Katakana"),
		("hostname", "validation of A-label (punycode) host names", "MIDDLE DOT with surrounding 'l's"),
		("hostname", "validation of A-label (punycode) host names", "ZERO WIDTH JOINER preceded by Virama"),
		("hostname", "validation of A-label (punycode) host names", "ZERO WIDTH NON-JOINER not preceded by Virama but matches regexp"),
		("hostname", "validation of A-label (punycode) host names", "ZERO WIDTH NON-JOINER preceded by Virama"),
		("idn-hostname", "validation of internationalized host names", "a name longer than 253 characters is invalid"),
		("idn-hostname", "validation of internationalized host names", "a U-label whose A-label form is longer than 63 octets is invalid"),
		("idn-hostname", "validation of internationalized host names", "Bidi domain name with a digit-first label is invalid"),
		("idn-hostname", "validation of internationalized host names", "contains illegal char U+302E Hangul single dot tone mark"),
		("idn-hostname", "validation of internationalized host names", "Exceptions that are DISALLOWED, left-to-right chars"),
		("idn-hostname", "validation of internationalized host names", "Exceptions that are DISALLOWED, right-to-left chars"),
		("idn-hostname", "validation of internationalized host names", "KATAKANA MIDDLE DOT with no Hiragana, Katakana, or Han"),
		("idn-hostname", "validation of internationalized host names", "KATAKANA MIDDLE DOT with no other characters"),
		("idn-hostname", "validation of internationalized host names", "label starting with a digit before a right-to-left letter is invalid"),
		("idn-hostname", "validation of internationalized host names", "left-to-right label containing a right-to-left letter is invalid"),
		("idn-hostname", "validation of internationalized host names", "right-to-left label mixing both digit types is invalid"),
		("idn-hostname", "validation of internationalized host names", "valid Chinese Punycode"),
		("idn-hostname", "validation of internationalized host names", "zero width non-joiner must pass at every occurrence"),
		("idn-hostname", "validation of separators in internationalized host names", "fullwidth full stop as label separator"),
		("idn-hostname", "validation of separators in internationalized host names", "halfwidth ideographic full stop as label separator"),
		("idn-hostname", "validation of separators in internationalized host names", "ideographic full stop as label separator"),
		("idn-hostname", "validation of separators in internationalized host names", "label too long if separator ignored (fullwidth full stop)"),
		("idn-hostname", "validation of separators in internationalized host names", "label too long if separator ignored (halfwidth ideographic full stop)"),
		("idn-hostname", "validation of separators in internationalized host names", "label too long if separator ignored (ideographic full stop)"),
	];

	private static readonly Dictionary<string, IKeywordHandler> _supportedProposals = new()
	{
		[PropertyDependenciesKeyword.Instance.Name] = PropertyDependenciesKeyword.Instance
	};

	/// <summary>
	/// Enumerates every test in the suite that applies to a supported dialect.
	/// </summary>
	/// <param name="useExternal">Whether to read the suite from a sibling clone rather than the submodule.</param>
	/// <param name="runV1">Whether to include the v1 dialect.</param>
	/// <param name="outputFormat">The output format the runner evaluates with.</param>
	public static IEnumerable<TestCaseData> GetTests(bool useExternal, bool runV1, OutputFormat outputFormat)
	{
		// ReSharper disable once HeuristicUnreachableCode
		var testCasesPath = useExternal ? _externalTestCasesPath : _testCasesPath;

		var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, testCasesPath)
			.AdjustForPlatform();
		if (!Directory.Exists(testsPath)) return [];

		var fileNames = Directory.GetFiles(testsPath, "*.json", SearchOption.AllDirectories);

		var allTests = new List<TestCaseData>();
		foreach (var fileName in fileNames)
		{
			var shortFileName = Path.GetFileNameWithoutExtension(fileName);

			var filePath = fileName.Split('/', '\\');
			var proposals = Array.IndexOf(filePath, "proposals");
			IKeywordHandler? proposedHandler = null;
			if (proposals != -1 && proposals + 1 < filePath.Length)
			{
				var proposal = filePath[proposals + 1];
				if (!_supportedProposals.TryGetValue(proposal, out proposedHandler)) continue;
			}

			var isOptional = fileName.Contains("optional");

			// The ignore list is keyed on the format name, and the suite spells the root files
			// `format-<name>.json`, so strip the prefix to match.
			var ignoreKey = shortFileName.StartsWith("format-", StringComparison.Ordinal)
				? shortFileName.Substring("format-".Length)
				: shortFileName;

			// `format` is an annotation through 2020-12 and an assertion from v1 on, so the tests under
			// `optional/format/` (all of which stop at 2020-12) are the ones that need assertion switched
			// on.  Everything else is left to the dialect's own default, which is what
			// `format-annotation.json` checks.
			var isFormatAssertionFile = fileName.Contains("format/".AdjustForPlatform());

			var contents = File.ReadAllText(fileName);
			var suiteFile = JsonSerializer.Deserialize<TestSuiteFile>(contents, TestEnvironment.TestSuiteSerializationOptions);

			foreach (var collection in suiteFile!.Tests)
			{
				collection.IsOptional = isOptional;

				foreach (var (draftName, release, dialect) in TestSuiteDialects.Supported)
				{
					if (!runV1 && draftName == "v1") continue;
					if (!TestSuiteDialects.IsCompatible(collection.Compatibility, release)) continue;

					foreach (var test in collection.Tests)
					{
						if (IsIgnored(ignoreKey, collection.Description, test.Description)) continue;

						var testDialect = dialect;
						if (proposedHandler is not null)
							testDialect = dialect.With([proposedHandler],
								refIgnoresSiblingKeywords: dialect.RefIgnoresSiblingKeywords,
								allowUnknownKeywords: dialect.AllowUnknownKeywords);

						// Each test gets its own registry.  The test builds the case's schema itself, and a
						// registry only accepts one build of a given schema, so the registry (and the external
						// schemas within it) cannot be shared across the tests of a case.
						var buildOptions = new BuildOptions
						{
							Dialect = testDialect,
							SchemaRegistry = new()
						};
						RegisterExternalSchemas(collection, buildOptions);

						var evaluationOptions = new EvaluationOptions
						{
							OutputFormat = outputFormat,
							RequireFormatValidation = isFormatAssertionFile
						};

						var optional = collection.IsOptional ? "(optional) / " : null;
						var name = $"{draftName} / {optional}{shortFileName} / {collection.Description} / {test.Description}";
						allTests.Add(new TestCaseData(collection, test, shortFileName, buildOptions, evaluationOptions) { TestName = name });
					}
				}
			}
		}

		return allTests;
	}

	private static bool IsIgnored(string ignoreKey, string collectionDescription, string testDescription) =>
		_ignoredTests.Contains((ignoreKey, collectionDescription, testDescription)) ||
		_ignoredTests.Contains((ignoreKey, collectionDescription, "*"));

	/// <summary>
	/// Loads a test case's `externalSchemas` into its own registry, keyed by retrieval URI.  This replaces
	/// the suite's former `remotes` directory.
	/// </summary>
	private static void RegisterExternalSchemas(TestCollection collection, BuildOptions buildOptions)
	{
		if (collection.ExternalSchemas is null) return;

		foreach (var externalSchema in collection.ExternalSchemas)
		{
			var uri = new Uri(externalSchema.Key, UriKind.RelativeOrAbsolute);
			try
			{
				// External schemas generally omit `$id` and `$schema` so that they suit as many dialects as
				// possible, so the retrieval URI supplies the identity and the dialect under test supplies
				// the semantics.  Where a schema does declare `$schema`, the build honors it.
				var schema = JsonSchema.Build(externalSchema.Value, buildOptions, uri);
				// Register under the retrieval URI as well, since a schema's `$id` may differ from it.
				buildOptions.SchemaRegistry.Register(uri, schema);
			}
			catch (JsonSchemaException e)
			{
				TestConsole.WriteLine($"Error loading external schema '{externalSchema.Key}': {e}");
			}
		}
	}
}
