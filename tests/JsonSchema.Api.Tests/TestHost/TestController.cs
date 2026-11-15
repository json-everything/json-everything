using System;
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

	[HttpPost("enum")]
	public IActionResult PostEnum([FromBody] EnumModel model)
	{
		return Ok(model);
	}

	[HttpGet("filter")]
	public IActionResult Filter([FromQuery] Category? category, [FromQuery] Guid? customerId, [FromQuery] int page = 1)
	{
		return Ok(new SimpleModel($"{category}-{customerId}", page));
	}

	[HttpPost("unvalidated")]
	public IActionResult PostUnvalidated([FromBody] UnvalidatedModel model)
	{
		return Ok(model);
	}
}
