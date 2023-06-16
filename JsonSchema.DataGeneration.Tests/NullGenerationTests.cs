using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class NullGenerationTests
{
	[Test]
	public async Task GenerateNull()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.Null);

		await Run(schema);
	}
}