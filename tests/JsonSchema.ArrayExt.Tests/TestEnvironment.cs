using NUnit.Framework;

namespace Json.Schema.ArrayExt.Tests;

[SetUpFixture]
public class TestEnvironment
{
	[OneTimeSetUp]
	public void Setup()
	{
		BuildOptions.Default.Dialect = Dialect.ArrayExt_202012;
		MetaSchemas.Register();
		EvaluationOptions.Default.OutputFormat = OutputFormat.Hierarchical;
	}
}