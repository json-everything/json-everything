using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Json.Schema.CodeGeneration.Language;
using Json.Schema.CodeGeneration.Model;

namespace Json.Schema.CodeGeneration;

/// <summary>
/// Generates code from a <see cref="JsonSchema"/>.
/// </summary>
public static class CodeGenExtensions
{
	internal static readonly JsonSerializerOptions SerializerOptions = new();

	/// <summary>
	/// Generates code from a <see cref="JsonSchema"/>.
	/// </summary>
	/// <param name="schema">The JSON Schema object.</param>
	/// <param name="codeWriter">The writer for the output language.</param>
	/// <param name="options">Evaluation options.</param>
	/// <returns></returns>
	public static string GenerateCode(this JsonSchema schema, ICodeWriter codeWriter, EvaluationOptions? options = null)
	{
		options = EvaluationOptions.From(options ?? EvaluationOptions.Default);
		options.SchemaRegistry.Register(schema);

		var generationCache = new GenerationCache();
		var model = schema.GenerateCodeModel(options, generationCache);

		generationCache.FillPlaceholders();

		var sb = new StringBuilder();

		var allModels = CollectModels(model)
			.Distinct()
			.GroupBy(x => codeWriter.TransformName(x.Name))
			.ToArray();
		var duplicateNames = allModels.Where(x => x.Key != null && x.Count() != 1);

		// ReSharper disable PossibleMultipleEnumeration
		if (duplicateNames.Any())
		{
			var names = string.Join(",", duplicateNames.Select(x => x.Key));
			// ReSharper restore PossibleMultipleEnumeration
			throw new UnsupportedSchemaException($"Found multiple definitions for the names [{names}]");
		}

		foreach (var singleModel in allModels.Where(x => x.Key != null))
		{
			codeWriter.Write(sb, singleModel.Single());
		}

		return sb.ToString();
	}

	private static IEnumerable<TypeModel> CollectModels(TypeModel model)
	{
		var found = new List<TypeModel>();
		var toCheck = new Queue<TypeModel>();
		toCheck.Enqueue(model);
		while (toCheck.Count != 0)
		{
			var current = toCheck.Dequeue();
			if (found.Contains(current)) continue;

			found.Add(current);
			switch (current)
			{
				case ArrayModel arrayModel:
					toCheck.Enqueue(arrayModel.Items);
					break;
				case ObjectModel objectModel:
					foreach (var propertyModel in objectModel.Properties)
					{
						toCheck.Enqueue(propertyModel.Type);
					}
					break;
				case DictionaryModel dictionaryModel:
					toCheck.Enqueue(dictionaryModel.Keys);
					toCheck.Enqueue(dictionaryModel.Items);
					break;
			}
		}

		return found;
	}
}
