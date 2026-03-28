using System;
using Json.Schema.Api;
using Json.Schema.Api.Tests.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#if NET9_0_OR_GREATER
using Microsoft.AspNetCore.OpenApi;
#endif
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddControllers();

builder.Services.AddJsonSchemaValidation();

#if NET9_0_OR_GREATER
builder.Services.AddOpenApi();
#endif

var app = builder.Build();

app.MapControllers();
#if NET9_0_OR_GREATER
app.MapOpenApi();
#endif

var minimal = app.MapGroup("/minimal/test");
minimal.MapPost("/simple", (SimpleModel model) => Results.Ok(model));
minimal.MapPost("/strict", (StrictModel model) => Results.Ok(model));
minimal.MapPost("/multiword", (MultiWordModel model) => Results.Ok(model));
minimal.MapPost("/unvalidated", (UnvalidatedModel model) => Results.Ok(model));

app.Run();
