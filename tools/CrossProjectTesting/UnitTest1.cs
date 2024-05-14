using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Yaml2JsonNode;

namespace CrossProjectTesting;

public class Tests
{
	[Test]
	[Ignore("Dev purposes only")]
	public void Test()
	{
		var schema = JsonSchema.FromFile(@"C:\Users\gregs\Downloads\null-repro-JsonSchema\template\ContentTemplate\schemas\BusinessCentralApplicationObject.schema.json");
		var instanceFiles = Directory
			.GetFiles(@"C:\Users\gregs\Downloads\null-repro-JsonSchema\reference", "*.yml", SearchOption.AllDirectories)
			.ToList();

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical,
			PreserveDroppedAnnotations = true
		};

		var instances = new List<JsonNode?>();

		foreach (var instanceFile in instanceFiles)
		{
			Console.WriteLine(instanceFile);
			instances.AddRange(YamlSerializer.Parse(File.ReadAllText(instanceFile)).Documents.Select(x => x.ToJsonNode()));
		}

		var results = new List<EvaluationResults>();

		instances.AsParallel().ForAll(x =>
		{
			var result = schema.Evaluate(x, options);

			results.Add(result);
		});

		foreach (var result in results)
		{
			Console.WriteLine(JsonSerializer.Serialize(result));
		}
	}
}