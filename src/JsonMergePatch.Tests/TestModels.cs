using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Json.MergePatch.Tests;

[GenerateMergePatchUpdate]
public partial class WeatherForecast
{
	public int Temperature { get; set; }
	public string? Summary { get; set; }
	public Location? Location { get; set; }
}

[GenerateMergePatchUpdate]
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
