using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.Schema.Api.OpenApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Json.Schema.Api.Tests.OpenApi;

/// <summary>
/// Verifies that the description is written to disk at startup.
/// </summary>
public class OutputPathTests
{
	private string _directory = null!;

	[SetUp]
	public void SetUp()
	{
		_directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(_directory))
			Directory.Delete(_directory, true);
	}

	private WebApplicationFactory<Program> CreateHost(OpenApiFormats formats)
	{
		return new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddControllers();
					services.AddOpenApi(c =>
					{
						c.DocumentFormats = formats;
						c.InteractivePath = null;
						c.FileOutputPath = Path.Combine(_directory, "openapi");
					});
				});
			});
	}

	[Test]
	public async Task BothFormatsAreWritten()
	{
		await using var factory = CreateHost(OpenApiFormats.Json | OpenApiFormats.Yaml);
		using var client = factory.CreateClient();

		Assert.That(File.Exists(Path.Combine(_directory, "openapi.json")), Is.True, "json");
		Assert.That(File.Exists(Path.Combine(_directory, "openapi.yaml")), Is.True, "yaml");
	}

	[Test]
	public async Task OnlyTheConfiguredFormatIsWritten()
	{
		await using var factory = CreateHost(OpenApiFormats.Json);
		using var client = factory.CreateClient();

		Assert.That(File.Exists(Path.Combine(_directory, "openapi.json")), Is.True, "json");
		Assert.That(File.Exists(Path.Combine(_directory, "openapi.yaml")), Is.False, "yaml");
	}

	[Test]
	public async Task TheWrittenDescriptionIsTheServedOne()
	{
		await using var factory = CreateHost(OpenApiFormats.Json);
		using var client = factory.CreateClient();

		var written = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(_directory, "openapi.json")))!;
		var served = JsonNode.Parse(await client.GetStringAsync("/openapi.json"))!;

		Assert.That(written.ToJsonString(), Is.EqualTo(served.ToJsonString()));
	}

	[Test]
	public async Task NothingIsWrittenWithoutAnOutputPath()
	{
		await using var factory = new WebApplicationFactory<Program>();
		using var client = factory.CreateClient();

		Assert.That(Directory.Exists(_directory), Is.False);
	}
}
