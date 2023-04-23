using System.IO;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class References
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
	public void ReferenceFragmentFromFile()
	{
		var baseSchema = JsonSchema.FromFile(GetFile("base_schema"));
		var refSchema = JsonSchema.FromFile(GetFile("ref_schema"));

		var baseData = JsonNode.Parse(GetResource("base_data"));

		SchemaRegistry.Global.Register(refSchema);
		SchemaRegistry.Global.Register(baseSchema);

		var res = baseSchema.Evaluate(baseData);

		res.AssertValid();
	}
	[Test]
	public void MultipleHashInUriThrowsException()
	{
		var baseSchema = JsonSchema.FromFile(GetFile("base_schema"));
		var refSchema = JsonSchema.FromFile(GetFile("ref_schema"));
		var hashSchema = JsonSchema.FromFile(GetFile("schema_with_#_in_uri"));

		var baseData = JsonNode.Parse(GetResource("base_data_hash_uri"));

		SchemaRegistry.Global.Register(refSchema);
		SchemaRegistry.Global.Register(baseSchema);
		SchemaRegistry.Global.Register(hashSchema);
		Assert.Throws<JsonSchemaException>(()=>baseSchema.Evaluate(baseData));
	}
}