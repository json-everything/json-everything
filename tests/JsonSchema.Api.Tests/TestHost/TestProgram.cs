using System;
using System.Threading.Tasks;
using Json.Schema.Api.OpenApi;
using Json.Schema.Api.Tests.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IEchoService, EchoService>();

builder.Services.AddOpenApi(c =>
{
	c.Document.Info.Title = "Test Host API";
	c.Document.Info.Description = "Test host for OpenAPI generation.";
	c.Document.Info.Version = "2.4.0";
});

builder.Services.AddOpenApi("admin", c =>
{
	c.Document.Info.Title = "Admin API";
});

builder.Services.AddOpenApi("partner", c =>
{
	c.Document.Info.Title = "Partner API";
});

var app = builder.Build();

app.MapControllers();

var minimal = app.MapGroup("/minimal/test");
minimal.MapPost("/simple", (SimpleModel model) => Results.Ok(model));
minimal.MapPost("/strict", (StrictModel model) => Results.Ok(model));
minimal.MapPost("/multiword", (MultiWordModel model) => Results.Ok(model));
minimal.MapPost("/unvalidated", (UnvalidatedModel model) => Results.Ok(model));

// The verbs other than POST, and parameters that come from the route and the query rather
// than the body.
minimal.MapGet("/{id:int}", (int id) => Results.Ok(new SimpleModel($"item-{id}", id)));
minimal.MapGet("/search", (string term, int? limit) => Results.Ok(new SimpleModel(term, limit ?? 0)))
	.WithSummary("Searches items by name.")
	.WithDescription("Fluent metadata reaches the operation.")
	.WithTags("Minimal", "Search");
minimal.MapPut("/{id:int}", (int id, StrictModel model) => Results.Ok(model));
minimal.MapPatch("/{id:int}", (int id, SimpleModel model) => Results.Ok(model));
minimal.MapDelete("/{id:int}", (int id) => Results.NoContent());

// An async lambda, so the response type sits behind a `Task<T>`.
minimal.MapPost("/async", async (SimpleModel model) =>
{
	await Task.Yield();

	return Results.Ok(model);
});

// `TypedResults` carries the response type in the signature, where `Results` erases it.
minimal.MapPost("/typed", (StrictModel model) => TypedResults.Ok(model));

// A method group rather than a lambda, so the handler body is in a separate declaration.
minimal.MapPost("/methodgroup", MinimalHandlers.Handle);

// Declared rather than returned, so the schema comes from the call instead of the body.
minimal.MapGet("/declared", () => Results.Ok(new MultiWordModel("first", "last")))
	.Produces<MultiWordModel>();

// Nested, so the generator has to compose both prefixes onto the endpoint's own template.
var nested = minimal.MapGroup("/nested");
nested.MapPost("/simple", (SimpleModel model) => Results.Ok(model));
nested.MapGet("/{id:int}", (int id) => Results.Ok(new SimpleModel($"nested-{id}", id)));

// Fluent calls chained onto the group must not hide its prefix.
var chained = app.MapGroup("/minimal/chained").WithTags("Chained");
chained.MapGet("/{id:int}", (int id) => Results.Ok(new SimpleModel($"chained-{id}", id)));

// An injected service after the body must not displace it, and a service on a GET must not
// become a body.
minimal.MapPost("/injected", ([FromBody] SimpleModel model, IEchoService echo) => Results.Ok(echo.Echo(model)));
minimal.MapGet("/injected/{id:int}", (int id, IEchoService echo) => Results.Ok(echo.Echo(new SimpleModel($"item-{id}", id))));

// Nullable query parameters are optional and still carry a schema.
minimal.MapGet("/filter", (Category? category, Guid? customerId, int page = 1) =>
	Results.Ok(new SimpleModel($"{category}-{customerId}", page)));

app.Run();
