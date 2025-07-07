using System.Collections.Generic;

namespace Json.Schema;

public static class KeywordHandlers
{
    public static Dictionary<string, IKeywordHandler> Handlers { get; } =
        new()
        {
            ["type"] = new TypeHandler(),
            ["anyOf"] = new AnyOfHandler(),
            ["properties"] = new PropertiesHandler(),
            ["$ref"] = new RefHandler(),
            ["items"] = new ItemsHandler(),
            ["minimum"] = new MinimumHandler(),
            ["maximum"] = new MaximumHandler(),
            ["exclusiveMinimum"] = new ExclusiveMinimumHandler(),
            ["exclusiveMaximum"] = new ExclusiveMaximumHandler(),
            ["multipleOf"] = new MultipleOfHandler(),
            ["minLength"] = new MinLengthHandler(),
            ["maxLength"] = new MaxLengthHandler(),
            ["pattern"] = new PatternHandler(),
            ["allOf"] = new AllOfHandler(),
            ["oneOf"] = new OneOfHandler(),
            ["not"] = new NotHandler(),
            ["required"] = new RequiredHandler(),
            ["additionalProperties"] = new AdditionalPropertiesHandler(),
            ["minProperties"] = new MinPropertiesHandler(),
            ["maxProperties"] = new MaxPropertiesHandler(),
            ["minItems"] = new MinItemsHandler(),
            ["maxItems"] = new MaxItemsHandler(),
            ["contains"] = new ContainsHandler(),
            ["uniqueItems"] = new UniqueItemsHandler(),
            ["prefixItems"] = new PrefixItemsHandler(),
            ["additionalItems"] = new AdditionalItemsHandler(),
            ["enum"] = new EnumHandler(),
            ["const"] = new ConstHandler(),
            ["propertyNames"] = new PropertyNamesHandler(),
            ["patternProperties"] = new PatternPropertiesHandler(),
            ["dependentSchemas"] = new DependentSchemasHandler(),
            ["dependentRequired"] = new DependentRequiredHandler(),
            ["dependencies"] = new DependenciesHandler(),
            ["if"] = new ConditionalHandler()
        };
} 