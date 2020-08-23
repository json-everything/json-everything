using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Json.Schema
{
	public static class JsonSchemaBuilderExtensions
	{
		public static JsonSchemaBuilder AdditionalItems(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new AdditionalItemsKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder AdditionalProperties(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new AdditionalPropertiesKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder AllOf(this JsonSchemaBuilder builder, params JsonSchema[] schemas)
		{
			builder.Add(new AllOfKeyword(schemas));
			return builder;
		}

		public static JsonSchemaBuilder AllOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new AllOfKeyword(schemas));
			return builder;
		}

		public static JsonSchemaBuilder AnyOf(this JsonSchemaBuilder builder, params JsonSchema[] schemas)
		{
			builder.Add(new AnyOfKeyword(schemas));
			return builder;
		}

		public static JsonSchemaBuilder AnyOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new AnyOfKeyword(schemas));
			return builder;
		}

		public static JsonSchemaBuilder Comment(this JsonSchemaBuilder builder, string comment)
		{
			builder.Add(new CommentKeyword(comment));
			return builder;
		}

		public static JsonSchemaBuilder Const(this JsonSchemaBuilder builder, JsonElement element)
		{
			builder.Add(new ConstKeyword(element));
			return builder;
		}

		public static JsonSchemaBuilder Contains(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new ContainsKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder Default(this JsonSchemaBuilder builder, JsonElement element)
		{
			builder.Add(new DefaultKeyword(element));
			return builder;
		}

		public static JsonSchemaBuilder Definitions(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchema> defs)
		{
			builder.Add(new DefinitionsKeyword(defs));
			return builder;
		}

		public static JsonSchemaBuilder Definitions(this JsonSchemaBuilder builder, params (string name, JsonSchema schema)[] defs)
		{
			builder.Add(new DefinitionsKeyword(defs.ToDictionary(x => x.name, x => x.schema)));
			return builder;
		}

		public static JsonSchemaBuilder Defs(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchema> defs)
		{
			builder.Add(new DefsKeyword(defs));
			return builder;
		}

		public static JsonSchemaBuilder Defs(this JsonSchemaBuilder builder, params (string name, JsonSchema schema)[] defs)
		{
			builder.Add(new DefsKeyword(defs.ToDictionary(x => x.name, x => x.schema)));
			return builder;
		}

		public static JsonSchemaBuilder Dependencies(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, SchemaOrPropertyList> deps)
		{
			builder.Add(new DependenciesKeyword(deps));
			return builder;
		}

		public static JsonSchemaBuilder Dependencies(this JsonSchemaBuilder builder, params (string name, SchemaOrPropertyList dep)[] deps)
		{
			builder.Add(new DependenciesKeyword(deps.ToDictionary(x => x.name, x => x.dep)));
			return builder;
		}

		public static JsonSchemaBuilder DependentRequired(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, IReadOnlyList<string>> deps)
		{
			builder.Add(new DependentRequiredKeyword(deps));
			return builder;
		}

		public static JsonSchemaBuilder DependentRequired(this JsonSchemaBuilder builder, params (string name, IEnumerable<string> properties)[] deps)
		{
			builder.Add(new DependentRequiredKeyword(deps.ToDictionary(x => x.name, x => (IReadOnlyList<string>) x.properties.ToList())));
			return builder;
		}

		public static JsonSchemaBuilder DependentSchemas(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchema> deps)
		{
			builder.Add(new DependentSchemasKeyword(deps));
			return builder;
		}

		public static JsonSchemaBuilder DependentSchemas(this JsonSchemaBuilder builder, params (string name, JsonSchema schema)[] deps)
		{
			builder.Add(new DependentSchemasKeyword(deps.ToDictionary(x => x.name, x => x.schema)));
			return builder;
		}

		public static JsonSchemaBuilder Deprecated(this JsonSchemaBuilder builder, bool deprecated)
		{
			builder.Add(new DeprecatedKeyword(deprecated));
			return builder;
		}

		public static JsonSchemaBuilder Description(this JsonSchemaBuilder builder, string description)
		{
			builder.Add(new DescriptionKeyword(description));
			return builder;
		}

		public static JsonSchemaBuilder Else(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new ElseKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, IEnumerable<JsonElement> elements)
		{
			builder.Add(new EnumKeyword(elements));
			return builder;
		}

		public static JsonSchemaBuilder Enum(this JsonSchemaBuilder builder, params JsonElement[] elements)
		{
			builder.Add(new EnumKeyword(elements));
			return builder;
		}

		public static JsonSchemaBuilder Examples(this JsonSchemaBuilder builder, params JsonElement[] elements)
		{
			builder.Add(new ExamplesKeyword(elements));
			return builder;
		}

		public static JsonSchemaBuilder ExclusiveMaximum(this JsonSchemaBuilder builder, decimal max)
		{
			builder.Add(new ExclusiveMaximumKeyword(max));
			return builder;
		}

		public static JsonSchemaBuilder ExclusiveMinimum(this JsonSchemaBuilder builder, decimal min)
		{
			builder.Add(new ExclusiveMinimumKeyword(min));
			return builder;
		}

		public static JsonSchemaBuilder Format(this JsonSchemaBuilder builder, Format format)
		{
			builder.Add(new FormatKeyword(format));
			return builder;
		}

		public static JsonSchemaBuilder Id(this JsonSchemaBuilder builder, Uri id)
		{
			builder.Add(new IdKeyword(id));
			return builder;
		}

		public static JsonSchemaBuilder Id(this JsonSchemaBuilder builder, string id)
		{
			builder.Add(new IdKeyword(new Uri(id, UriKind.RelativeOrAbsolute)));
			return builder;
		}

		public static JsonSchemaBuilder If(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new IfKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder Items(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new ItemsKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder Items(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new ItemsKeyword(schemas));
			return builder;
		}

		public static JsonSchemaBuilder MaxContains(this JsonSchemaBuilder builder, uint value)
		{
			builder.Add(new MaxContainsKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder Maximum(this JsonSchemaBuilder builder, decimal value)
		{
			builder.Add(new MaximumKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder MaxItems(this JsonSchemaBuilder builder, uint value)
		{
			builder.Add(new MaxItemsKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder MaxLength(this JsonSchemaBuilder builder, uint value)
		{
			builder.Add(new MaxLengthKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder MaxProperties(this JsonSchemaBuilder builder, uint value)
		{
			builder.Add(new MaxPropertiesKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder MinContains(this JsonSchemaBuilder builder, uint value)
		{
			builder.Add(new MinContainsKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder Minimum(this JsonSchemaBuilder builder, decimal value)
		{
			builder.Add(new MinimumKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder MinItems(this JsonSchemaBuilder builder, uint value)
		{
			builder.Add(new MinItemsKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder MinLength(this JsonSchemaBuilder builder, uint value)
		{
			builder.Add(new MinLengthKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder MinProperties(this JsonSchemaBuilder builder, uint value)
		{
			builder.Add(new MinPropertiesKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder MultipleOf(this JsonSchemaBuilder builder, decimal value)
		{
			builder.Add(new MultipleOfKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder Not(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new NotKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder OneOf(this JsonSchemaBuilder builder, params JsonSchema[] schemas)
		{
			builder.Add(new OneOfKeyword(schemas));
			return builder;
		}

		public static JsonSchemaBuilder OneOf(this JsonSchemaBuilder builder, IEnumerable<JsonSchema> schemas)
		{
			builder.Add(new OneOfKeyword(schemas));
			return builder;
		}

		public static JsonSchemaBuilder Pattern(this JsonSchemaBuilder builder, Regex pattern)
		{
			builder.Add(new PatternKeyword(pattern));
			return builder;
		}

		public static JsonSchemaBuilder Pattern(this JsonSchemaBuilder builder, [RegexPattern] string pattern)
		{
			builder.Add(new PatternKeyword(new Regex(pattern, RegexOptions.ECMAScript | RegexOptions.Compiled)));
			return builder;
		}

		public static JsonSchemaBuilder PatternProperties(this JsonSchemaBuilder builder, IReadOnlyDictionary<Regex, JsonSchema> props)
		{
			builder.Add(new PatternPropertiesKeyword(props));
			return builder;
		}

		public static JsonSchemaBuilder PatternProperties(this JsonSchemaBuilder builder, params (Regex pattern, JsonSchema schema)[] props)
		{
			builder.Add(new PatternPropertiesKeyword(props.ToDictionary(x => x.pattern, x => x.schema)));
			return builder;
		}

		public static JsonSchemaBuilder Properties(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, JsonSchema> props)
		{
			builder.Add(new PropertiesKeyword(props));
			return builder;
		}

		public static JsonSchemaBuilder Properties(this JsonSchemaBuilder builder, params (string name, JsonSchema schema)[] props)
		{
			builder.Add(new PropertiesKeyword(props.ToDictionary(x => x.name, x => x.schema)));
			return builder;
		}

		public static JsonSchemaBuilder PropertyNames(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new PropertyNamesKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder ReadOnly(this JsonSchemaBuilder builder, bool value)
		{
			builder.Add(new ReadOnlyKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder RecursiveAnchor(this JsonSchemaBuilder builder, bool value)
		{
			builder.Add(new RecursiveAnchorKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder RecursiveRef(this JsonSchemaBuilder builder, Uri reference)
		{
			builder.Add(new RecursiveRefKeyword(reference));
			return builder;
		}

		public static JsonSchemaBuilder RecursiveRef(this JsonSchemaBuilder builder, string reference)
		{
			builder.Add(new RecursiveRefKeyword(new Uri(reference, UriKind.RelativeOrAbsolute)));
			return builder;
		}

		public static JsonSchemaBuilder Ref(this JsonSchemaBuilder builder, Uri reference)
		{
			builder.Add(new RefKeyword(reference));
			return builder;
		}

		public static JsonSchemaBuilder Ref(this JsonSchemaBuilder builder, string reference)
		{
			builder.Add(new RefKeyword(new Uri(reference, UriKind.RelativeOrAbsolute)));
			return builder;
		}

		public static JsonSchemaBuilder Required(this JsonSchemaBuilder builder, IEnumerable<string> properties)
		{
			builder.Add(new RequiredKeyword(properties));
			return builder;
		}

		public static JsonSchemaBuilder Required(this JsonSchemaBuilder builder, params string[] properties)
		{
			builder.Add(new RequiredKeyword(properties));
			return builder;
		}

		public static JsonSchemaBuilder Schema(this JsonSchemaBuilder builder, Uri uri)
		{
			builder.Add(new SchemaKeyword(uri));
			return builder;
		}

		public static JsonSchemaBuilder Schema(this JsonSchemaBuilder builder, string uri)
		{
			builder.Add(new SchemaKeyword(new Uri(uri, UriKind.Absolute)));
			return builder;
		}

		public static JsonSchemaBuilder Then(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new ThenKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder Title(this JsonSchemaBuilder builder, string title)
		{
			builder.Add(new TitleKeyword(title));
			return builder;
		}

		public static JsonSchemaBuilder Type(this JsonSchemaBuilder builder, SchemaValueType type)
		{
			builder.Add(new TypeKeyword(type));
			return builder;
		}

		public static JsonSchemaBuilder Type(this JsonSchemaBuilder builder, params SchemaValueType[] types)
		{
			builder.Add(new TypeKeyword(types));
			return builder;
		}

		public static JsonSchemaBuilder Type(this JsonSchemaBuilder builder, IEnumerable<SchemaValueType> types)
		{
			builder.Add(new TypeKeyword(types));
			return builder;
		}

		public static JsonSchemaBuilder UnevaluatedItems(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new UnevaluatedItemsKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder UnevaluatedProperties(this JsonSchemaBuilder builder, JsonSchema schema)
		{
			builder.Add(new UnevaluatedPropertiesKeyword(schema));
			return builder;
		}

		public static JsonSchemaBuilder UniqueItems(this JsonSchemaBuilder builder, bool value)
		{
			builder.Add(new UniqueItemsKeyword(value));
			return builder;
		}

		public static JsonSchemaBuilder Vocabulary(this JsonSchemaBuilder builder, params (Uri id, bool required)[] vocabs)
		{
			builder.Add(new VocabularyKeyword(vocabs.ToDictionary(x => x.id, x => x.required)));
			return builder;
		}

		public static JsonSchemaBuilder Vocabulary(this JsonSchemaBuilder builder, params (string id, bool required)[] vocabs)
		{
			builder.Add(new VocabularyKeyword(vocabs.ToDictionary(x => new Uri(x.id, UriKind.Absolute), x => x.required)));
			return builder;
		}

		public static JsonSchemaBuilder Vocabulary(this JsonSchemaBuilder builder, IReadOnlyDictionary<Uri, bool> vocabs)
		{
			builder.Add(new VocabularyKeyword(vocabs));
			return builder;
		}

		public static JsonSchemaBuilder Vocabulary(this JsonSchemaBuilder builder, IReadOnlyDictionary<string, bool> vocabs)
		{
			builder.Add(new VocabularyKeyword(vocabs.ToDictionary(x => new Uri(x.Key, UriKind.Absolute), x => x.Value)));
			return builder;
		}

		public static JsonSchemaBuilder WriteOnly(this JsonSchemaBuilder builder, bool value)
		{
			builder.Add(new WriteOnlyKeyword(value));
			return builder;
		}
	}
}