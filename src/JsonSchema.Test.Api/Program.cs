using Json.Schema.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
	.AddControllers()
	.AddJsonSchemaValidation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	// dev options
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
