using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Json.Schema;

public interface IKeywordHandler
{
    void Build(
        JsonElement value,
        JsonSchema.BuildContext context,
        Dictionary<string, Func<JsonElement, List<EvaluationOutput>, KeywordResult>> constraints,
        Dictionary<string, List<JsonSchemaNode>> dependencies
    );
}