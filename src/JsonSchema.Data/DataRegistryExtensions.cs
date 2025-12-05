using System.Runtime.CompilerServices;

namespace Json.Schema.Data;

public static class DataRegistryExtensions
{
	private static readonly ConditionalWeakTable<BuildOptions, DataRegistry> _associations = new();

	public static void SetDataRegistry(this BuildOptions options, DataRegistry registry)
	{
		_associations.Add(options, registry);
	}

	public static DataRegistry GetDataRegistry(this BuildOptions options)
	{
		if (_associations.TryGetValue(options, out var value)) return value;

		return DataRegistry.Global;
	}
}