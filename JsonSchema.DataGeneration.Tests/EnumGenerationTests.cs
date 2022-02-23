using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests
{
	public class EnumGenerationTests
	{
		[Test]
		public void EnumPicksAValue()
		{
			var schema = new JsonSchemaBuilder()
				.Enum("these", "are", "all", "the", "options");

			Run(schema);
		}
	}
}