using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class DevTests
{
	[Test]
	public async Task Test()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Number)
			.Minimum(0)
			.Maximum(100)
			.Not(new JsonSchemaBuilder().MultipleOf(3));

		await Run(schema);
	}
}