using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class ConstGenerationTests
	{
		[Test]
		public void ConstSchemaGeneratesItsValue()
		{
			var schema = new JsonSchemaBuilder()
				.Const("this is totally a random string");

			Run(schema);
		}
	}
}
