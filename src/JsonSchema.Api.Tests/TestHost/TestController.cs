using Microsoft.AspNetCore.Mvc;

namespace Json.Schema.Api.Tests.TestHost;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
	[HttpPost("simple")]
	public IActionResult PostSimple([FromBody] SimpleModel model)
	{
		return Ok(model);
	}

	[HttpPost("strict")]
	public IActionResult PostStrict([FromBody] StrictModel model)
	{
		return Ok(model);
	}

	[HttpPost("multiword")]
	public IActionResult PostMultiWord([FromBody] MultiWordModel model)
	{
		return Ok(model);
	}

	[HttpPost("unvalidated")]
	public IActionResult PostUnvalidated([FromBody] UnvalidatedModel model)
	{
		return Ok(model);
	}
}
