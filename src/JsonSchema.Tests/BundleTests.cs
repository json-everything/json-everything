using System;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public class BundleTests
{
    [Test]
    public void CreateBundle_IncludesReferencedSchemasAndExcludesIndependent()
    {
        var buildOptions = new BuildOptions { SchemaRegistry = new() };
        var registry = buildOptions.SchemaRegistry;

        var aUri = new Uri("https://json-everything.test/bundle-ref-a");
        var bUri = new Uri("https://json-everything.test/bundle-ref-b");
        var rootUri = new Uri("https://json-everything.test/bundle-root");
        var independentUri = new Uri("https://json-everything.test/bundle-independent");
        var bundleUri = new Uri("https://json-everything.test/bundle");

        _ = new JsonSchemaBuilder(buildOptions)
            .Schema(MetaSchemas.Draft202012Id)
            .Id(aUri)
            .Type(SchemaValueType.String)
            .Build();

        _ = new JsonSchemaBuilder(buildOptions)
            .Schema(MetaSchemas.Draft202012Id)
            .Id(bUri)
            .Type(SchemaValueType.Integer)
            .Build();

        _ = new JsonSchemaBuilder(buildOptions)
            .Schema(MetaSchemas.Draft202012Id)
            .Id(rootUri)
            .Properties(
                ("a", new JsonSchemaBuilder().Ref(aUri)),
                ("b", new JsonSchemaBuilder().Ref(bUri))
            )
            .Build();

        _ = new JsonSchemaBuilder(buildOptions)
            .Schema(MetaSchemas.Draft202012Id)
            .Id(independentUri)
            .Type(SchemaValueType.Object)
            .Build();

        var bundle = registry.CreateBundle(rootUri, bundleUri);

        Assert.That(bundle, Is.Not.Null);

		TestConsole.WriteLine(JsonSerializer.Serialize(bundle, TestEnvironment.TestOutputSerializerOptions));

        var defs = bundle!.Root.Source.GetProperty("$defs");

        Assert.That(defs.TryGetProperty(rootUri.ToString(), out _), Is.True, "Root schema should be in $defs");
        Assert.That(defs.TryGetProperty(aUri.ToString(), out _), Is.True, "Schema A should be in $defs");
        Assert.That(defs.TryGetProperty(bUri.ToString(), out _), Is.True, "Schema B should be in $defs");
        Assert.That(defs.TryGetProperty(independentUri.ToString(), out _), Is.False, "Independent schema should not be in $defs");
    }

    [Test]
    public void CreateBundle_EmbedsADocumentOnceWhenReferencedAtSeveralFragments()
    {
        var buildOptions = new BuildOptions { SchemaRegistry = new() };
        var registry = buildOptions.SchemaRegistry;

        var commonUri = new Uri("https://json-everything.test/bundle-common");
        var rootUri = new Uri("https://json-everything.test/bundle-root");
        var bundleUri = new Uri("https://json-everything.test/bundle");

        _ = new JsonSchemaBuilder(buildOptions)
            .Schema(MetaSchemas.Draft202012Id)
            .Id(commonUri)
            .Defs(
                ("a", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("b", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
            )
            .Build();

        _ = new JsonSchemaBuilder(buildOptions)
            .Schema(MetaSchemas.Draft202012Id)
            .Id(rootUri)
            .Properties(
                ("a", new JsonSchemaBuilder().Ref(new Uri(commonUri, "#/$defs/a"))),
                ("b", new JsonSchemaBuilder().Ref(new Uri(commonUri, "#/$defs/b")))
            )
            .Build();

        var bundle = registry.CreateBundle(rootUri, bundleUri);

        Assert.That(bundle, Is.Not.Null);

        TestConsole.WriteLine(JsonSerializer.Serialize(bundle, TestEnvironment.TestOutputSerializerOptions));

        var defs = bundle!.Root.Source.GetProperty("$defs");

        Assert.That(defs.TryGetProperty(commonUri.ToString(), out _), Is.True, "Common schema should be in $defs under its own URI");
        Assert.That(defs.EnumerateObject().Count(), Is.EqualTo(2), "Common schema should be embedded once, beside the root");
        Assert.DoesNotThrow(() => JsonSchema.Build(bundle.Root.Source, new BuildOptions { SchemaRegistry = new() }), "Bundle should load on its own");
    }
}
