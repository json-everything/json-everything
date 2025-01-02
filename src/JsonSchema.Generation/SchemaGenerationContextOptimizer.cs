using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Provides meta-data about the generation process.
/// </summary>
public static class SchemaGenerationContextOptimizer
{
	/// <summary>
	/// Provides custom naming functionality.
	/// </summary>
	public static ITypeNameGenerator? TypeNameGenerator { get; set; }

	internal static void Optimize()
	{
		var allContexts = SchemaGenerationContextCache.Cache.Values;
		var root = allContexts.First();

		var reffedContexts = allContexts.Where(x => x.References.Count >= 1 && x.Intents.Count > 1)
			.ToDictionary(x => x.DefinitionName, SchemaGenerationContextBase (x) => x);

		if (reffedContexts.Count != 0)
		{
			var defsIntent = new DefsIntent(reffedContexts);
			root.Intents.Add(defsIntent);
		}
	}
}