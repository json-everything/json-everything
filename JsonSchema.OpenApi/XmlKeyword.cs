using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.OpenApi;

/// <summary>
/// Handles `example`.
/// </summary>
[SchemaKeyword(_Name)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[Vocabulary(Vocabularies.OpenApiId)]
[JsonConverter(typeof(XmlKeywordJsonConverter))]
public class XmlKeyword : IJsonSchemaKeyword
{
	// ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
	internal const string _Name = "xml";
#pragma warning restore IDE1006 // Naming Styles

	private readonly JsonNode? _json;

	/// <summary>
	/// The URI of the namespace definition. This MUST be in the form of an absolute URI.
	/// </summary>
	public Uri? Namespace { get; }
	/// <summary>
	/// Replaces the name of the element/attribute used for the described schema property.
	/// When defined within `items`, it will affect the name of the individual XML elements within the list.
	/// When defined alongside `type` being `array` (outside the `items`), it will affect the wrapping element and
	/// only if `wrapped` is `true`. If `wrapped` is `false`, it will be ignored.
	/// </summary>
	public string? Name { get; }
	/// <summary>
	/// The prefix to be used for the name.
	/// </summary>
	public string? Prefix { get; }
	/// <summary>
	/// Declares whether the property definition translates to an attribute instead of an
	/// element. Default value is `false`.
	/// </summary>
	public bool? Attribute { get; }
	/// <summary>
	/// MAY be used only for an array definition. Signifies whether the array is wrapped (for example, `<books><book/><book/></books>`)
	/// or unwrapped (`<book/><book/>`). Default value is `false`. The definition takes effect only when defined alongside `type`
	/// being `array` (outside the `items`).
	/// </summary>
	public bool? Wrapped { get; }
	/// <summary>
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </summary>
	public IReadOnlyDictionary<string, JsonNode?>? Extensions { get; }

	/// <summary>
	/// Creates a new <see cref="ExternalDocsKeyword"/>.
	/// </summary>
	/// <param name="namespace">The URI of the namespace definition. This MUST be in the form of an absolute URI.</param>
	/// <param name="name">Replaces the name of the element/attribute used for the described schema property.
	/// When defined within `items`, it will affect the name of the individual XML elements within the list.
	/// When defined alongside `type` being `array` (outside the `items`), it will affect the wrapping element and
	/// only if `wrapped` is `true`. If `wrapped` is `false`, it will be ignored.</param>
	/// <param name="prefix">The prefix to be used for the name.</param>
	/// <param name="attribute">Declares whether the property definition translates to an attribute instead of an
	/// element. Default value is `false`.</param>
	/// <param name="wrapped">
	/// MAY be used only for an array definition. Signifies whether the array is wrapped (for example, `<books><book/><book/></books>`)
	/// or unwrapped (`<book/><book/>`). Default value is `false`. The definition takes effect only when defined alongside `type`
	/// being `array` (outside the `items`).</param>
	/// <param name="extensions">
	/// Allows extensions to the OpenAPI Schema. The field name MUST begin with `x-`, for example,
	/// `x-internal-id`. Field names beginning `x-oai-` and `x-oas-` are reserved for uses defined by the OpenAPI Initiative.
	/// The value can be null, a primitive, an array or an object.
	/// </param>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public XmlKeyword(Uri? @namespace, string? name, string? prefix, bool? attribute, bool? wrapped, IReadOnlyDictionary<string, JsonNode?>? extensions)
	{
		Namespace = @namespace;
		Name = name;
		Prefix = prefix;
		Attribute = attribute;
		Wrapped = wrapped;
		Extensions = extensions;

		_json = JsonSerializer.SerializeToNode(this, JsonSchemaOpenApiSerializerContext.Default.JsonNode);
	}

	internal XmlKeyword(Uri? @namespace, string? name, string? prefix, bool? attribute, bool? wrapped, IReadOnlyDictionary<string, JsonNode?>? extensions, JsonNode? json)
		: this(@namespace, name, prefix, attribute, wrapped, extensions)
	{
		_json = json;
	}

	/// <summary>
	/// Builds a constraint object for a keyword.
	/// </summary>
	/// <param name="schemaConstraint">The <see cref="SchemaConstraint"/> for the schema object that houses this keyword.</param>
	/// <param name="localConstraints">
	/// The set of other <see cref="KeywordConstraint"/>s that have been processed prior to this one.
	/// Will contain the constraints for keyword dependencies.
	/// </param>
	/// <param name="context">The <see cref="EvaluationContext"/>.</param>
	/// <returns>A constraint object.</returns>
	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
		IReadOnlyList<KeywordConstraint> localConstraints,
		EvaluationContext context)
	{
		return new KeywordConstraint(_Name, (e, _) => e.Results.SetAnnotation(_Name, _json));
	}
}

/// <summary>
/// JSON converter for <see cref="XmlKeyword"/>.
/// </summary>
public sealed class XmlKeywordJsonConverter : AotCompatibleJsonConverter<XmlKeyword>
{
	/// <summary>Reads and converts the JSON to type <see cref="XmlKeyword"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override XmlKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = options.Read(ref reader, JsonSchemaOpenApiSerializerContext.Default.JsonNode)!;

		var namespcText = node["namespace"]?.GetValue<string>();
		var namespc = namespcText is null ? null : new Uri(namespcText);
		var name = node["name"]?.GetValue<string>();
		var prefix = node["prefix"]?.GetValue<string>();
		bool? attribute = null;
		node["attribute"]?.AsValue().TryGetValue(out attribute);
		bool? wrapped = null;
		node["wrapped"]?.AsValue().TryGetValue(out wrapped);
		var extensionData = node!.AsObject().Where(x => x.Key.StartsWith("x-"))
			.ToDictionary(x => x.Key, x => x.Value);

		return new XmlKeyword(namespc, name, prefix, attribute, wrapped, extensionData, node);
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, XmlKeyword value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		if (value.Namespace != null)
			writer.WriteString("namespace", value.Namespace.OriginalString);
		if (value.Name != null)
			writer.WriteString("name", value.Name);
		if (value.Prefix != null)
			writer.WriteString("prefix", value.Prefix);
		if (value.Attribute.HasValue)
			writer.WriteBoolean("attribute", value.Attribute.Value);
		if (value.Wrapped.HasValue)
			writer.WriteBoolean("wrapped", value.Wrapped.Value);

		if (value.Extensions != null)
		{
			foreach (var extension in value.Extensions)
			{
				writer.WritePropertyName(extension.Key);
				options.Write(writer, extension.Value, JsonSchemaOpenApiSerializerContext.Default.JsonNode!);
			}
		}
	}
}