using System.Text.Json.Serialization;
using LiveChartsCore;

namespace JsonEverythingNet.ViewModels;

public class FeaturedSponsorData
{
	private readonly IReadOnlyList<SponsorData> _sponsorData;

	public ISeries[] Series { get; set; }

	public FeaturedSponsorData(IReadOnlyList<SponsorData> sponsorData)
	{
		_sponsorData = sponsorData;
	}

	public void Initialize(string fileContent)
	{

	}
}

[JsonConverter(typeof(JsonStringEnumConverter<BubbleSize>))]
public enum BubbleSize
{
	None,
	Small,
	Medium,
	Large
}

public record SponsorData
{
	public string Username { get; init; }
	public Uri AvatarUrl { get; init; }
	public Uri WebsiteUrl { get; init; }
	public BubbleSize BubbleSize { get; init; }
	public double X { get; init; }
	public double T { get; init; }
}