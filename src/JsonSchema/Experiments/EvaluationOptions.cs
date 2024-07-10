using System;

namespace Json.Schema.Experiments;

public class EvaluationOptions
{
	public static EvaluationOptions Default { get; } = new() { SchemaRegistry = SchemaRegistry.Global };

	public bool RequireFormatValidation { get; set; }

	public Uri DefaultMetaSchema { get; set; } = MetaSchemas.Draft202012Id;

	public SchemaRegistry SchemaRegistry { get; private init; } = new();
}