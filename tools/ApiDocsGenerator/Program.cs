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

			await GenerateAndSaveDocs<JsonSchema>("9.01", outputDir);
			await GenerateAndSaveDocs<DataKeyword>("9.02", outputDir);
			await GenerateAndSaveDocs<UniqueKeysKeyword>("9.03", outputDir);
			await GenerateAndSaveDocs<DiscriminatorKeyword>("9.04", outputDir);
			await GenerateAndSaveDocs<ISchemaGenerator>("9.05", outputDir);
			await GenerateAndSaveDocs<NumberRange>("9.06", outputDir);
			await GenerateAndSaveDocs<JsonPath>("9.07", outputDir);
			await GenerateAndSaveDocs<JsonPatch>("9.08", outputDir);
			await GenerateAndSaveDocs<JsonPointer>("9.09", outputDir);
			await GenerateAndSaveDocs(typeof(JsonLogic), "9.10", outputDir);
			await GenerateAndSaveDocs(typeof(JsonNull), "9.11", outputDir);
			await GenerateAndSaveDocs(typeof(YamlConverter), "9.12", outputDir);
		}

		private static Task GenerateAndSaveDocs<T>(string index, params string[] outputDir)
		{
			return GenerateAndSaveDocs(typeof(T), index, outputDir);
		}

		private static async Task GenerateAndSaveDocs(Type type, string index, params string[] outputDir)
		{
			var docs = await ApiDocGenerationService.GenerateForAssemblyContaining(type, index);
			var asmName = type.Assembly.GetName().Name!;

			foreach (var (path, doc) in docs)
			{
				var fullOutputPath = outputDir.Concat(new []{asmName, path}).ToArray();
				var filePath = $"{Path.Combine(fullOutputPath)}.md";

				var directoryName = Path.GetDirectoryName(filePath)!;
				if (!Directory.Exists(directoryName))
					Directory.CreateDirectory(directoryName);
				var titlePath = Path.Combine(directoryName, "title.md");
				var (title, close) = ApiDocGenerationService.GenerateFolderMarkersForNamespace(type, index);
				var closePath = Path.Combine(directoryName, "close.md");
				await WriteFile(titlePath, title);
				await WriteFile(closePath, close);
				await WriteFile(filePath, doc);
			}
		}

		private static async Task WriteFile(string filePath, string content)
		{
			Console.WriteLine("Writing file: '{0}'", filePath);
			await File.WriteAllTextAsync(filePath, content);
		}
	}
}