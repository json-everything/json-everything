using System.Reflection;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Json.Schema.CodeGeneration.Tests;

public static class Compiler
{
	private static readonly List<MetadataReference> _references;

	static Compiler()
	{
		var refs = AppDomain.CurrentDomain.GetAssemblies();
		var references = new List<MetadataReference>();

		foreach (var reference in refs.Where(x => !x.IsDynamic))
		{
			var stream = File.Open(reference.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			references.Add(MetadataReference.CreateFromStream(stream));
		}

		_references = references;
	}


	public static Assembly? Compile(string source)
	{
		var fullSource = $@"
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Schema;
using Json.Schema.CodeGeneration;

namespace JsonEverythingTest;

{source}
";

		var syntaxTree = CSharpSyntaxTree.ParseText(fullSource);
		var assemblyPath = Path.ChangeExtension(Path.GetTempFileName(), "dll");

		var compilation = CSharpCompilation.Create(Path.GetFileName(assemblyPath))
			.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.AddReferences(_references)
			.AddSyntaxTrees(syntaxTree);

		using var dllStream = new MemoryStream();
		using var pdbStream = new MemoryStream();
		var emitResult = compilation.Emit(dllStream, pdbStream);
		if (!emitResult.Success)
		{
			foreach (var diagnostic in emitResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error))
			{
				Console.WriteLine(JsonSerializer.Serialize(diagnostic, new JsonSerializerOptions { WriteIndented = true }));
			}
			return null;
		}

		var assembly = Assembly.Load(dllStream.ToArray());

		return assembly;
	}
}