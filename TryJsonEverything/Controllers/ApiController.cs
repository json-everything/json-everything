using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Json.Schema;
using Microsoft.AspNetCore.Mvc;
using TryJsonEverything.Models;

namespace TryJsonEverything.Controllers
{
	public class ApiController : Controller
	{
		[HttpPost("api/validate-schema")]
		public ActionResult<SchemaValidationOutput> ValidateSchema([FromBody] SchemaVlidationInput input)
		{
			var schema = input.Schema;
			var instance = input.Instance.RootElement;

			var result = schema.Validate(instance, new ValidationOptions
			{
				OutputFormat = OutputFormat.Detailed
			});
			return Ok(new SchemaValidationOutput {Result = result});
		}
	}
}
