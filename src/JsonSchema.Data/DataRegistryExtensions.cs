using System.Runtime.CompilerServices;

namespace Json.Schema.Data;

/// <summary>
/// Provides extension methods for associating a custom <see cref="DataRegistry"/> with <see cref="BuildOptions"/>
/// instances.
/// </summary>
/// <remarks>These methods enable attaching and retrieving a <see cref="DataRegistry"/> to a <see
/// cref="BuildOptions"/> object without modifying its structure. This is useful for scenarios where build options need
/// to be linked with specific data registries at runtime. The association is maintained using a weak reference, so it
/// does not prevent garbage collection of the <see cref="BuildOptions"/> instance.</remarks>
public static class DataRegistryExtensions
{
	private static readonly ConditionalWeakTable<BuildOptions, DataRegistry> _associations = new();

	/// <summary>
	/// Associates the specified data registry with the given build options instance.
	/// </summary>
	/// <param name="options">The build options instance to which the data registry will be attached. Cannot be null.</param>
	/// <param name="registry">The data registry to associate with the build options. Cannot be null.</param>
	public static void SetDataRegistry(this BuildOptions options, DataRegistry registry)
	{
		_associations.Add(options, registry);
	}

	/// <summary>
	/// Retrieves the data registry associated with the specified build options.
	/// </summary>
	/// <param name="options">The build options for which to obtain the corresponding data registry.</param>
	/// <returns>The data registry linked to the provided build options. If no specific registry is associated, returns the global
	/// data registry.</returns>
	public static DataRegistry GetDataRegistry(this BuildOptions options)
	{
		if (_associations.TryGetValue(options, out var value)) return value;

		return DataRegistry.Global;
	}
}