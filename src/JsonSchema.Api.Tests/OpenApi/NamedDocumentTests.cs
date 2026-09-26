using System.Linq;
using Json.Schema.Api.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Json.Schema.Api.Tests.OpenApi;

/// <summary>
/// Verifies that `[OpenApiDocument]` splits the API surface across descriptions.
/// </summary>
/// <remarks>
/// These assert on the assembled description rather than on a served response, since
/// partitioning is decided when the description is built.  The fragments come from this
/// assembly's own controllers, so no host is needed.
/// </remarks>
public class NamedDocumentTests
{
	/// <summary>
	/// Builds a description the way registration does, without standing up a host.
	/// </summary>
	private static OpenApiDocument Build(string? name)
	{
		var services = new ServiceCollection();

		if (name is null)
			services.AddOpenApi(c => c.InteractivePath = null);
		else
			services.AddOpenApi(name);

		return services
			.Select(x => x.ImplementationInstance)
			.OfType<OpenApiOptions>()
			.Single(x => x.Name == name)
			.Document;
	}

	private static string[] RoutesOf(string? name) =>
		Build(name).Paths?.Select(x => x.Key.ToString()).ToArray() ?? [];

	private static string[] SchemasOf(string? name) =>
		Build(name).Components?.Schemas?.Select(x => x.Key).ToArray() ?? [];

	[Test]
	public void AnAttributedControllerIsAbsentFromTheDefaultDocument()
	{
		var routes = RoutesOf(null);

		Assert.That(routes, Does.Not.Contain("/api/admin/strict"));
		Assert.That(routes, Does.Not.Contain("/api/shared/{id}"));
	}

	[Test]
	public void AnUnattributedControllerIsAbsentFromANamedDocument()
	{
		Assert.That(RoutesOf("admin"), Does.Not.Contain("/api/test/simple"));
	}

	[Test]
	public void ANamedDocumentCarriesItsOwnEndpoints()
	{
		Assert.That(RoutesOf("admin"), Does.Contain("/api/admin/strict"));
	}

	[Test]
	public void AControllerCanAppearInSeveralDocuments()
	{
		Assert.That(RoutesOf("admin"), Does.Contain("/api/shared/{id}"), "admin");
		Assert.That(RoutesOf("partner"), Does.Contain("/api/shared/{id}"), "partner");
	}

	[Test]
	public void ANamedDocumentCarriesOnlyItsOwnEndpoints()
	{
		// `partner` names only the shared controller, so its description holds that alone.
		Assert.That(RoutesOf("partner"), Is.EqualTo(new[] { "/api/shared/{id}" }));
	}

	[Test]
	public void ADocumentCarriesOnlyTheSchemasItUses()
	{
		var schemas = SchemasOf("partner");

		// Reached through the shared controller's response; nothing else is.
		Assert.That(schemas, Does.Contain("SimpleModel"));
		Assert.That(schemas, Does.Not.Contain("MultiWordModel"));
	}

	[Test]
	public void AnUnknownNameYieldsAnEmptyDocument()
	{
		Assert.That(RoutesOf("nonexistent"), Is.Empty);
	}

	[Test]
	public void EachDocumentIsBuiltIndependently()
	{
		Assert.That(Build("admin"), Is.Not.SameAs(Build("partner")));
	}
}
