using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Json.More;

namespace Json.Schema
{
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
		public static JsonSchemaBuilder AdditionalItems(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new AdditionalItemsKeyword(schema));
			return builder;
		}

		/// <summary>
		/// Add an `additionalProperties` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `additionalProperties`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder AdditionalProperties(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new AdditionalPropertiesKeyword(schema));
			return builder;
		}

		/// <summary>
		/// Add an `allOf` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schemas for `allOf`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder AllOf(this JsonSchemaBuilder builder, params JsonSchema[] schemas)
		{
			builder.Add(new AllOfKeyword(schemas));
			return builder;
		}

		/// <summary>
		/// Add an `allOf` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schemas for `allOf`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder AllOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new AllOfKeyword(schemas));
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
			builder.Add(new AnchorKeyword(reference));
			return builder;
		}

		/// <summary>
		/// Add an `anyOf` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schemas for `anyOf`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder AnyOf(this JsonSchemaBuilder builder, params JsonSchema[] schemas)
		{
			builder.Add(new AnyOfKeyword(schemas));
			return builder;
		}

		/// <summary>
		/// Add an `anyOf` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schemas for `anyOf`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder AnyOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new AnyOfKeyword(schemas));
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
			builder.Add(new CommentKeyword(comment));
			return builder;
		}

		/// <summary>
		/// Add a `const` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="element">The constant value.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Const(this JsonSchemaBuilder builder, JsonElement element)
		{
			builder.Add(new ConstKeyword(element));
			return builder;
		}

		/// <summary>
		/// Add a `const` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="element">The constant value.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Const(this JsonSchemaBuilder builder, JsonElementProxy element)
		{
			builder.Add(new ConstKeyword(element));
			return builder;
		}

		/// <summary>
		/// Add an `contains` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `contains`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Contains(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new ContainsKeyword(schema));
			return builder;
		}

		/// <summary>
		/// Add a `default` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="element">The value.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Default(this JsonSchemaBuilder builder, JsonElement element)
		{
			builder.Add(new DefaultKeyword(element));
			return builder;
		}

		/// <summary>
		/// Add a `default` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="element">The value.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Default(this JsonSchemaBuilder builder, JsonElementProxy element)
		{
			builder.Add(new DefaultKeyword(element));
			return builder;
		}

		/// <summary>
		/// Add a `definitions` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="defs">The schema definition map.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Definitions(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchema> defs)
		{
			builder.Add(new DefinitionsKeyword(defs));
			return builder;
		}

		/// <summary>
		/// Add a `definitions` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="defs">The schema definition map.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Definitions(this JsonSchemaBuilder builder, params (string name, JsonSchema schema)[] defs)
		{
			builder.Add(new DefinitionsKeyword(defs.ToDictionary(x => x.name, x => x.schema)));
			return builder;
		}

		/// <summary>
		/// Add a `$defs` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="defs">The schema definition map.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Defs(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchema> defs)
		{
			builder.Add(new DefsKeyword(defs));
			return builder;
		}

		/// <summary>
		/// Add a `$defs` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="defs">The schema definition map.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Defs(this JsonSchemaBuilder builder, params (string name, JsonSchema schema)[] defs)
		{
			builder.Add(new DefsKeyword(defs.ToDictionary(x => x.name, x => x.schema)));
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
			builder.Add(new DependenciesKeyword(deps));
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
			builder.Add(new DependenciesKeyword(deps.ToDictionary(x => x.name, x => x.dep)));
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
			builder.Add(new DependentRequiredKeyword(deps));
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
			builder.Add(new DependentRequiredKeyword(deps.ToDictionary(x => x.name, x => (IReadOnlyList<string>) x.properties.ToList())));
			return builder;
		}

		/// <summary>
		/// Add a `dependentSchemas` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="deps">The dependencies.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder DependentSchemas(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchema> deps)
		{
			builder.Add(new DependentSchemasKeyword(deps));
			return builder;
		}

		/// <summary>
		/// Add a `dependentSchemas` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="deps">The dependencies.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder DependentSchemas(this JsonSchemaBuilder builder, params (string name, JsonSchema schema)[] deps)
		{
			builder.Add(new DependentSchemasKeyword(deps.ToDictionary(x => x.name, x => x.schema)));
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
			builder.Add(new DeprecatedKeyword(deprecated));
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
			builder.Add(new DescriptionKeyword(description));
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
			builder.Add(new DynamicAnchorKeyword(reference));
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
			builder.Add(new DynamicRefKeyword(reference));
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
			builder.Add(new DynamicRefKeyword(new Uri(reference, UriKind.RelativeOrAbsolute)));
			return builder;
		}

		/// <summary>
		/// Add an `else` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `else`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Else(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new ElseKeyword(schema));
			return builder;
		}

		/// <summary>
		/// Add an `enum` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="elements">The values for the enum.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, IEnumerable<JsonElement> elements)
		{
			builder.Add(new EnumKeyword(elements));
			return builder;
		}

		/// <summary>
		/// Add an `enum` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="elements">The values for the enum.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, IEnumerable<JsonElementProxy> elements)
		{
			builder.Add(new EnumKeyword(elements.Select(p => (JsonElement) p)));
			return builder;
		}

		/// <summary>
		/// Add an `enum` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="elements">The values for the enum.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, params JsonElement[] elements)
		{
			builder.Add(new EnumKeyword(elements));
			return builder;
		}

		/// <summary>
		/// Add an `enum` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="elements">The values for the enum.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, params JsonElementProxy[] elements)
		{
			builder.Add(new EnumKeyword(elements.Select(p => (JsonElement) p)));
			return builder;
		}

		/// <summary>
		/// Add an `examples` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="elements">The example values.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Examples(this JsonSchemaBuilder builder, IEnumerable<JsonElement> elements)
		{
			builder.Add(new ExamplesKeyword(elements));
			return builder;
		}

		/// <summary>
		/// Add an `examples` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="elements">The example values.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Examples(this JsonSchemaBuilder builder, params JsonElement[] elements)
		{
			builder.Add(new ExamplesKeyword(elements));
			return builder;
		}

		/// <summary>
		/// Add an `examples` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="elements">The example values.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Examples(this JsonSchemaBuilder builder, IEnumerable<JsonElementProxy> elements)
		{
			builder.Add(new ExamplesKeyword(elements.Select(p => (JsonElement) p)));
			return builder;
		}

		/// <summary>
		/// Add an `examples` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="elements">The example values.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Examples(this JsonSchemaBuilder builder, params JsonElementProxy[] elements)
		{
			builder.Add(new ExamplesKeyword(elements.Select(p => (JsonElement) p)));
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
			builder.Add(new ExclusiveMaximumKeyword(max));
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
			builder.Add(new ExclusiveMinimumKeyword(min));
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
			builder.Add(new FormatKeyword(Formats.Get(format)));
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
			builder.Add(new FormatKeyword(format));
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
			builder.Add(new IdKeyword(id));
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
			builder.Add(new IdKeyword(new Uri(id, UriKind.RelativeOrAbsolute)));
			return builder;
		}

		/// <summary>
		/// Add an `if` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `if`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder If(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new IfKeyword(schema));
			return builder;
		}

		/// <summary>
		/// Add a single-schema `items` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `items`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Items(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new ItemsKeyword(schema));
			return builder;
		}

		/// <summary>
		/// Add a schema-array `items` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schemas for `items`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Items(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new ItemsKeyword(schemas));
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
			builder.Add(new MaxContainsKeyword(value));
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
			builder.Add(new MaximumKeyword(value));
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
			builder.Add(new MaxItemsKeyword(value));
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
			builder.Add(new MaxLengthKeyword(value));
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
			builder.Add(new MaxPropertiesKeyword(value));
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
			builder.Add(new MinContainsKeyword(value));
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
			builder.Add(new MinimumKeyword(value));
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
			builder.Add(new MinItemsKeyword(value));
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
			builder.Add(new MinLengthKeyword(value));
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
			builder.Add(new MinPropertiesKeyword(value));
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
			builder.Add(new MultipleOfKeyword(value));
			return builder;
		}

		/// <summary>
		/// Add a `not` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `not`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Not(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new NotKeyword(schema));
			return builder;
		}

		/// <summary>
		/// Add a `oneOf` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schema for `oneOf`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder OneOf(this JsonSchemaBuilder builder, params JsonSchema[] schemas)
		{
			builder.Add(new OneOfKeyword(schemas));
			return builder;
		}

		/// <summary>
		/// Add a `oneOf` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schemas for `oneOf`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder OneOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new OneOfKeyword(schemas));
			return builder;
		}

		/// <summary>
		/// Add a `pattern` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="pattern">The pattern to match.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Pattern(this JsonSchemaBuilder builder, Regex pattern)
		{
			builder.Add(new PatternKeyword(pattern));
			return builder;
		}

		/// <summary>
		/// Add a `pattern` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="pattern">The pattern to match.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Pattern(this JsonSchemaBuilder builder, [RegexPattern] string pattern)
		{
			builder.Add(new PatternKeyword(new Regex(pattern, RegexOptions.ECMAScript | RegexOptions.Compiled)));
			return builder;
		}

		/// <summary>
		/// Add a `patternProperties` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="props">The property schemas.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder PatternProperties(this JsonSchemaBuilder builder, IReadOnlyDictionary<Regex, JsonSchema> props)
		{
			builder.Add(new PatternPropertiesKeyword(props));
			return builder;
		}

		/// <summary>
		/// Add a `patternProperties` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="props">The property schemas.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder PatternProperties(this JsonSchemaBuilder builder, params (Regex pattern, JsonSchema schema)[] props)
		{
			builder.Add(new PatternPropertiesKeyword(props.ToDictionary(x => x.pattern, x => x.schema)));
			return builder;
		}

		/// <summary>
		/// Add a `prefixItems` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schemas for `prefixItems`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder PrefixItems(this JsonSchemaBuilder builder, params JsonSchema[] schemas)
		{
			builder.Add(new PrefixItemsKeyword(schemas));
			return builder;
		}

		/// <summary>
		/// Add a `prefixItems` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schemas">The schemas for `prefixItems`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder PrefixItems(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new PrefixItemsKeyword(schemas));
			return builder;
		}

		/// <summary>
		/// Add a `properties` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="props">The property schemas.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Properties(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchema> props)
		{
			builder.Add(new PropertiesKeyword(props));
			return builder;
		}

		/// <summary>
		/// Add a `properties` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="props">The property schemas.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Properties(this JsonSchemaBuilder builder, params (string name, JsonSchema schema)[] props)
		{
			builder.Add(new PropertiesKeyword(props.ToDictionary(x => x.name, x => x.schema)));
			return builder;
		}

		/// <summary>
		/// Add a `propertyNames` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `propertyNames`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder PropertyNames(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new PropertyNamesKeyword(schema));
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
			builder.Add(new ReadOnlyKeyword(value));
			return builder;
		}

		/// <summary>
		/// Add a `$recursiveAnchor` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="value">The value.</param>
		/// <returns>The builder.</returns>
		/// <remarks>
		/// Per Draft 2019-09, the value must always be <code>true</code>.  This is implied for this method.
		/// </remarks>
		public static JsonSchemaBuilder RecursiveAnchor(this JsonSchemaBuilder builder, bool value = true)
		{
			builder.Add(new RecursiveAnchorKeyword(value));
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
			builder.Add(new RecursiveRefKeyword(reference));
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
			builder.Add(new RecursiveRefKeyword(new Uri(reference, UriKind.RelativeOrAbsolute)));
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
			builder.Add(new RefKeyword(reference));
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
			builder.Add(new RefKeyword(new Uri(reference, UriKind.RelativeOrAbsolute)));
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
			builder.Add(new RequiredKeyword(properties));
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
			builder.Add(new RequiredKeyword(properties));
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
			builder.Add(new SchemaKeyword(uri));
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
			builder.Add(new SchemaKeyword(new Uri(uri, UriKind.Absolute)));
			return builder;
		}

		/// <summary>
		/// Add a `then` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `then`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder Then(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new ThenKeyword(schema));
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
			builder.Add(new TitleKeyword(title));
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
			builder.Add(new TypeKeyword(type));
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
			builder.Add(new TypeKeyword(types));
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
			builder.Add(new TypeKeyword(types));
			return builder;
		}

		/// <summary>
		/// Add an `unevaluatedItems` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `unevaluatedItems`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder UnevaluatedItems(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new UnevaluatedItemsKeyword(schema));
			return builder;
		}

		/// <summary>
		/// Add an `unevaluatedProperties` keyword.
		/// </summary>
		/// <param name="builder">The builder.</param>
		/// <param name="schema">The schema for `unevaluatedProperties`.</param>
		/// <returns>The builder.</returns>
		public static JsonSchemaBuilder UnevaluatedProperties(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new UnevaluatedPropertiesKeyword(schema));
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
			builder.Add(new UniqueItemsKeyword(value));
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
			builder.Add(new VocabularyKeyword(vocabs.ToDictionary(x => x.id, x => x.required)));
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
			builder.Add(new VocabularyKeyword(vocabs.ToDictionary(x => new Uri(x.id, UriKind.Absolute), x => x.required)));
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
			builder.Add(new VocabularyKeyword(vocabs));
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
			builder.Add(new VocabularyKeyword(vocabs.ToDictionary(x => new Uri(x.Key, UriKind.Absolute), x => x.Value)));
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
			builder.Add(new WriteOnlyKeyword(value));
			return builder;
		}
	}
}