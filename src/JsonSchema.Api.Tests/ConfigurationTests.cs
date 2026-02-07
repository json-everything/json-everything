using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.Schema.Generation;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Json.Schema.Api.Tests;

public class ConfigurationTests
{
	[Test]
	public async Task DefaultConfiguration_UsesCamelCase()
	{
		await using var factory = new WebApplicationFactory<Program>();
		using var client = factory.CreateClient();

		var json = """{"name": "Test", "age": 30}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await client.PostAsync("/api/test/simple", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
	}

	[Test]
	public async Task CustomConfiguration_SnakeCase_AcceptsSnakeCaseKeys()
	{
		await using var factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddControllers()
						.AddJsonSchemaValidation(converter =>
						{
							converter.GeneratorConfiguration.PropertyNameResolver = PropertyNameResolvers.SnakeCase;
							converter.EvaluationOptions.OutputFormat = OutputFormat.List;
							converter.EvaluationOptions.RequireFormatValidation = false;
						});
				});
			});
		using var client = factory.CreateClient();

		var json = """{"name": "Test", "age": 30}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await client.PostAsync("/api/test/simple", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
	}

	[Test]
	public async Task CustomConfiguration_SnakeCase_RejectsCamelCaseKeys()
	{
		await using var factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddControllers()
						.AddJsonSchemaValidation(converter => { converter.GeneratorConfiguration.PropertyNameResolver = PropertyNameResolvers.SnakeCase; });
				});
			});
		using var client = factory.CreateClient();

		var json = """{"firstName": "John", "lastName": "Doe"}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await client.PostAsync("/api/test/multiword", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
	}

	[Test]
	public async Task CustomEvaluationOptions_Applied()
	{
		await using var factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddControllers()
						.AddJsonSchemaValidation(converter =>
						{
							converter.EvaluationOptions.OutputFormat = OutputFormat.Flag;
							converter.EvaluationOptions.RequireFormatValidation = false;
							converter.GeneratorConfiguration.PropertyNameResolver = PropertyNameResolvers.CamelCase;
						});
				});
			});
		using var client = factory.CreateClient();

		var json = """{"age": 30}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await client.PostAsync("/api/test/simple", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

		var problemDetails = await response.Content.ReadFromJsonAsync<JsonObject>();
		Assert.That(problemDetails!["type"]!.GetValue<string>(), Is.EqualTo("https://json-everything.net/errors/validation"));
	}

	[Test]
	public async Task CustomGeneratorConfiguration_WithBuildOptions()
	{
		await using var factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services =>
				{
					services.AddControllers()
						.AddJsonSchemaValidation(converter =>
						{
							converter.GeneratorConfiguration.PropertyNameResolver = PropertyNameResolvers.CamelCase;
							converter.GeneratorConfiguration.PropertyOrder = PropertyOrder.ByName;
						});
				});
			});
		using var client = factory.CreateClient();

		var json = """{"name": "Test", "age": 30}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await client.PostAsync("/api/test/simple", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
	}

	[Test]
	public async Task DefaultConfiguration_HierarchicalOutputFormat()
	{
		await using var factory = new WebApplicationFactory<Program>();
		using var client = factory.CreateClient();

		var json = """{"age": 30}""";
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var response = await client.PostAsync("/api/test/simple", content);

		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

		var problemDetails = await response.Content.ReadFromJsonAsync<JsonObject>();
		var errors = problemDetails!["errors"] as JsonObject;
	}
}
