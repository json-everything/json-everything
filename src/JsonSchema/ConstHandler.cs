using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Json.Schema;

public sealed class ConstHandler : IKeywordHandler
{
    public void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    )
    {
        var constValue = value;
        
        constraints["const"] = (instance, _) =>
        {
            if (instance.DeepEquals(constValue))
            {
                return new KeywordResult(true, null, null);
            }

            return new KeywordResult(
                false,
                "Value must exactly match the specified constant value",
                null
            );
        };
    }
} 