using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Json.Pointer;
using NUnit.Framework;

namespace Json.Schema.Tests;

public partial class DependencyChecks
{
	// already copied as part of library build
	private const string _nuspecFile = "nuspec/JsonSchema.Net.nuspec";

	[GeneratedRegex("""
	                id="(?<lib>.*)" version="(?<version>\d*\.\d*\.\d*(\.\d*)?(-.*)?)"
	                """)]
	private static partial Regex VersionPattern();

	private static readonly string[] _focusedAssemblies =
	[
		"Json.More",
		"JsonPointer.Net"
	];

	[Test]
	public void VerifyDependencyVersions()
	{
		_ = JsonPointer.Create("load"); // need to load the assembly when the other tests aren't run
	
		var nuspecLines = File.ReadAllLines(_nuspecFile);
		var nugetDependencies = nuspecLines
			.Select(x => VersionPattern().Match(x))
			.Where(x1 => x1.Success)
			.ToDictionary(GetLibName, GetVersion);

		var library = typeof(JsonSchema).Assembly;
		var domain = AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.FullName);
		var libDependencies = library.GetReferencedAssemblies()
			.Where(x => _focusedAssemblies.Contains(x.Name))
			.ToDictionary(x => x.Name!, x => domain.First(a => a.GetName().Name == x.Name));

		foreach (var (nugetDependency, nugetVersion) in nugetDependencies)
		{
			var libDependency = libDependencies[nugetDependency];
			var libVersion = ((AssemblyInformationalVersionAttribute?)libDependency
				.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute))
				.SingleOrDefault())?.InformationalVersion;

			Assert.That(libVersion, Does.StartWith(nugetVersion));
		}
	}

	private string GetLibName(Match match)
	{
		var pkgName = match.Groups["lib"].Value;
		return _focusedAssemblies.First(pkgName.StartsWith);
	}

	private string GetVersion(Match match) => match.Groups["version"].Value;
}