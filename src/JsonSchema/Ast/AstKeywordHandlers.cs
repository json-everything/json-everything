using System.Collections.Generic;

namespace Json.Schema.Ast;

public static class KeywordHandlers
{
    public static Dictionary<string, IKeywordHandler> Handlers { get; } = new()
    {
        ["type"] = new TypeHandler(),
        ["properties"] = new PropertiesHandler(),
        ["required"] = new RequiredHandler(),
        ["minimum"] = new MinimumHandler(),
        ["items"] = new ItemsHandler(),
        ["$ref"] = new RefHandler(),
        ["allOf"] = new AllOfHandler()
    };
} 