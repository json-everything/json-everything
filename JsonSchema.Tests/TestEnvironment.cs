using NUnit.Framework;

namespace Json.Schema.Tests
{
	[SetUpFixture]
	public class TestEnvironment
	{
		[OneTimeSetUp]
		public void Setup()
		{
			ValidationOptions.Default.Log = new TestLog();
		}
	}
}