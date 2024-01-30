using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Data;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabularies
{
	/// <summary>
	/// The data vocabulary ID.
	/// </summary>
	public const string DataId = "https://docs.json-everything.net/schema/vocabs/data-2023";

	/// <summary>
	/// The data vocabulary.
	/// </summary>
	public static readonly Vocabulary Data = new(DataId, typeof(DataKeyword));

	/// <summary>
	/// Registers the all components required to use the data vocabulary.
	/// </summary>
	public static void Register(VocabularyRegistry? vocabRegistry = null, SchemaRegistry? schemaRegistry = null)
	{
		vocabRegistry ??= VocabularyRegistry.Global;
		schemaRegistry ??= SchemaRegistry.Global;

		vocabRegistry.Register(Data);
		SchemaKeywordRegistry.Register<DataKeyword>(DataExtSerializerContext.Default);
		schemaRegistry.Register(MetaSchemas.Data);
		schemaRegistry.Register(MetaSchemas.Data_202012);
	}
}

[JsonSerializable(typeof(DataKeyword))]
[JsonSerializable(typeof(OptionalDataKeyword))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, JsonNode>))]
[JsonSerializable(typeof(JsonPointer))]
[JsonSerializable(typeof(RelativeJsonPointer))]
[JsonSerializable(typeof(Uri))]
internal partial class DataExtSerializerContext : JsonSerializerContext
{
	public static TypeResolverOptionsManager OptionsManager { get; }

	static DataExtSerializerContext() {
		OptionsManager = new(
			Default,
			JsonSchema.TypeInfoResolver
		);
	}
}
