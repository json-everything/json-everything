// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using GenerateSponsorList;
using Microsoft.Extensions.DependencyInjection;

var accessToken = Environment.GetEnvironmentVariable("GenerateSponsorList");
if (string.IsNullOrEmpty(accessToken))
	throw new ArgumentException("Cannot locate access token in env var `GenerateSponsorList`");

var serviceCollection = new ServiceCollection();

serviceCollection
	.AddGitHubClient()
	.ConfigureHttpClient(client =>
	{
		client.BaseAddress = new Uri("https://api.github.com/graphql");
		client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue("Bearer", accessToken);
	});

IServiceProvider services = serviceCollection.BuildServiceProvider();

var client = services.GetRequiredService<IGitHubClient>();
var response = await client.GetSponsors.ExecuteAsync();

var options = new JsonSerializerOptions
{
	IncludeFields = true,
	PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	WriteIndented = true
};
var sponsorData = response.Data!.User!.Sponsors.Nodes!.Select(x =>
{
	string? name = null;
	Uri? avatar = null;
	Uri? website = null;
	int value = 0;
	if (x is IGetSponsors_User_Sponsors_Nodes_User { Login: not null } user)
	{
		name = user.Login;
		avatar = user.AvatarUrl;
		website = user.WebsiteUrl;
		value = user.SponsorshipForViewerAsSponsorable!.Tier!.MonthlyPriceInDollars;
	}
	else if (x is IGetSponsors_User_Sponsors_Nodes_Organization { Login: not null } org)
	{
		name = org.Login;
		avatar = org.AvatarUrl;
		website = org.WebsiteUrl;
		value = org.SponsorshipForViewerAsSponsorable!.Tier!.MonthlyPriceInDollars;
	}

	return new SponsorData (name!, avatar!, website!, GetBubbleSize(value));
}).ToList();
var allSponsorsJson = JsonSerializer.Serialize(sponsorData, options);
Console.WriteLine(allSponsorsJson);

var sponsorsWithBubbles = sponsorData.Where(x => x.BubbleSize != BubbleSize.None).ToList();
Arrange(sponsorsWithBubbles);
var featuredSponsorsJson = JsonSerializer.Serialize(sponsorsWithBubbles, options);
Console.WriteLine(featuredSponsorsJson);

File.WriteAllText("sponsor-data.json", featuredSponsorsJson);

return;

static BubbleSize GetBubbleSize(int value)
{
	return BubbleSize.Medium;

	if (value < 100) return BubbleSize.None;
	if (value < 250) return BubbleSize.Small;
	if (value < 500) return BubbleSize.Medium;
	return BubbleSize.Large;
}

static int GetBubbleRadius(BubbleSize x) => x switch
{
	BubbleSize.Small => 15,
	BubbleSize.Medium => 30,
	BubbleSize.Large => 60,
	_ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
};

static void Arrange(List<SponsorData> data)
{

	bool Overlaps(SponsorData item, double x, double y, HashSet<SponsorData> field) =>
		field.Any(sd =>
		{
			const int padding = 5;
			var fieldRadius = GetBubbleRadius(sd.BubbleSize);
			var itemRadius = GetBubbleRadius(item.BubbleSize);
			var requiredDistance = fieldRadius + padding + itemRadius;
			var actualDistance = Math.Sqrt((sd.X!.Value - x) * (sd.X.Value - x) + (sd.Y!.Value - y) * (sd.Y.Value - y));

			return actualDistance < requiredDistance;
		});

	var sorted = data.OrderByDescending(x => x.BubbleSize).ToList();
	if (sorted.Count != 0)
	{
		sorted[0].X = 0;
		sorted[0].Y = 0;
	}
	var toPosition = new Queue<SponsorData>(sorted.Skip(1));
	var positioned = new HashSet<SponsorData>(sorted.Take(1));

	while (toPosition.Count != 0)
	{
		var item = toPosition.Dequeue();
		int radius = 1;
		var done = false;
		while (!done)
		{
			for (double angle = 0; angle < 2* Math.PI; angle += Math.PI/36)  // 5 degrees
			{
				var x = radius * Math.Cos(angle);
				var y = radius * Math.Sin(angle);
				if (!Overlaps(item, x, y, positioned))
				{
					item.X = (int) x;
					item.Y = (int) y;
					done = true;
					break;
				}
			}

			radius++;
		}

		positioned.Add(item);
		Reposition(positioned);
	}
}

static void Reposition(HashSet<SponsorData> field)
{
	var minX = field.Select(x => x.X - GetBubbleRadius(x.BubbleSize)).Min();
	var minY = field.Select(x => x.Y - GetBubbleRadius(x.BubbleSize)).Min();
	var maxX = field.Select(x => x.X + GetBubbleRadius(x.BubbleSize)).Max();
	var maxY = field.Select(x => x.Y + GetBubbleRadius(x.BubbleSize)).Max();

	var deltaX = (maxX + minX) / 2;
	var deltaY = (maxY + minY) / 2;

	foreach (var data in field)
	{
		data.X -= deltaX;
		data.Y -= deltaY;
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

public record SponsorData(string Username, Uri AvatarUrl, Uri WebsiteUrl, BubbleSize BubbleSize)
{
	public int? X { get; set; }
	public int? Y { get; set; }
}
