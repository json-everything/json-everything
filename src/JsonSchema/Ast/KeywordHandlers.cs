using System.Collections.Generic;

namespace Json.Schema.Ast;

public static class KeywordHandlers
{
    public static Dictionary<string, IKeywordHandler> Handlers { get; } = new()
    {
        ["additionalItems"] = new AdditionalItemsHandler(),
        ["additionalProperties"] = new AdditionalPropertiesHandler(),
        ["allOf"] = new AllOfHandler(),
        ["anyOf"] = new AnyOfHandler(),
        ["const"] = new ConstHandler(),
        ["contains"] = new ContainsHandler(),
        ["dependencies"] = new DependenciesHandler(),
        ["dependentRequired"] = new DependentRequiredHandler(),
        ["dependentSchemas"] = new DependentSchemasHandler(),
        ["enum"] = new EnumHandler(),
        ["exclusiveMaximum"] = new ExclusiveMaximumHandler(),
        ["exclusiveMinimum"] = new ExclusiveMinimumHandler(),
        ["items"] = new ItemsHandler(),
        ["maximum"] = new MaximumHandler(),
        ["maxItems"] = new MaxItemsHandler(),
        ["maxLength"] = new MaxLengthHandler(),
        ["maxProperties"] = new MaxPropertiesHandler(),
        ["minimum"] = new MinimumHandler(),
        ["minItems"] = new MinItemsHandler(),
        ["minLength"] = new MinLengthHandler(),
        ["minProperties"] = new MinPropertiesHandler(),
        ["multipleOf"] = new MultipleOfHandler(),
        ["not"] = new NotHandler(),
        ["oneOf"] = new OneOfHandler(),
        ["pattern"] = new PatternHandler(),
        ["patternProperties"] = new PatternPropertiesHandler(),
        ["prefixItems"] = new PrefixItemsHandler(),
        ["properties"] = new PropertiesHandler(),
        ["propertyNames"] = new PropertyNamesHandler(),
        ["required"] = new RequiredHandler(),
        ["$ref"] = new RefHandler(),
        ["type"] = new TypeHandler(),
        ["uniqueItems"] = new UniqueItemsHandler()
    };
} 