using System;
using System.IO;
using Json.Schema.Api.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Json.Schema.Api.Tests.OpenApi;

/// <summary>
/// Verifies that a misconfigured description is rejected at startup rather than published
/// wrongly.
/// </summary>
/// <remarks>
/// Each of these is silent otherwise: a description that shares a path is unreachable, and
/// one that shares a file path is overwritten.
/// </remarks>
public class ConfigurationErrorTests
{
	private static IServiceCollection Services() => new ServiceCollection();

	[Test]
	public void APageOnANamedDescriptionIsRejected()
	{
		var exception = Assert.Throws<InvalidOperationException>(() =>
			Services().AddOpenApi("admin", c => c.InteractivePath = "/openapi/admin/reference"));

		Assert.That(exception!.Message, Does.Contain("admin"));
		Assert.That(exception.Message, Does.Contain("takes no name"));
	}

	[Test]
	public void APageWithoutAServedDescriptionIsRejected()
	{
		var exception = Assert.Throws<InvalidOperationException>(() =>
			Services().AddOpenApi(c =>
			{
				c.DocumentPath = null;
				c.InteractivePath = "/openapi/reference";
			}));

		Assert.That(exception!.Message, Does.Contain(nameof(OpenApiOptions.DocumentPath)));
	}

	[Test]
	public void APageWithoutJsonIsRejected()
	{
		var exception = Assert.Throws<InvalidOperationException>(() =>
			Services().AddOpenApi(c => c.DocumentFormats = OpenApiFormats.Yaml));

		Assert.That(exception!.Message, Does.Contain(nameof(OpenApiOptions.DocumentFormats)));
	}

	[Test]
	public void ASharedDocumentPathIsRejected()
	{
		var services = Services();
		services.AddOpenApi(c => c.InteractivePath = null);

		var exception = Assert.Throws<InvalidOperationException>(() =>
			services.AddOpenApi("admin", c => c.DocumentPath = "/openapi"));

		Assert.That(exception!.Message, Does.Contain(nameof(OpenApiOptions.DocumentPath)));
		Assert.That(exception.Message, Does.Contain("/openapi"));
		Assert.That(exception.Message, Does.Contain("default description"));
	}

	[Test]
	public void TwoNamedDescriptionsSharingAPathAreRejected()
	{
		var services = Services();
		services.AddOpenApi(c => c.InteractivePath = null);
		services.AddOpenApi("admin", c => c.DocumentPath = "/openapi/shared");

		var exception = Assert.Throws<InvalidOperationException>(() =>
			services.AddOpenApi("partner", c => c.DocumentPath = "/openapi/shared"));

		Assert.That(exception!.Message, Does.Contain("`admin` description"));
	}

	[Test]
	public void ASharedFileOutputPathIsRejected()
	{
		var path = Path.Combine(Path.GetTempPath(), "openapi");

		var services = Services();
		services.AddOpenApi(c =>
		{
			c.InteractivePath = null;
			c.FileOutputPath = path;
		});

		var exception = Assert.Throws<InvalidOperationException>(() =>
			services.AddOpenApi("admin", c => c.FileOutputPath = path));

		Assert.That(exception!.Message, Does.Contain(nameof(OpenApiOptions.FileOutputPath)));
	}

	[Test]
	public void ADocumentPathColldingWithThePageIsRejected()
	{
		var services = Services();
		services.AddOpenApi(c => c.InteractivePath = "/openapi/reference");

		var exception = Assert.Throws<InvalidOperationException>(() =>
			services.AddOpenApi("admin", c => c.DocumentPath = "/openapi/reference"));

		Assert.That(exception!.Message, Does.Contain("/openapi/reference"));
	}

	[Test]
	public void PathsDifferingOnlyByCaseCollide()
	{
		var services = Services();
		services.AddOpenApi(c => c.InteractivePath = null);

		var exception = Assert.Throws<InvalidOperationException>(() =>
			services.AddOpenApi("admin", c => c.DocumentPath = "/OpenApi"));

		Assert.That(exception!.Message, Does.Contain(nameof(OpenApiOptions.DocumentPath)));
	}

	[Test]
	public void ReconfiguringADescriptionDoesNotCollideWithItself()
	{
		var services = Services();
		services.AddOpenApi(c => c.InteractivePath = null);

		Assert.DoesNotThrow(() => services.AddOpenApi(c =>
		{
			c.InteractivePath = null;
			c.Document.Info.Title = "Reconfigured";
		}));
	}

	[Test]
	public void ANullNameIsRejected()
	{
		Assert.Throws<ArgumentNullException>(() => Services().AddOpenApi(null!, _ => { }));
	}

	[Test]
	public void DistinctPathsAreAccepted()
	{
		var services = Services();

		Assert.DoesNotThrow(() =>
		{
			services.AddOpenApi();
			services.AddOpenApi("admin");
			services.AddOpenApi("partner");
		});
	}
}
