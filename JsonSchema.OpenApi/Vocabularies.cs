using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.OpenApi;

/// <summary>
/// Declares the vocabularies of the supported drafts.
/// </summary>
public static class Vocabularies
{
	/// <summary>
	/// The data vocabulary ID.
	/// </summary>
	public const string OpenApiId = "https://spec.openapis.org/oas/3.1/vocab/base";

	/// <summary>
	/// The data vocabulary.
	/// </summary>
	public static readonly Vocabulary OpenApi = new(OpenApiId,
		typeof(ExampleKeyword),
		typeof(DiscriminatorKeyword),
		typeof(ExternalDocsKeyword),
		typeof(XmlKeyword)
	);

	/// <summary>
	/// Registers the all components required to use the data vocabulary.
	/// </summary>
	public static void Register(VocabularyRegistry? vocabRegistry = null, SchemaRegistry? schemaRegistry = null)
	{
		vocabRegistry ??= VocabularyRegistry.Global;
		schemaRegistry ??= SchemaRegistry.Global;

		vocabRegistry.Register(OpenApi);
		SchemaKeywordRegistry.Register<ExampleKeyword>(OpenApiSerializerContext.Default);
		SchemaKeywordRegistry.RegisterNullValue(new ExampleKeyword(null));
		SchemaKeywordRegistry.Register<DiscriminatorKeyword>(OpenApiSerializerContext.Default);
		SchemaKeywordRegistry.Register<ExternalDocsKeyword>(OpenApiSerializerContext.Default);
		SchemaKeywordRegistry.Register<XmlKeyword>(OpenApiSerializerContext.Default);
		schemaRegistry.Register(MetaSchemas.OpenApiMeta);
	}
}

[JsonSerializable(typeof(ExampleKeyword))]
[JsonSerializable(typeof(DiscriminatorKeyword))]
[JsonSerializable(typeof(DiscriminatorKeywordJsonConverter.Model), TypeInfoPropertyName = "DiscriminatorModel")]
[JsonSerializable(typeof(ExternalDocsKeyword))]
[JsonSerializable(typeof(ExternalDocsKeywordJsonConverter.Model), TypeInfoPropertyName = "ExternalDocsModel")]
[JsonSerializable(typeof(XmlKeyword))]
[JsonSerializable(typeof(XmlKeywordJsonConverter.Model), TypeInfoPropertyName = "XmlModel")]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(IReadOnlyDictionary<string, string>))]
internal partial class OpenApiSerializerContext : JsonSerializerContext
{
	public static TypeResolverOptionsManager OptionsManager { get; }

	static OpenApiSerializerContext()
	{
		OptionsManager = new(
#if NET8_0_OR_GREATER
			Default,
			JsonSchema.TypeInfoResolver
#endif
		);
	}
}
