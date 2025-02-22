﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Logic.Tests.Suite;

public class SuiteRunner
{
	public static IEnumerable<TestCaseData> Suite()
	{
		var text = Task.Run(async () =>
		{
			var testsPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files\\tests.json").AdjustForPlatform();

			string? content = null;
			try
			{
				using var client = new HttpClient();
				using var request = new HttpRequestMessage(HttpMethod.Get, "https://jsonlogic.com/tests.json");
				using var response = await client.SendAsync(request);

				content = await response.Content.ReadAsStringAsync();

				File.WriteAllText(testsPath, content);
			}
			catch (Exception e)
			{
				content ??= File.ReadAllText(testsPath);

				TestConsole.WriteLine(e);
			}
			return content;

		}).Result;

		var testSuite = JsonSerializer.Deserialize(text, TestSerializerContext.Default.TestSuite);

		return testSuite!.Tests.Select(t => new TestCaseData(t) { TestName = $"{t.Logic}  |  {t.Data.AsJsonString()}  |  {t.Expected.AsJsonString()}" });
	}

	[TestCaseSource(nameof(Suite))]
	public void Run(Test test)
	{
		var rule = JsonSerializer.Deserialize(test.Logic, TestSerializerContext.Default.Rule);

		if (rule == null)
		{
			Assert.That(test.Expected, Is.Null);
			return;
		}

		JsonAssert.AreEquivalent(test.Expected, rule.Apply(test.Data));
	}
}