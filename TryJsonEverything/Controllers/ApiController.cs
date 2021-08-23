using System.Linq;
using Json.Path;
using Json.Schema;
using Microsoft.AspNetCore.Mvc;
using TryJsonEverything.Models;
using TryJsonEverything.Services;

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
			var path = JsonPath.Parse(input.Path);
			var data = input.Data.RootElement;
			
			var result = path.Evaluate(data);
			return Ok(new PathQueryOutput {Result = result});
		}

		[HttpPost("api/patch-apply")]
		public ActionResult<PatchProcessOutput> ApplyPatch([FromBody] PatchProcessInput input)
		{
			var data = input.Data.RootElement;
			var patch = input.Patch;
			
			var result = patch.Apply(data);
			return Ok(new PatchProcessOutput {Result = result});
		}

		[HttpPost("api/logic-apply")]
		public ActionResult<PatchProcessOutput> ApplyLogic([FromBody] LogicProcessInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput { Errors = ModelState.Root.GetErrors().ToList() });

			var data = input.Data.RootElement;
			var patch = input.Logic;

			var result = patch.Apply(data);
			return Ok(new LogicProcessOutput {Result = result});
		}

		[HttpPost("api/pointer-query")]
		public ActionResult<PointerProcessOutput> QueryPointer([FromBody] PointerProcessInput input)
		{
			var data = input.Data.RootElement;
			var pointer = input.Pointer.Value;

			var result = pointer.Evaluate(data);
			return Ok(new PointerProcessOutput {Result = result});
		}
	}
}
