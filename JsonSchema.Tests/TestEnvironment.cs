using NUnit.Framework;

namespace Json.Schema.Tests;

[SetUpFixture]
public class TestEnvironment
{
	[OneTimeSetUp]
	public void Setup()
	{
#if !DEBUG
		EvaluationOptions.Default.Log = new TestLog();
#endif
	}
}