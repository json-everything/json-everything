using System;
using System.Threading.Tasks;
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

builder.Services.AddOpenApi(c =>
{
	c.Document.Info.Title = "Test Host API";
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

// The verbs other than POST, and parameters that come from the route and the query rather
// than the body.
minimal.MapGet("/{id:int}", (int id) => Results.Ok(new SimpleModel($"item-{id}", id)));
minimal.MapGet("/search", (string term, int? limit) => Results.Ok(new SimpleModel(term, limit ?? 0)));
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

app.Run();
