using System.Collections.Concurrent;
using Json.Schema.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Json.Schema.Tests.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
	private static readonly string[] Summaries = new[]
	{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

	private readonly ILogger<WeatherForecastController> _logger;

	public WeatherForecastController(ILogger<WeatherForecastController> logger)
	{
		_logger = logger;
	}

	[HttpGet(Name = "GetWeatherForecast")]
	public IEnumerable<WeatherForecast> Get()
	{
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
	}

	[HttpPost("channel")]
	public IActionResult CreateChannel(CreateChannelRequest request)
	{
		return Ok(request);
	}
}

[JsonSchema(typeof(SchemaRegistry), nameof(SchemaRegistry.CreateChannelRequest))]
public record CreateChannelRequest(string Name, bool Enabled);

public class SchemaRegistry
{
	public static readonly JsonSchema CreateChannelRequest =
		JsonSchema.FromFile("Schemas/create-channel-request.json");
}

public class MyRequestSchemaTransformer : IOpenApiSchemaTransformer
{
	public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
	{
		if (context.JsonTypeInfo.Type == typeof(CreateChannelRequest))
		{
			schema.Type = JsonSchemaType.Object;
			schema.Properties ??= new ConcurrentDictionary<string, IOpenApiSchema>();
			schema.Properties.Add("name", new OpenApiSchema { Type = JsonSchemaType.String });
			schema.Properties.Add("enabled", new OpenApiSchema { Type = JsonSchemaType.Boolean });
			schema.Required ??= new HashSet<string>();
			schema.Required.Add("name");
			schema.Required.Add("enabled");
		}
		return Task.CompletedTask;
	}
}

// Register the schema transformer
