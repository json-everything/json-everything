using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;
using TestHelpers;
using YamlDotNet.RepresentationModel;

namespace Yaml2JsonNode.Tests;

public class Tests
{
	private static readonly JsonSerializerOptions _serializerOptions = new()
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static IEnumerable<TestCaseData> TestCases
	{
		get
		{
			var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Files", "yaml-content.yaml")
				.AdjustForPlatform();

			var text = File.ReadAllText(path);

			var stream = new YamlStream();
			stream.Load(new StringReader(text));

			return stream.Documents.Select(x => new TestCaseData(x));
		}
	}

	[TestCaseSource(nameof(TestCases))]
	public void JsonIsTheSameAsWhatYamlDotNetGives(YamlDocument yaml)
	{
		var myJson = yaml.ToJsonNode();

		var theirJson = JsonNode.Parse(yaml.RootNode.SerializeToJson());

		Console.WriteLine(myJson.AsJsonString(_serializerOptions));

		JsonAssert.AreEquivalent(theirJson, myJson);
	}
}