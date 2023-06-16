using System.Threading.Tasks;
using NUnit.Framework;

using static Json.Schema.DataGeneration.Tests.TestHelpers;

namespace Json.Schema.DataGeneration.Tests;

public class ConstGenerationTests
{
	[Test]
	public async Task ConstSchemaGeneratesItsValue()
	{
		var schema = new JsonSchemaBuilder()
			.Const("this is totally a random string");

		await Run(schema);
	}

	[Test]
	public async Task NotConstSchemaGeneratesAnythingButItsValue()
	{
		var schema = new JsonSchemaBuilder()
			.Not(new JsonSchemaBuilder()
				.Const("anything but this value"));

		await Run(schema);
	}
}