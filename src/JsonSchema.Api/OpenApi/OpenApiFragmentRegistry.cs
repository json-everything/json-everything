using System.Collections.Generic;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Collects the fragments contributed by each assembly.
/// </summary>
/// <remarks>
/// Every assembly that declares API surface emits a module initializer which adds its
/// fragment here and touches the assemblies it references, so that their initializers run
/// in turn.  By the time the application configures services, the registry holds every
/// fragment reachable from the entry assembly, with no reflection and no dependence on
/// load order.
/// </remarks>
public static class OpenApiFragmentRegistry
{
	private static readonly List<OpenApiFragment> _fragments = [];
	private static readonly object _lock = new();

	/// <summary>
	/// Adds a fragment.
	/// </summary>
	/// <param name="fragment">The fragment.</param>
	/// <remarks>
	/// Called from generated module initializers.  The runtime runs a module initializer
	/// at most once per assembly, so a fragment cannot be added twice.
	/// </remarks>
	public static void Add(OpenApiFragment fragment)
	{
		lock (_lock)
		{
			_fragments.Add(fragment);
		}
	}

	/// <summary>
	/// Gets the fragments registered so far.
	/// </summary>
	/// <returns>The fragments.</returns>
	public static IReadOnlyList<OpenApiFragment> GetFragments()
	{
		lock (_lock)
		{
			return _fragments.ToArray();
		}
	}
}
