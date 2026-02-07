using System;
using Json.Schema.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	ContentRootPath = AppContext.BaseDirectory
});

builder.Services
	.AddControllers()
	.AddJsonSchemaValidation();

var app = builder.Build();

app.MapControllers();

app.Run();
