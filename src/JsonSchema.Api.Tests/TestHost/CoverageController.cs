using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Json.Schema.Api.Tests.TestHost;

/// <summary>
/// Exercises the discovery shapes <see cref="TestController"/> does not: the verbs other than
/// POST, route and query parameters, async handlers, declared response types, and the
/// metadata read from documentation comments and ASP.NET's own attributes.
/// </summary>
/// <remarks>
/// The route uses the `[controller]` token, so the generator has to perform the same
/// substitution routing does.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("Coverage")]
public class CoverageController : ControllerBase
{
	/// <summary>
	/// Fetches one item.
	/// </summary>
	/// <remarks>
	/// The summary and this remark reach the operation; the <c>param</c>, <c>returns</c>,
	/// and <c>response</c> elements reach the parameter and the responses.
	/// </remarks>
	/// <param name="id">The item's identifier.</param>
	/// <returns>The item.</returns>
	/// <response code="404">No item has that identifier.</response>
	[HttpGet("{id:int}")]
	public IActionResult GetById(int id)
	{
		return Ok(new SimpleModel($"item-{id}", id));
	}

	/// <summary>
	/// This summary loses to the attribute.
	/// </summary>
	/// <param name="term">Text the name must contain.</param>
	/// <param name="limit">The most items to return.</param>
	[HttpGet("search")]
	[EndpointSummary("Searches items by name.")]
	[EndpointDescription("Attributes win over the documentation comment.")]
	[Tags("Search")]
	public IActionResult Search([FromQuery] string term, [FromQuery] int? limit)
	{
		return Ok(new SimpleModel(term, limit ?? 0));
	}

	/// <summary>
	/// Replaces an item.
	/// </summary>
	/// <param name="id">The item's identifier.</param>
	/// <param name="model">The item's new state.</param>
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
