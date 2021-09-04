using System.Linq;
using Json.Logic;
using Json.Path;
using Microsoft.AspNetCore.Mvc;
using TryJsonEverything.Models;
using TryJsonEverything.Services;

namespace TryJsonEverything.Controllers
{
	[Route("api")]
	public class ApiController : Controller
	{
		[HttpPost("schema-validation")]
		public ActionResult<SchemaValidationOutput> Validate([FromBody] SchemaValidationInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var schema = input.Schema;
			var instance = input.Instance.RootElement;
			var options = input.Options?.ToValidationOptions();

			var result = schema.Validate(instance, options);
			return Ok(new SchemaValidationOutput {Result = result});
		}

		[HttpPost("path-query")]
		public ActionResult<PathQueryOutput> QueryPath([FromBody] PathQueryInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var path = JsonPath.Parse(input.Path);
			var data = input.Data.RootElement;
			
			var result = path.Evaluate(data);
			return Ok(new PathQueryOutput {Result = result});
		}

		[HttpPost("patch-apply")]
		public ActionResult<PatchProcessOutput> ApplyPatch([FromBody] PatchProcessInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var data = input.Data.RootElement;
			var patch = input.Patch;
			
			var result = patch.Apply(data);
			return Ok(new PatchProcessOutput {Result = result});
		}

		[HttpPost("logic-apply")]
		public ActionResult<PatchProcessOutput> ApplyLogic([FromBody] LogicProcessInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var data = input.Data?.RootElement;
			var patch = input.Logic;

			var result = data == null ? patch.Apply() : patch.Apply(data.Value);
			return Ok(new LogicProcessOutput {Result = result});
		}

		[HttpPost("pointer-query")]
		public ActionResult<PointerProcessOutput> QueryPointer([FromBody] PointerProcessInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var data = input.Data.RootElement;
			var pointer = input.Pointer;

			var result = pointer.Evaluate(data);
			return Ok(new PointerProcessOutput {Result = result});
		}
	}
}
