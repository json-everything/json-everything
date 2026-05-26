using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Json.MergePatch.Tests;

[GenerateMergePatchUpdate(Nesting = NestingMode.Automatic)]
public partial class AutoNestingModel
{
	public int Value { get; set; }
}

[GenerateMergePatchUpdate(Nesting = NestingMode.Subclass)]
public partial class SubclassNestingModel
{
	public int Value { get; set; }
}

[GenerateMergePatchUpdate(Nesting = NestingMode.SameNamespace)]
public class NamespaceNestingModel
{
	public int Value { get; set; }
}

// This should trigger a diagnostic in Subclass mode (not partial)
[GenerateMergePatchUpdate(Nesting = NestingMode.Subclass)]
public class InvalidSubclassNestingModel
{
	public int Value { get; set; }
}

[GenerateMergePatchUpdate]
public partial class WeatherForecast
{
	public int Temperature { get; set; }
	public string? Summary { get; set; }
	public Location? Location { get; set; }
	[PatchIgnore]
	public string? InternalNote { get; set; }
}

[GenerateMergePatchUpdate(Name = "LocationPatch")]
public partial class Location
{
	public string? City { get; set; }
	[JsonPropertyName("zip")]
	public string? PostalCode { get; set; }
}

[GenerateMergePatchUpdate]
public partial class CollectionContainer
{
	public int[]? IntArray { get; set; }
	public List<int>? IntList { get; set; }
	public HashSet<string>? Tags { get; set; }
	public List<Location>? Locations { get; set; }
}
