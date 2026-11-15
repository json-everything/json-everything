using System;
using NUnit.Framework;

namespace Json.Schema.Api.Tests;

// WebApplicationFactory falls back to locating the content root from the solution file,
// and before .NET 10 it only recognizes .sln, not .slnx.  Supplying the content root
// through this setting skips that lookup on every target framework.
[SetUpFixture]
public class ContentRootSetup
{
	[OneTimeSetUp]
	public void SetContentRoot()
	{
		var assemblyName = typeof(Program).Assembly.GetName().Name!;
		var settingName = $"ASPNETCORE_TEST_CONTENTROOT_{assemblyName.ToUpperInvariant().Replace(".", "_")}";

		Environment.SetEnvironmentVariable(settingName, AppContext.BaseDirectory);
	}
}
