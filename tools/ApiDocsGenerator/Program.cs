﻿using Json.JsonE;
using Json.Logic;
using Json.More;
using Json.Patch;
using Json.Path;
using Json.Pointer;
using Json.Schema;
using Json.Schema.ArrayExt;
using Json.Schema.CodeGeneration;
using Json.Schema.Data;
using Json.Schema.DataGeneration;
using Json.Schema.Generation.DataAnnotations;
using Json.Schema.Generation.Generators;
using Json.Schema.OpenApi;
using Yaml2JsonNode;

namespace ApiDocsGenerator
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var outputDir = args.Length == 0 ? "output" : args[0];

			CopyReleaseNotes(outputDir, "release-notes");

			await GenerateAndSaveDocs<JsonSchema>("10.01", outputDir, "api");
			await GenerateAndSaveDocs<DataKeyword>("10.02", outputDir, "api");
			await GenerateAndSaveDocs<UniqueKeysKeyword>("10.03", outputDir, "api");
			await GenerateAndSaveDocs<DiscriminatorKeyword>("10.04", outputDir, "api");
			await GenerateAndSaveDocs<ISchemaGenerator>("10.05", outputDir, "api");
			await GenerateAndSaveDocs<StringLengthAttributeHandler>("10.06", outputDir, "api");
			await GenerateAndSaveDocs<NumberRange>("10.07", outputDir, "api");
			await GenerateAndSaveDocs<JsonPath>("10.08", outputDir, "api");
			await GenerateAndSaveDocs<JsonPatch>("10.09", outputDir, "api");
			await GenerateAndSaveDocs<JsonPointer_Old>("10.10", outputDir, "api");
			await GenerateAndSaveDocs(typeof(JsonLogic), "10.11", outputDir, "api");
			await GenerateAndSaveDocs(typeof(JsonE), "10.12", outputDir, "api");
			await GenerateAndSaveDocs(typeof(EnumStringConverter<>), "10.13", outputDir, "api");
			await GenerateAndSaveDocs(typeof(YamlConverter), "10.14", outputDir, "api");
			await GenerateAndSaveDocs(typeof(CodeGenExtensions), "10.15", outputDir, "api");
		}

		private static void CopyReleaseNotes(params string[] outputDir)
		{
			CopyDirectory("release-notes", Path.Combine(outputDir), false);
		}

		// source: https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
		private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
		{
			// Get information about the source directory
			var dir = new DirectoryInfo(sourceDir);

			// Check if the source directory exists
			if (!dir.Exists)
				throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

			// Cache directories before we start copying
			DirectoryInfo[] dirs = dir.GetDirectories();

			// Create the destination directory
			Directory.CreateDirectory(destinationDir);

			// Get the files in the source directory and copy to the destination directory
			foreach (FileInfo file in dir.GetFiles())
			{
				string targetFilePath = Path.Combine(destinationDir, file.Name);
				file.CopyTo(targetFilePath, true);
			}

			// If recursive and copying subdirectories, recursively call this method
			if (recursive)
			{
				foreach (DirectoryInfo subDir in dirs)
				{
					string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
					CopyDirectory(subDir.FullName, newDestinationDir, true);
				}
			}
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