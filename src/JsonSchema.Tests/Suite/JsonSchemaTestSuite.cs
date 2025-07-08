using System.Text.Json;

namespace Json.Schema.Tests.Suite;

public class SchemaTest
{
    public string Description { get; set; } = string.Empty;
    public JsonElement Schema { get; set; }
    public ValidationCase[] Tests { get; set; } = [];
}

public class ValidationCase
{
    public string Description { get; set; } = string.Empty;
    public JsonElement Data { get; set; }
    public bool Valid { get; set; }
}

public class TestScenario
{
    public string DraftVersion { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string TestDescription { get; set; } = string.Empty;
    public string CaseDescription { get; set; } = string.Empty;
    public JsonElement Schema { get; set; }
    public JsonElement Data { get; set; }
    public bool ExpectedValid { get; set; }
    public bool IsOptional { get; set; }

    public override string ToString() => $"{DraftVersion} / {FileName} / {TestDescription} / {CaseDescription}";
} 