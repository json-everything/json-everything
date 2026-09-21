namespace Json.Schema.Generation.SourceGeneration;

/// <summary>
/// Identifies the canonical shape of a type for the purpose of schema identity.
/// </summary>
internal enum TypeShapeKind
{
	/// <summary>
	/// A named type with no canonical collection shape.
	/// </summary>
	Named,
	/// <summary>
	/// An array-shaped type.  All array shapes over the same element are one schema.
	/// </summary>
	Array,
	/// <summary>
	/// A dictionary-shaped type.  All dictionary shapes over the same key and value are one schema.
	/// </summary>
	Dictionary
}

/// <summary>
/// The canonical shape of a type.
/// </summary>
/// <remarks>
/// This captures the rule that `IEnumerable&lt;T&gt;`, `ICollection&lt;T&gt;`, `T[]` (and friends)
/// all describe the same schema, as do `Dictionary&lt;K,V&gt;` and its interfaces.  It is
/// deliberately separate from any particular identifier format so that a consumer can
/// apply the same canonicalization while spelling identifiers its own way.
/// </remarks>
internal readonly struct TypeShape
{
	/// <summary>
	/// Gets the kind of shape.
	/// </summary>
	public TypeShapeKind Kind { get; }

	/// <summary>
	/// Gets the fully-qualified type name.  Only meaningful when <see cref="Kind"/> is
	/// <see cref="TypeShapeKind.Named"/>.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the element shape of an array, or the key shape of a dictionary.
	/// </summary>
	public string? First { get; }

	/// <summary>
	/// Gets the value shape of a dictionary.
	/// </summary>
	public string? Second { get; }

	private TypeShape(TypeShapeKind kind, string name, string? first = null, string? second = null)
	{
		Kind = kind;
		Name = name;
		First = first;
		Second = second;
	}

	/// <summary>
	/// Determines the canonical shape of a fully-qualified type name.
	/// </summary>
	/// <param name="typeName">The fully-qualified type name.</param>
	/// <returns>The canonical shape.</returns>
	public static TypeShape Of(string typeName)
	{
		// Remove global:: prefix if present
		if (typeName.StartsWith("global::"))
			typeName = typeName.Substring(8);

		// Canonical collection shapes share IDs.
		if (typeName.StartsWith("System.Collections.Generic.IEnumerable<") ||
			typeName.StartsWith("System.Collections.Generic.IReadOnlyCollection<") ||
			typeName.StartsWith("System.Collections.Generic.ICollection<") ||
			typeName.StartsWith("System.Collections.Generic.HashSet<") ||
			typeName.StartsWith("System.Collections.Generic.Queue<") ||
			typeName.StartsWith("System.Collections.Generic.Stack<") ||
			typeName.EndsWith("[]"))
		{
			string elementType;
			if (typeName.EndsWith("[]"))
			{
				elementType = typeName.Substring(0, typeName.Length - 2);
			}
			else
			{
				var start = typeName.IndexOf('<') + 1;
				var len = typeName.LastIndexOf('>') - start;
				elementType = typeName.Substring(start, len);
			}

			return new TypeShape(TypeShapeKind.Array, typeName, elementType);
		}

		// Canonical dictionary shapes share IDs.
		if (typeName.StartsWith("System.Collections.Generic.Dictionary<") ||
			typeName.StartsWith("System.Collections.Generic.IDictionary<") ||
			typeName.StartsWith("System.Collections.Generic.IReadOnlyDictionary<"))
		{
			var start = typeName.IndexOf('<') + 1;
			var len = typeName.LastIndexOf('>') - start;
			var args = typeName.Substring(start, len).Split(',');

			return new TypeShape(TypeShapeKind.Dictionary, typeName, args[0].Trim(), args[1].Trim());
		}

		return new TypeShape(TypeShapeKind.Named, typeName);
	}
}
