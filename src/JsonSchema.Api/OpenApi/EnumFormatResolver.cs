using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.SourceGeneration;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Reconciles the enum formats declared by an application's assemblies into the one the
/// serializer has to satisfy.
/// </summary>
internal static class EnumFormatResolver
{
	/// <summary>
	/// Resolves the format across fragments.
	/// </summary>
	/// <remarks>
	/// One serializer configuration serves every assembly, so it has to accept whatever any
	/// of them describes: names if any fragment describes names, integers if any describes
	/// integers.  Assemblies that disagree therefore resolve to accepting both.
	/// </remarks>
	public static EnumFormat Resolve(IEnumerable<OpenApiFragment> fragments)
	{
		var formats = fragments.Select(x => x.EnumFormat).Distinct().ToList();

		if (formats.Count == 0) return EnumFormat.Names;
		if (formats.Count == 1) return formats[0];

		return EnumFormat.NamesAndValues;
	}
}
