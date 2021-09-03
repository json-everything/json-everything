using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Json.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using TryJsonEverything.Controllers;

namespace TryJsonEverything.Services
{
	public class SchemaValidationMiddleware
	{
		private readonly RequestDelegate _next;

		public SchemaValidationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			var results = await CheckParameters(context);

			if (results?.IsValid != false)
			{
				await _next(context);
				return;
			}

			var resultsText = results.NestedResults.Any()
				? JsonSerializer.Serialize(new {validationErrors = results.NestedResults})
				: JsonSerializer.Serialize(new {validationErrors = new[] {results}});

			context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
			await using var writer = new StreamWriter(context.Response.Body);
			await writer.WriteAsync(resultsText);
			await writer.FlushAsync();
		}

		private static async Task<ValidationResults?> CheckParameters(HttpContext context)
		{
			if (!context.Request.Headers.TryGetValue("Content-Type", out var contentType) &&
			    contentType == "application/json") return null;

			var controllerActionDescriptor = context
				.GetEndpoint()?
				.Metadata
				.GetMetadata<ControllerActionDescriptor>();

			if (controllerActionDescriptor == null) return null;

			var controllerName = controllerActionDescriptor.ControllerName;
			var actionName = controllerActionDescriptor.ActionName;

			var controllerType = Type.GetType(typeof(ApiController).AssemblyQualifiedName!.Replace("Api", controllerName));
			var method = controllerType!.GetMethods().FirstOrDefault(x => x.Name == actionName && !x.IsStatic);
			if (method == null) return null;

			var parameter = method.GetParameters().Where(IsBodyParameter).FirstOrDefault();
			if (parameter == null) return null;

			var schema = parameter.ParameterType.GetCustomAttributes().OfType<SchemaAttribute>().First().Schema;

			var instanceText = await GetBody(context);
			JsonDocument instance;
			try
			{
				instance = JsonDocument.Parse(instanceText);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

			return schema.Validate(instance.RootElement, new ValidationOptions
			{
				OutputFormat = OutputFormat.Basic,
				RequireFormatValidation = true
			});
		}

		private static bool IsBodyParameter(ParameterInfo parameter)
		{
			return parameter.CustomAttributes.Any(x => x.AttributeType == typeof(FromBodyAttribute)) &&
			       parameter.ParameterType.GetCustomAttributes().OfType<SchemaAttribute>().Any();
		}

		private static async Task<string> GetBody(HttpContext context)
		{
			context.Request.EnableBuffering();
			var originalBodyStream = context.Request.Body;

			await using var requestBody = new MemoryStream();
			await originalBodyStream.CopyToAsync(requestBody);
			originalBodyStream.Position = 0;
			requestBody.Position = 0;

			using var reader = new StreamReader(requestBody);
			return await reader.ReadToEndAsync();
		}
	}
}