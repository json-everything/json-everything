using System;
using System.Collections.Generic;

namespace Json.Schema;

/// <summary>
/// Provides filtering options for output formats.
/// </summary>
public class OutputFilteringOptions
{
	internal HashSet<Type>? IgnoredAnnotationTypes { get; private set; }

	/// <summary>
	/// Ignores annotations from the specified keyword.
	/// </summary>
	/// <typeparam name="T">The keyword type which should not have annotations.</typeparam>
	public void IgnoreAnnotationsFrom<T>()
		where T : IJsonSchemaKeyword
	{
		IgnoredAnnotationTypes ??= new HashSet<Type>();

		IgnoredAnnotationTypes.Add(typeof(T));
	}

	/// <summary>
	/// Ignores all annotations.
	/// </summary>
	public void IgnoreAllAnnotations()
	{
		IgnoredAnnotationTypes = new HashSet<Type>(SchemaKeywordRegistry.KeywordTypes);
	}

	/// <summary>
	/// Clears ignored annotations.
	/// </summary>
	public void ClearIgnoredAnnotations()
	{
		IgnoredAnnotationTypes = null;
	}

	/// <summary>
	/// Restores annotation collection for the specified keyword.
	/// </summary>
	public void CollectAnnotationsFrom<T>()
	{
		IgnoredAnnotationTypes?.Remove(typeof(T));
	}
}