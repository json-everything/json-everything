using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class ReferenceTests
{
	private static string GetFile(string name)
	{
		return Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "Referencing", $"{name}.json")
			.AdjustForPlatform();
	}

	private static string GetResource(string name)
	{
		return File.ReadAllText(GetFile(name));
	}

	[Test]
	public async Task ReferenceFragmentFromFile()
	{
		var baseSchema = JsonSchema.FromFile(GetFile("base_schema"));
		var refSchema = JsonSchema.FromFile(GetFile("ref_schema"));

		var baseData = JsonNode.Parse(GetResource("base_data"));

		await SchemaRegistry.Global.Register(refSchema);
		await SchemaRegistry.Global.Register(baseSchema);

		var res = await baseSchema.Evaluate(baseData);

		res.AssertValid();
	}
	[Test]
	public async Task MultipleHashInUriThrowsException()
	{
		var baseSchema = JsonSchema.FromFile(GetFile("base_schema"));
		var refSchema = JsonSchema.FromFile(GetFile("ref_schema"));
		var hashSchema = JsonSchema.FromFile(GetFile("schema_with_#_in_uri"));

		var baseData = JsonNode.Parse(GetResource("base_data_hash_uri"));

		await SchemaRegistry.Global.Register(refSchema);
		await SchemaRegistry.Global.Register(baseSchema);
		await SchemaRegistry.Global.Register(hashSchema);

		Assert.ThrowsAsync<JsonSchemaException>(() => baseSchema.Evaluate(baseData));
	}
}