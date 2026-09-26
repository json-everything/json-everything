using Json.Schema.Api.OpenApi;
using Microsoft.AspNetCore.Mvc;

namespace Json.Schema.Api.Tests.TestHost;

/// <summary>
/// Placed in a named description, so it is absent from the default one.
/// </summary>
[OpenApiDocument("admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
	[HttpPost("strict")]
	public IActionResult PostStrict([FromBody] StrictModel model)
	{
		return Ok(model);
	}
}

/// <summary>
/// Placed in both descriptions, so the same endpoint is described twice.
/// </summary>
[OpenApiDocument("admin", "partner")]
[ApiController]
[Route("api/shared")]
public class SharedController : ControllerBase
{
	/// <remarks>
	/// Declared as <see cref="ActionResult{TValue}"/> rather than `IActionResult`, so the
	/// response type survives into the description and the schema is reachable.
	/// </remarks>
	[HttpGet("{id:int}")]
	public ActionResult<SimpleModel> GetById(int id)
	{
		return new SimpleModel($"shared-{id}", id);
	}
}
