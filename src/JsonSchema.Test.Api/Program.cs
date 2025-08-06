using Json.Schema;
using Json.Schema.Serialization;
using Json.Schema.Tests.Api.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(o =>
{
	o.AddSchemaTransformer<MyRequestSchemaTransformer>();
});
builder.Services.ConfigureHttpJsonOptions(opt =>
{
	opt.SerializerOptions.Converters.Add(new ValidatingJsonConverter
	{
		Options =
		{
			RequireFormatValidation = true,
			OutputFormat = OutputFormat.List
		}
	});
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
