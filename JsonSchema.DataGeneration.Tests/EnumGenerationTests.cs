using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class EnumGenerationTests
{
	[Test]
	public async Task EnumPicksAValue()
	{
		var schema = new JsonSchemaBuilder()
			.Enum("these", "are", "all", "the", "options");

		await Run(schema);
	}
}