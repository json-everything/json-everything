using Json.Schema.Generation;
using Json.Schema.Generation.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Json.Schema.Tests.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ChannelController : ControllerBase
{
	[HttpPost("/channel")]
	public IActionResult CreateChannel(CreateChannelRequest request)
	{
		return Ok(request);
	}
}

[GenerateJsonSchema]
[AdditionalProperties(false)]
public record CreateChannelRequest(
	[property: Required]
	string Name,
	bool Enabled);
