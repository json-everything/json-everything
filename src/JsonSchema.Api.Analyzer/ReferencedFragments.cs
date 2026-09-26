using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// Finds the generated fragments in referenced assemblies.
/// </summary>
/// <remarks>
/// A referenced assembly is already compiled when this analyzer runs, so its fragment is
/// visible as an ordinary symbol.  The generated module initializer calls each one's
/// `Register` method, which does nothing except force the runtime to load that assembly so
/// its own module initializer runs.
/// </remarks>
internal static class ReferencedFragments
{
	/// <summary>
	/// Collects the fully-qualified names of the fragments this compilation references.
	/// </summary>
	/// <param name="compilation">The compilation.</param>
	/// <returns>The fully-qualified type names, in a stable order.</returns>
	public static IReadOnlyList<string> Find(Compilation compilation)
	{
		var names = new SortedSet<string>(System.StringComparer.Ordinal);

		foreach (var reference in compilation.References)
		{
			if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly) continue;

			foreach (var type in FindIn(assembly.GlobalNamespace))
			{
				if (type.DeclaredAccessibility != Accessibility.Public) continue;

				names.Add(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
			}
		}

		return [.. names];
	}

	private static IEnumerable<INamedTypeSymbol> FindIn(INamespaceSymbol ns)
	{
		foreach (var type in ns.GetTypeMembers(OpenApiSourceGenerator.FragmentClassName))
			yield return type;

		foreach (var nested in ns.GetNamespaceMembers())
		{
			foreach (var type in FindIn(nested))
				yield return type;
		}
	}
}
