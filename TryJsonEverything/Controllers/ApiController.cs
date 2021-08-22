using System.Text.Json;
using Json.Path;
using Json.Schema;
using Microsoft.AspNetCore.Mvc;
using TryJsonEverything.Models;

namespace TryJsonEverything.Controllers
{
	public class ApiController : Controller
	{
		[HttpPost("api/schema-validation")]
		public ActionResult<SchemaValidationOutput> Validate([FromBody] SchemaValidationInput input)
		{
			var schema = input.Schema;
			var instance = input.Instance.RootElement;

			var result = schema.Validate(instance, new ValidationOptions
			{
				OutputFormat = OutputFormat.Basic
			});
			return Ok(new SchemaValidationOutput {Result = result});
		}

		[HttpPost("api/path-query")]
		public ActionResult<PathQueryOutput> QueryPath([FromBody] PathQueryInput input)
		{
			if (input == null)
				return BadRequest(new PathQueryOutput {Error = "No input provided"});
			if (input.Data == null || input.Data.RootElement.ValueKind == JsonValueKind.Undefined)
				return BadRequest(new PathQueryOutput {Error = "No data provided"});

			JsonPath path;
			try
			{
				path = JsonPath.Parse(input.Path);
			}
			catch (PathParseException e)
			{
				return BadRequest(new PathQueryOutput {Error = e.Message});
			}

			var data = input.Data.RootElement;
			var result = path.Evaluate(data);
			return Ok(new PathQueryOutput {Result = result});
		}

		[HttpPost("api/patch-apply")]
		public ActionResult<PatchProcessOutput> ProcessPatch([FromBody] PatchProcessInput input)
		{
			if (input == null)
				return BadRequest(new PathQueryOutput {Error = "No input provided"});
			if (input.Data == null || input.Data.RootElement.ValueKind == JsonValueKind.Undefined)
				return BadRequest(new PathQueryOutput {Error = "No data provided"});

			var data = input.Data.RootElement;
			var patch = input.Patch;
			var result = patch.Apply(data);
			return Ok(new PatchProcessOutput {Result = result});
		}
	}
}
