#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace JsonEverythingNet.Models;

public record SponsorData
{
	public string? Username { get; set; }
	public Uri? AvatarUrl { get; set; }
	public Uri? WebsiteUrl { get; set; }
	public BubbleSize BubbleSize { get; set; }
	public double X { get; set; }
	public double Y { get; set; }

	public byte[]? ImageData { get; set; }
}
