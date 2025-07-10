using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema.Ast;

public class PatternPropertiesHandler : IKeywordHandler
{
    public string Keyword => "patternProperties";

    public void BuildAst(JsonElement keywordValue, BuildContext context, Dictionary<string, object> properties)
    {
        if (keywordValue.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'patternProperties' keyword value: expected object, found {keywordValue.ValueKind}");

        properties["patternProperties"] = keywordValue;
        
        // Store regex patterns for evaluation
        var patterns = new List<string>();
        foreach (var property in keywordValue.EnumerateObject())
        {
            try
            {
                var regex = new Regex(property.Name, RegexOptions.ECMAScript);
                patterns.Add(property.Name);
            }
            catch (ArgumentException ex)
            {
                throw new JsonSchemaException($"Invalid pattern '{property.Name}' in patternProperties: {ex.Message}");
            }
        }
        
        properties["patternProperties.patterns"] = patterns;
    }

    public KeywordResult Evaluate(JsonSchemaNode node, JsonElement instance, List<EvaluationOutput> dependencies)
    {
        if (instance.ValueKind != JsonValueKind.Object)
            return KeywordResult.Skip;

        if (!node.Properties.TryGetValue("patternProperties.patterns", out var patternsObj) || 
            patternsObj is not List<string> patterns)
            return KeywordResult.Skip;

        // Create regex patterns for matching
        var patternRegexes = patterns.Select(p => new { Pattern = p, Regex = new Regex(p, RegexOptions.ECMAScript) }).ToList();

        // Group dependencies by the pattern they match
        var dependenciesByPattern = new Dictionary<string, List<EvaluationOutput>>();
        foreach (var dep in dependencies)
        {
            var location = dep.InstanceLocation;
            if (location.SegmentCount == 0) continue;
            
            var propertyName = location.GetSegment(location.SegmentCount - 1).ToString();
            
            // Find which pattern this property matches
            foreach (var patternInfo in patternRegexes)
            {
                if (patternInfo.Regex.IsMatch(propertyName))
                {
                    var pattern = patternInfo.Pattern;
                    if (!dependenciesByPattern.ContainsKey(pattern))
                        dependenciesByPattern[pattern] = new List<EvaluationOutput>();
                    dependenciesByPattern[pattern].Add(dep);
                }
            }
        }

        // Check that all properties matching patterns passed validation
        var allValid = true;
        foreach (var pattern in patterns)
        {
            if (dependenciesByPattern.TryGetValue(pattern, out var patternDeps))
            {
                if (!patternDeps.All(dep => dep.IsValid))
                {
                    allValid = false;
                    break;
                }
            }
        }

        return new KeywordResult(
            allValid,
            allValid ? null : "All properties matching patterns must match their respective schemas",
            null
        );
    }

    public IEnumerable<ChildSpec> GetChildSpecs(JsonElement keywordValue, BuildContext context)
    {
        if (keywordValue.ValueKind != JsonValueKind.Object)
            throw new JsonSchemaException($"Invalid 'patternProperties' keyword value: expected object, found {keywordValue.ValueKind}");

        // Create wildcard child specs for properties matching patterns
        foreach (var property in keywordValue.EnumerateObject())
        {
            var pattern = property.Name;
            yield return new ChildSpec(
                InstancePath: JsonPointerHelpers.Wildcard, // Use wildcard for property matching
                SchemaPath: JsonPointer.Create(pattern),
                SubSchema: property.Value
            );
        }
    }
} 