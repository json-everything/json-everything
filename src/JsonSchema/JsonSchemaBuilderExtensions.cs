using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Schema.Keywords;

namespace Json.Schema;

/// <summary>
/// Provides a fluent interface for <see cref="JsonSchemaBuilder"/>.
/// </summary>
public static class JsonSchemaBuilderExtensions
{
	/// <summary>
	/// Add an `additionalItems` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `additionalItems`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder AdditionalItems(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("additionalItems", schema);
		return builder;
	}

	/// <summary>
	/// Add an `additionalProperties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `additionalProperties`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder AdditionalProperties(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("additionalProperties", schema);
		return builder;
	}

	/// <summary>
	/// Add an `allOf` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schemas for `allOf`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder AllOf(this JsonSchemaBuilder builder, params JsonSchemaBuilder[] schemas)
	{
		builder.Add("allOf", schemas);
		return builder;
	}

	/// <summary>
	/// Add an `allOf` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schemas for `allOf`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder AllOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchemaBuilder> schemas)
	{
		builder.Add("allOf", schemas);
		return builder;
	}

	/// <summary>
	/// Add an `$anchor` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="reference">The anchor reference.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Anchor(this JsonSchemaBuilder builder, string reference)
	{
		builder.Add("$anchor", reference);
		return builder;
	}

	/// <summary>
	/// Add an `anyOf` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schemas for `anyOf`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder AnyOf(this JsonSchemaBuilder builder, params JsonSchemaBuilder[] schemas)
	{
		builder.Add("anyOf", schemas);
		return builder;
	}

	/// <summary>
	/// Add an `anyOf` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schemas for `anyOf`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder AnyOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchemaBuilder> schemas)
	{
		builder.Add("anyOf", schemas);
		return builder;
	}

	/// <summary>
	/// Add a `$comment` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="comment">The comment.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Comment(this JsonSchemaBuilder builder, string comment)
	{
		builder.Add("$comment", comment);
		return builder;
	}

	/// <summary>
	/// Add a `const` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="node">The constant value.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Const(this JsonSchemaBuilder builder, JsonNode? node)
	{
		builder.Add("const", node);
		return builder;
	}

	/// <summary>
	/// Add an `contains` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `contains`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Contains(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("contains", schema);
		return builder;
	}

	/// <summary>
	/// Add a `default` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="node">The value.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Default(this JsonSchemaBuilder builder, JsonNode? node)
	{
		builder.Add("default", node);
		return builder;
	}

	/// <summary>
	/// Add a `definitions` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="defs">The schema definition map.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Definitions(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchemaBuilder> defs)
	{
		builder.Add("definitions", defs);
		return builder;
	}

	/// <summary>
	/// Add a `definitions` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="defs">The schema definition map.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Definitions(this JsonSchemaBuilder builder, params (string name, JsonSchemaBuilder schema)[] defs)
	{
		builder.Add("definitions", defs);
		return builder;
	}

	/// <summary>
	/// Add a `$defs` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="defs">The schema definition map.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Defs(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchemaBuilder> defs)
	{
		builder.Add("$defs", defs);
		return builder;
	}

	/// <summary>
	/// Add a `$defs` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="defs">The schema definition map.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Defs(this JsonSchemaBuilder builder, params (string name, JsonSchemaBuilder schema)[] defs)
	{
		builder.Add("$defs", defs);
		return builder;
	}

	/// <summary>
	/// Add a `dependencies` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="deps">The dependencies.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Dependencies(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, SchemaOrPropertyList> deps)
	{
		builder.Add("dependencies", new JsonObject(deps
			.ToDictionary(x => x.Key,
				x => x.Value.Requirements is not null
					? JsonSerializer.SerializeToNode(x.Value.Requirements, JsonSchemaSerializerContext.Default.StringArray)
					: x.Value.Schema!.Keywords.DeepClone())));
		return builder;
	}

	/// <summary>
	/// Add a `dependencies` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="deps">The dependencies.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Dependencies(this JsonSchemaBuilder builder, params (string name, SchemaOrPropertyList dep)[] deps)
	{
		builder.Add("dependencies", new JsonObject(deps
			.ToDictionary(x => x.name,
				x => x.dep.Requirements is not null
					? JsonSerializer.SerializeToNode(x.dep.Requirements, JsonSchemaSerializerContext.Default.StringArray)
					: x.dep.Schema!.Keywords.DeepClone())));
		return builder;
	}

	/// <summary>
	/// Add a `dependentRequired` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="deps">The dependencies.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder DependentRequired(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, IReadOnlyList<string>> deps)
	{
		builder.Add("dependentRequired", new JsonObject(deps.ToDictionary(x => x.Key, x => (JsonNode?)new JsonArray(x.Value.Select(v => (JsonNode?)v).ToArray()))));
		return builder;
	}

	/// <summary>
	/// Add a `dependentRequired` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="deps">The dependencies.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder DependentRequired(this JsonSchemaBuilder builder, params (string name, IEnumerable<string> properties)[] deps)
	{
		builder.Add("dependentRequired", new JsonObject(deps.ToDictionary(x => x.name, x => (JsonNode?)new JsonArray(x.properties.Select(v => (JsonNode?)v).ToArray()))));
		return builder;
	}

	/// <summary>
	/// Add a `dependentSchemas` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="deps">The dependencies.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder DependentSchemas(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchemaBuilder> deps)
	{
		builder.Add("dependentRequired", deps);
		return builder;
	}

	/// <summary>
	/// Add a `dependentSchemas` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="deps">The dependencies.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder DependentSchemas(this JsonSchemaBuilder builder, params (string name, JsonSchemaBuilder schema)[] deps)
	{
		builder.Add("dependentRequired", deps);
		return builder;
	}

	/// <summary>
	/// Add a `deprecated` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="deprecated">Whether the schema is deprecated.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Deprecated(this JsonSchemaBuilder builder, bool deprecated)
	{
		builder.Add("deprecated", (JsonNode)deprecated);
		return builder;
	}

	/// <summary>
	/// Add a `description` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="description">The description.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Description(this JsonSchemaBuilder builder, string description)
	{
		builder.Add("description", description);
		return builder;
	}

	/// <summary>
	/// Add an `$dynamicAnchor` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="reference">The anchor reference.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder DynamicAnchor(this JsonSchemaBuilder builder, string reference)
	{
		builder.Add("$dynamicAnchor", reference);
		return builder;
	}

	/// <summary>
	/// Add a `$dynamicRef` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="reference">The URI reference.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder DynamicRef(this JsonSchemaBuilder builder, Uri reference)
	{
		builder.Add("$dynamicRef", reference.OriginalString);
		return builder;
	}

	/// <summary>
	/// Add a `$dynamicRef` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="reference">The URI reference.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder DynamicRef(this JsonSchemaBuilder builder, string reference)
	{
		builder.Add("$dynamicRef", reference);
		return builder;
	}

	/// <summary>
	/// Add an `else` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `else`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Else(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("else", schema);
		return builder;
	}

	/// <summary>
	/// Add an `enum` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="elements">The values for the enum.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, IEnumerable<JsonNode?> elements)
	{
		builder.Add("enum", new JsonArray(elements.ToArray()));
		return builder;
	}

	/// <summary>
	/// Add an `enum` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="elements">The values for the enum.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, params JsonNode?[] elements)
	{
		builder.Add("enum", new JsonArray(elements));
		return builder;
	}

	/// <summary>
	/// Add an `enum` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="elements">The values for the enum.</param>
	/// <returns>The builder.</returns>
	/// <remarks>
	/// This overload is provided as a convenience as string-only enums are most common.
	/// </remarks>
	public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, IEnumerable<string> elements)
	{
		builder.Add("enum", new JsonArray(elements.Select(x => (JsonNode?)x).ToArray()));
		return builder;
	}

	/// <summary>
	/// Add an `examples` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="elements">The example values.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Examples(this JsonSchemaBuilder builder, IEnumerable<JsonNode?> elements)
	{
		builder.Add("examples", new JsonArray(elements.ToArray()));
		return builder;
	}

	/// <summary>
	/// Add an `examples` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="elements">The example values.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Examples(this JsonSchemaBuilder builder, params JsonNode?[] elements)
	{
		builder.Add("examples", new JsonArray(elements));
		return builder;
	}

	/// <summary>
	/// Add an `exclusiveMaximum` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="max">The max value.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder ExclusiveMaximum(this JsonSchemaBuilder builder, decimal max)
	{
		builder.Add("exclusiveMaximum", max);
		return builder;
	}

	/// <summary>
	/// Add an `exclusiveMinimum` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="min">The min value.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder ExclusiveMinimum(this JsonSchemaBuilder builder, decimal min)
	{
		builder.Add("exclusiveMinimum", min);
		return builder;
	}

	/// <summary>
	/// Add a `format` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="format">The format.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Format(this JsonSchemaBuilder builder, string format)
	{
		builder.Add("format", format);
		return builder;
	}

	/// <summary>
	/// Add a `format` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="format">The format.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Format(this JsonSchemaBuilder builder, Format format)
	{
		builder.Add("format", format.Key);
		return builder;
	}

	/// <summary>
	/// Add an `$id` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="id">The ID.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Id(this JsonSchemaBuilder builder, Uri id)
	{
		builder.Add("$id", id.OriginalString);
		return builder;
	}

	/// <summary>
	/// Add an `$id` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="id">The ID.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Id(this JsonSchemaBuilder builder, string id)
	{
		builder.Add("$id", id);
		return builder;
	}

	/// <summary>
	/// Add an `if` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `if`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder If(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("if", schema);
		return builder;
	}

	/// <summary>
	/// Add a single-schema `items` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `items`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Items(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("items", schema);
		return builder;
	}

	/// <summary>
	/// Add a schema-array `items` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schemas for `items`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Items(this JsonSchemaBuilder builder, IEnumerable<JsonSchemaBuilder> schemas)
	{
		builder.Add("items", schemas);
		return builder;
	}

	/// <summary>
	/// Add a `maxContains` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The max number of expected matches.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MaxContains(this JsonSchemaBuilder builder, uint value)
	{
		builder.Add("maxContains", value);
		return builder;
	}

	/// <summary>
	/// Add a `maximum` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The max value.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Maximum(this JsonSchemaBuilder builder, decimal value)
	{
		builder.Add("maximum", value);
		return builder;
	}

	/// <summary>
	/// Add a `maxItems` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The max number of expected items.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MaxItems(this JsonSchemaBuilder builder, uint value)
	{
		builder.Add("maxItems", value);
		return builder;
	}

	/// <summary>
	/// Add a `maxLength` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The max string length.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MaxLength(this JsonSchemaBuilder builder, uint value)
	{
		builder.Add("maxLength", value);
		return builder;
	}

	/// <summary>
	/// Add a `maxProperties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The max number of expected properties.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MaxProperties(this JsonSchemaBuilder builder, uint value)
	{
		builder.Add("maxProperties", value);
		return builder;
	}

	/// <summary>
	/// Add a `minContains` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The min number of expected matches.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MinContains(this JsonSchemaBuilder builder, uint value)
	{
		builder.Add("minContains", value);
		return builder;
	}

	/// <summary>
	/// Add a `minimum` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The min value.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Minimum(this JsonSchemaBuilder builder, decimal value)
	{
		builder.Add("minimum", value);
		return builder;
	}

	/// <summary>
	/// Add a `minItems` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The min number of expected items.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MinItems(this JsonSchemaBuilder builder, uint value)
	{
		builder.Add("minItems", value);
		return builder;
	}

	/// <summary>
	/// Add a `minLength` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The min string length.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MinLength(this JsonSchemaBuilder builder, uint value)
	{
		builder.Add("minLength", value);
		return builder;
	}

	/// <summary>
	/// Add a `minProperties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The min number of expected properties.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MinProperties(this JsonSchemaBuilder builder, uint value)
	{
		builder.Add("minProperties", value);
		return builder;
	}

	/// <summary>
	/// Add a `multipleOf` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The divisor.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder MultipleOf(this JsonSchemaBuilder builder, decimal value)
	{
		builder.Add("multipleOf", value);
		return builder;
	}

	/// <summary>
	/// Add a `not` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `not`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Not(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("not", schema);
		return builder;
	}

	/// <summary>
	/// Add a `oneOf` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schema for `oneOf`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder OneOf(this JsonSchemaBuilder builder, params JsonSchemaBuilder[] schemas)
	{
		builder.Add("oneOf", schemas);
		return builder;
	}

	/// <summary>
	/// Add a `oneOf` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schemas for `oneOf`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder OneOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchemaBuilder> schemas)
	{
		builder.Add("oneOf", schemas);
		return builder;
	}

	/// <summary>
	/// Add a `pattern` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="pattern">The Regex instance to match.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Pattern(this JsonSchemaBuilder builder, Regex pattern)
	{
		builder.Add("pattern", pattern.ToString());
		return builder;
	}

	/// <summary>
	/// Add a `pattern` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="pattern">The pattern to match.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Pattern(this JsonSchemaBuilder builder, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern)
	{
		builder.Add("pattern", pattern);
		return builder;
	}

	/// <summary>
	/// Add a `patternProperties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="props">The property schemas.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder PatternProperties(this JsonSchemaBuilder builder, IReadOnlyDictionary<Regex, JsonSchemaBuilder> props)
	{
		builder.Add("patternProperties", props.ToDictionary(x => x.Key.ToString(), x => x.Value));
		return builder;
	}

	/// <summary>
	/// Add a `patternProperties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="props">The property schemas.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder PatternProperties(this JsonSchemaBuilder builder, params (Regex pattern, JsonSchemaBuilder schema)[] props)
	{
		builder.Add("patternProperties", props.ToDictionary(x => x.pattern.ToString(), x => x.schema));
		return builder;
	}

	/// <summary>
	/// Add a `patternProperties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="props">The property schemas.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder PatternProperties(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchemaBuilder> props)
	{
		builder.Add("patternProperties", props.ToDictionary(x => x.Key, x => x.Value));
		return builder;
	}

	/// <summary>
	/// Add a `patternProperties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="props">The property schemas.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder PatternProperties(this JsonSchemaBuilder builder, params (string pattern, JsonSchemaBuilder schema)[] props)
	{
		builder.Add("patternProperties", props.ToDictionary(x => x.pattern, x => x.schema));
		return builder;
	}

	/// <summary>
	/// Add a `prefixItems` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schemas for `prefixItems`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder PrefixItems(this JsonSchemaBuilder builder, params JsonSchemaBuilder[] schemas)
	{
		builder.Add("prefixItems", schemas);
		return builder;
	}

	/// <summary>
	/// Add a `prefixItems` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schemas">The schemas for `prefixItems`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder PrefixItems(this JsonSchemaBuilder builder, IEnumerable<JsonSchemaBuilder> schemas)
	{
		builder.Add("prefixItems", schemas);
		return builder;
	}

	/// <summary>
	/// Add a `properties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="props">The property schemas.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Properties(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchemaBuilder> props)
	{
		builder.Add("properties", props);
		return builder;
	}

	/// <summary>
	/// Add a `properties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="props">The property schemas.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Properties(this JsonSchemaBuilder builder, params (string name, JsonSchemaBuilder schema)[] props)
	{
		builder.Add("properties", props);
		return builder;
	}

	///// <summary>
	///// Adds a `propertyDependencies` keyword.
	///// </summary>
	///// <param name="builder">The builder.</param>
	///// <param name="dependencies">The property dependency schemas.</param>
	///// <returns>The builder.</returns>
	//public static JsonSchemaBuilder PropertyDependencies(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, PropertyDependency> dependencies)
	//{
	//	builder.Add(new PropertyDependenciesKeyword(dependencies.ToDictionary(x => x.Key, x => x.Value)));
	//	return builder;
	//}

	///// <summary>
	///// Adds a `propertyDependencies` keyword.
	///// </summary>
	///// <param name="builder">The builder.</param>
	///// <param name="dependencies">The property dependency schemas.</param>
	///// <returns>The builder.</returns>
	//public static JsonSchemaBuilder PropertyDependencies(this JsonSchemaBuilder builder, params (string property, PropertyDependency dependency)[] dependencies)
	//{
	//	builder.Add(new PropertyDependenciesKeyword(dependencies.ToDictionary(x => x.property, x => x.dependency)));
	//	return builder;
	//}

	/// <summary>
	/// Add a `propertyNames` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `propertyNames`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder PropertyNames(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("propertyNames", schema);
		return builder;
	}

	/// <summary>
	/// Add a `readOnly` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">Whether the instance is read-only.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder ReadOnly(this JsonSchemaBuilder builder, bool value)
	{
		builder.Add("readOnly", (JsonNode)value);
		return builder;
	}

	/// <summary>
	/// Add a `$recursiveAnchor` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">The value.</param>
	/// <returns>The builder.</returns>
	/// <remarks>
	/// Per Draft 2019-09, the value must always be `true`.  This is implied for this method.
	/// </remarks>
	public static JsonSchemaBuilder RecursiveAnchor(this JsonSchemaBuilder builder, bool value = true)
	{
		builder.Add("$recursiveAnchor", (JsonNode)value);
		return builder;
	}

	/// <summary>
	/// Add a `$recursiveRef` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="reference">The URI reference.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder RecursiveRef(this JsonSchemaBuilder builder, Uri reference)
	{
		builder.Add("$recursiveRef", reference.OriginalString);
		return builder;
	}

	/// <summary>
	/// Add a `$recursiveRef` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="reference">The URI reference.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder RecursiveRef(this JsonSchemaBuilder builder, string reference)
	{
		builder.Add("$recursiveRef", reference);
		return builder;
	}

	/// <summary>
	/// Add a `$ref` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="reference">The URI reference.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Ref(this JsonSchemaBuilder builder, Uri reference)
	{
		builder.Add("$ref", reference.OriginalString);
		return builder;
	}

	/// <summary>
	/// Add a `$ref` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="reference">The URI reference.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Ref(this JsonSchemaBuilder builder, string reference)
	{
		builder.Add("$ref", reference);
		return builder;
	}

	/// <summary>
	/// Add a `required` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="properties">The required property collections.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Required(this JsonSchemaBuilder builder, IEnumerable<string> properties)
	{
		builder.Add("required", new JsonArray(properties.Select(x => (JsonNode?)x).ToArray()));
		return builder;
	}

	/// <summary>
	/// Add a `required` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="properties">The required property collections.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Required(this JsonSchemaBuilder builder, params string[] properties)
	{
		builder.Add("required", new JsonArray(properties.Select(x => (JsonNode?)x).ToArray()));
		return builder;
	}

	/// <summary>
	/// Add a `$schema` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="uri">The uri of the meta-schema.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Schema(this JsonSchemaBuilder builder, Uri uri)
	{
		builder.Add("$schema", uri.OriginalString);
		return builder;
	}

	/// <summary>
	/// Add a `$schema` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="uri">The uri of the meta-schema.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Schema(this JsonSchemaBuilder builder, string uri)
	{
		builder.Add("$schema", uri);
		return builder;
	}

	/// <summary>
	/// Add a `then` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `then`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Then(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("then", schema);
		return builder;
	}

	/// <summary>
	/// Add a `title` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="title">The title.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Title(this JsonSchemaBuilder builder, string title)
	{
		builder.Add("title", title);
		return builder;
	}

	/// <summary>
	/// Add a `type` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="type">The type.  Can be combined with the bit-wise OR operator `|`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Type(this JsonSchemaBuilder builder, SchemaValueType type)
	{
		var types = GetString(type);
		if (types.Length == 1)
			builder.Add("type", types[0]);
		else
			builder.Add("type", new JsonArray(types));
		return builder;
	}

	/// <summary>
	/// Add a `type` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="types">The types.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Type(this JsonSchemaBuilder builder, params SchemaValueType[] types)
	{
		var type = types.Aggregate((SchemaValueType)0, (t, x) => t | x);
		var strings = GetString(type);
		if (strings.Length == 1)
			builder.Add("type", strings[0]);
		else
			builder.Add("type", new JsonArray(strings));
		return builder;
	}

	/// <summary>
	/// Add a `type` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="types">The types.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Type(this JsonSchemaBuilder builder, IEnumerable<SchemaValueType> types)
	{
		var type = types.Aggregate((SchemaValueType)0, (t, x) => t | x);
		var strings = GetString(type);
		if (strings.Length == 1)
			builder.Add("type", strings[0]);
		else
			builder.Add("type", new JsonArray(strings));
		return builder;
	}

	private static JsonNode?[] GetString(SchemaValueType type)
	{
		var strings = new List<JsonNode?>();
		
		if (type.HasFlag(SchemaValueType.Null))
			strings.Add("null");
		if (type.HasFlag(SchemaValueType.Boolean))
			strings.Add("boolean");
		if (type.HasFlag(SchemaValueType.Object))
			strings.Add("object");
		if (type.HasFlag(SchemaValueType.Array))
			strings.Add("array");
		if (type.HasFlag(SchemaValueType.Number))
			strings.Add("number");
		if (type.HasFlag(SchemaValueType.String))
			strings.Add("string");
		if (type.HasFlag(SchemaValueType.Integer))
			strings.Add("integer");
		
		return strings.ToArray();
	}

	/// <summary>
	/// Add an `unevaluatedItems` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `unevaluatedItems`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder UnevaluatedItems(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("unevaluatedItems", schema);
		return builder;
	}

	/// <summary>
	/// Add an `unevaluatedProperties` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="schema">The schema for `unevaluatedProperties`.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder UnevaluatedProperties(this JsonSchemaBuilder builder, JsonSchemaBuilder schema)
	{
		builder.Add("unevaluatedItems", schema);
		return builder;
	}

	/// <summary>
	/// Add a `uniqueItems` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">Whether to expect a unique item set.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder UniqueItems(this JsonSchemaBuilder builder, bool value)
	{
		builder.Add("uniqueItems", (JsonNode)value);
		return builder;
	}

	/// <summary>
	/// Adds a keyword that's not recognized by any vocabulary - extra data - to the schema.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="name">The keyword name.</param>
	/// <param name="value">The value.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Unrecognized(this JsonSchemaBuilder builder, string name, JsonNode? value)
	{
		builder.Add(name, value);
		return builder;
	}

	/// <summary>
	/// Add an `$vocabulary` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="vocabs">The vocabulary callouts.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Vocabulary(this JsonSchemaBuilder builder, params (Uri id, bool required)[] vocabs)
	{
		builder.Add("$vocabulary", new JsonObject(vocabs.ToDictionary(x => x.id.OriginalString, x => (JsonNode?)x.required)));
		return builder;
	}

	/// <summary>
	/// Add an `$vocabulary` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="vocabs">The vocabulary callouts.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Vocabulary(this JsonSchemaBuilder builder, params (string id, bool required)[] vocabs)
	{
		builder.Add("$vocabulary", new JsonObject(vocabs.ToDictionary(x => x.id, x => (JsonNode?)x.required)));
		return builder;
	}

	/// <summary>
	/// Add an `$vocabulary` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="vocabs">The vocabulary callouts.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Vocabulary(this JsonSchemaBuilder builder, IReadOnlyDictionary<Uri, bool> vocabs)
	{
		builder.Add("$vocabulary", new JsonObject(vocabs.ToDictionary(x => x.Key.OriginalString, x => (JsonNode?)x.Value)));
		return builder;
	}

	/// <summary>
	/// Add an `$vocabulary` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="vocabs">The vocabulary callouts.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder Vocabulary(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, bool> vocabs)
	{
		builder.Add("$vocabulary", new JsonObject(vocabs.ToDictionary(x => x.Key, x => (JsonNode?)x.Value)));
		return builder;
	}

	/// <summary>
	/// Add a `writeOnly` keyword.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="value">Whether the instance is write-only.</param>
	/// <returns>The builder.</returns>
	public static JsonSchemaBuilder WriteOnly(this JsonSchemaBuilder builder, bool value)
	{
		builder.Add("writeOnly", (JsonNode)value);
		return builder;
	}

	/// <summary>
	/// Convenience method that builds and evaluates with a single call.
	/// </summary>
	/// <param name="builder">The builder.</param>
	/// <param name="root">The root instance.</param>
	/// <param name="options">The options to use for this evaluation.</param>
	/// <returns>A <see cref="EvaluationResults"/> that provides the outcome of the evaluation.</returns>
	public static EvaluationResults Evaluate(this JsonSchemaBuilder builder, JsonElement root, EvaluationOptions? options = null)
	{
		return builder.Build().Evaluate(root, options);
	}
}