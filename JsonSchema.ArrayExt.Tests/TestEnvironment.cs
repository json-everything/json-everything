using NUnit.Framework;

namespace Json.Schema.ArrayExt.Tests;

[SetUpFixture]
public class TestEnvironment
{
	[OneTimeSetUp]
	public void Setup()
	{
		Vocabularies.Register();
		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}
}