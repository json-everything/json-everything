using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Json.Schema.Api.Tests.TestHost;

/// <summary>
/// Exercises the discovery shapes <see cref="TestController"/> does not: the verbs other than
/// POST, route and query parameters, async handlers, and declared response types.
/// </summary>
/// <remarks>
/// The route uses the `[controller]` token, so the generator has to perform the same
/// substitution routing does.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class CoverageController : ControllerBase
{
	[HttpGet("{id:int}")]
	public IActionResult GetById(int id)
	{
		return Ok(new SimpleModel($"item-{id}", id));
	}

	[HttpGet("search")]
	public IActionResult Search([FromQuery] string term, [FromQuery] int? limit)
	{
		return Ok(new SimpleModel(term, limit ?? 0));
	}

	[HttpPut("{id:int}")]
	public IActionResult Replace(int id, [FromBody] StrictModel model)
	{
		return Ok(model);
	}

	[HttpPatch("{id:int}")]
	public IActionResult Modify(int id, [FromBody] SimpleModel model)
	{
		return Ok(model);
	}

	[HttpDelete("{id:int}")]
	public IActionResult Remove(int id)
	{
		return NoContent();
	}

	[HttpPost("async")]
	public async Task<IActionResult> PostAsync([FromBody] SimpleModel model)
	{
		await Task.Yield();

		return Ok(model);
	}

	/// <summary>
	/// Declares its response rather than returning it, so the schema comes from the attribute
	/// instead of from the handler body.
	/// </summary>
	[HttpGet("declared")]
	[ProducesResponseType(typeof(MultiWordModel), 200)]
	public IActionResult Declared()
	{
		return Ok(new MultiWordModel("first", "last"));
	}
}
