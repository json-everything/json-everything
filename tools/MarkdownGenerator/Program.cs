using Json.Logic;
using Json.More;
using Json.Patch;
using Json.Path;
using Json.Pointer;
using Json.Schema;
using Json.Schema.Data;
using Json.Schema.DataGeneration;
using Json.Schema.Generation.Generators;
using Json.Schema.OpenApi;
using Json.Schema.UniqueKeys;
using Yaml2JsonNode;

namespace ApiDocsGenerator
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var outputDir = args.Length == 0 ? "output" : args[0];

			await GenerateAndSaveDocs<JsonSchema>(outputDir, "schema");
			await GenerateAndSaveDocs<JsonPatch>(outputDir, "patch");
			await GenerateAndSaveDocs<JsonPath>(outputDir, "path");
			await GenerateAndSaveDocs<JsonPointer>(outputDir, "pointer");
			await GenerateAndSaveDocs(typeof(JsonLogic), outputDir, "logic");
			await GenerateAndSaveDocs(typeof(YamlConverter), outputDir, "yaml");
			await GenerateAndSaveDocs(typeof(JsonNull), outputDir, "more");
			await GenerateAndSaveDocs<DataKeyword>(outputDir, "schema", "data");
			await GenerateAndSaveDocs<UniqueKeysKeyword>(outputDir, "schema", "uniqueKeys");
			await GenerateAndSaveDocs<DiscriminatorKeyword>(outputDir, "schema", "openapi");
			await GenerateAndSaveDocs<NumberRange>(outputDir, "schema", "datagen");
			await GenerateAndSaveDocs<ISchemaGenerator>(outputDir, "schema", "schemagen");
		}

		private static Task GenerateAndSaveDocs<T>(params string[] outputDir)
		{
			return GenerateAndSaveDocs(typeof(T), outputDir);
		}

		private static async Task GenerateAndSaveDocs(Type type, params string[] outputDir)
		{
			var docs = await ApiDocGenerationService.GenerateForAssemblyContaining(type);

			foreach (var (path, doc) in docs)
			{
				var fullOutputPath = outputDir.Concat(path).ToArray();
				var filePath = $"{Path.Combine(fullOutputPath)}.md";

				var directoryName = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directoryName))
					Directory.CreateDirectory(directoryName!);
				await File.WriteAllTextAsync(filePath, doc);
			}
		}
	}
}