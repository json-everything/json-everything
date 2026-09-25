using System;
using Json.Schema.Api;
using Json.Schema.Api.OpenApi;
using Json.Schema.Api.Tests.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddControllers();

builder.Services.AddJsonSchemaValidation();

builder.Services.AddOpenApi(c =>
{
	c.Document.Info.Description = "Test host for OpenAPI generation.";
	c.Document.Info.Version = "2.4.0";
});

var app = builder.Build();

app.MapControllers();

var minimal = app.MapGroup("/minimal/test");
minimal.MapPost("/simple", (SimpleModel model) => Results.Ok(model));
minimal.MapPost("/strict", (StrictModel model) => Results.Ok(model));
minimal.MapPost("/multiword", (MultiWordModel model) => Results.Ok(model));
minimal.MapPost("/unvalidated", (UnvalidatedModel model) => Results.Ok(model));

app.Run();
