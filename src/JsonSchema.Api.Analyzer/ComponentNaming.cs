using System.Text;
using Json.Schema.Generation.SourceGeneration;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// Produces OpenAPI component names for types.
/// </summary>
/// <remarks>
/// Component names are map keys scoped to the document, not identifiers: they need to be
/// short, stable, and unique within the document.  The canonical shape comes from
/// <see cref="TypeShape"/> so that the aliasing rules match what the schema generator
/// used when it emitted the refs.
/// </remarks>
internal static class ComponentNaming
{
	/// <summary>
	/// Produces the component name for a fully-qualified type name.
	/// </summary>
	/// <param name="typeName">The fully-qualified type name.</param>
	/// <param name="qualified">Whether to include the namespace, to disambiguate a collision.</param>
	/// <returns>The component name.</returns>
	public static string ForType(string typeName, bool qualified = false)
	{
		var shape = TypeShape.Of(typeName);

		switch (shape.Kind)
		{
			case TypeShapeKind.Array:
				return $"{ForType(shape.First!, qualified)}Array";
			case TypeShapeKind.Dictionary:
				return $"{ForType(shape.First!, qualified)}{ForType(shape.Second!, qualified)}Map";
		}

		return Sanitize(qualified ? shape.Name : StripNamespace(shape.Name));
	}

	private static string StripNamespace(string typeName)
	{
		// Only strip the namespace from the outermost name; generic arguments keep whatever
		// qualification they already carry so that `Foo<Bar.Baz>` stays distinguishable.
		var genericStart = typeName.IndexOf('<');
		var head = genericStart < 0 ? typeName : typeName[..genericStart];
		var tail = genericStart < 0 ? string.Empty : typeName[genericStart..];

		var lastDot = head.LastIndexOf('.');
		if (lastDot >= 0)
			head = head[(lastDot + 1)..];

		return head + tail;
	}

	private static string Sanitize(string typeName)
	{
		// OpenAPI component names are restricted to `^[a-zA-Z0-9\.\-_]+$`.
		var sb = new StringBuilder();
		foreach (var ch in typeName)
		{
			if (char.IsLetterOrDigit(ch) || ch == '.' || ch == '-') sb.Append(ch);
			else if (ch == '<' || ch == ',') sb.Append('_');
			else if (ch == '>' || ch == ' ' || ch == '?') continue;
			else sb.Append('_');
		}

		return sb.ToString();
	}
}
