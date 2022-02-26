using System;
using System.Linq;
using Json.Logic;
using Json.Patch;
using Json.Path;
using Json.Schema.DataGeneration;
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

		[HttpPost("schema-data-generation")]
		public ActionResult<SchemaValidationOutput> GenerateData([FromBody] SchemaDataGenerationInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var schema = input.Schema;

			var result = schema.GenerateData();
			return Ok(new SchemaDataGenerationOutput
			{
				Result = result.Result,
				ErrorMessage = result.ErrorMessage
			});
		}

		[HttpPost("path-query")]
		public ActionResult<PathQueryOutput> QueryPath([FromBody] PathQueryInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var path = JsonPath.Parse(input.Path);
			var data = input.Data.RootElement;
			var options = new PathEvaluationOptions
			{
				ExperimentalFeatures =
				{
					ProcessDataReferences = input.Options?.ResolveReferences ?? false
				}
			};


			var result = path.Evaluate(data, options);
			return Ok(new PathQueryOutput {Result = result});
		}

		[HttpPost("patch-apply")]
		public ActionResult<PatchApplyOutput> ApplyPatch([FromBody] ApplyPatchInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var data = input.Data.RootElement;
			var patch = input.Patch;
			
			var result = patch.Apply(data);
			return Ok(new PatchApplyOutput {Result = result});
		}

		[HttpPost("patch-generate")]
		public ActionResult<PatchGenerationOutput> GeneratePatch([FromBody] GeneratePatchInput input)
		{
			if (!ModelState.IsValid)
				return BadRequest(new LogicProcessOutput {Errors = ModelState.Root.GetErrors().ToList()});

			var start = input.Start.RootElement;
			var target = input.Target.RootElement;

			try
			{
				var result = start.CreatePatch(target);
				return Ok(new PatchGenerationOutput {Patch = result});
			}
			catch (Exception e)
			{
				return Ok(new PatchGenerationOutput {Error = e.Message});
			}
		}

		[HttpPost("logic-apply")]
		public ActionResult<PatchGenerationOutput> ApplyLogic([FromBody] LogicProcessInput input)
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
