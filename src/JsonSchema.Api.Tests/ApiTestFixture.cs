using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Json.Schema.Api.Tests;

public class ApiTestFixture : IDisposable
{
	private readonly WebApplicationFactory<Program> _factory;
	
	public HttpClient Client { get; }

	public ApiTestFixture()
	{
		_factory = new WebApplicationFactory<Program>();
		Client = _factory.CreateClient();
	}

	public void Dispose()
	{
		Client.Dispose();
		_factory.Dispose();

		GC.SuppressFinalize(this);
	}
}
