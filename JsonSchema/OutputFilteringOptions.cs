using System;
using System.Collections.Generic;

namespace Json.Schema;

/// <summary>
/// Provides filtering options for output formats.
/// </summary>
public class OutputFilteringOptions
{
	private HashSet<Type>? _ignoredAnnotationTypes;

	/// <summary>
	/// Gets the set of keyword types from which annotations will be ignored.
	/// </summary>
	public IEnumerable<Type>? IgnoredAnnotations => _ignoredAnnotationTypes;

	/// <summary>
	/// Ignores annotations from the specified keyword.
	/// </summary>
	/// <typeparam name="T">The keyword type which should not have annotations.</typeparam>
	public void IgnoreAnnotationsFrom<T>()
		where T : IJsonSchemaKeyword
	{
		_ignoredAnnotationTypes ??= new HashSet<Type>();

		_ignoredAnnotationTypes.Add(typeof(T));
	}

	/// <summary>
	/// Ignores all annotations.
	/// </summary>
	public void IgnoreAllAnnotations()
	{
		_ignoredAnnotationTypes = new HashSet<Type>(SchemaKeywordRegistry.KeywordTypes);
	}

	/// <summary>
	/// Clears ignored annotations.
	/// </summary>
	public void ClearIgnoredAnnotations()
	{
		_ignoredAnnotationTypes = null;
	}

	/// <summary>
	/// Restores annotation collection for the specified keyword.
	/// </summary>
	public void CollectAnnotationsFrom<T>()
	{
		_ignoredAnnotationTypes?.Remove(typeof(T));
	}
}
