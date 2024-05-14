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
var featuredSponsorsJson = JsonSerializer.Serialize(sponsorsWithBubbles, options);
Console.WriteLine(featuredSponsorsJson);

File.WriteAllText("sponsor-data.json", featuredSponsorsJson);

return;

static BubbleSize GetBubbleSize(int value)
{
	if (value < 100) return BubbleSize.None;
	if (value < 250) return BubbleSize.Small;
	if (value < 500) return BubbleSize.Medium;
	return BubbleSize.Large;
}

[JsonConverter(typeof(JsonStringEnumConverter<BubbleSize>))]
enum BubbleSize
{
	None,
	Small,
	Medium,
	Large
}

record SponsorData(string Username, Uri AvatarUrl, Uri WebsiteUrl, BubbleSize BubbleSize);