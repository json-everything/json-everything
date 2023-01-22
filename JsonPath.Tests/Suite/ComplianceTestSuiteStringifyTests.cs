using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Path.Tests.Suite;

public class ComplianceTestSuiteStringifyTests
{
	[TestCaseSource(typeof(ComplianceTestSuiteTests), nameof(ComplianceTestSuiteTests.TestCases))]
	public void Run(ComplianceTestCase testCase)
	{
		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine(testCase);
		Console.WriteLine();

		JsonPath? path = null;

		var time = Debugger.IsAttached ? int.MaxValue : 100;
		using var cts = new CancellationTokenSource(time);
		Task.Run(() =>
		{
			if (testCase.Document == null) return;
			path = JsonPath.Parse(testCase.Selector);
		}, cts.Token).Wait(cts.Token);

		if (path != null && testCase.InvalidSelector)
			Assert.Inconclusive($"{testCase.Selector} is not a valid path but was parsed without error.");

		if (path == null)
		{
			if (testCase.InvalidSelector) return;
			Assert.Fail($"Could not parse path: {testCase.Selector}");
		}

		var backToString = path.ToString();
		Console.WriteLine(backToString);

		if (testCase.Selector != backToString)
			Assert.Inconclusive();
	}
}