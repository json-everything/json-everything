using System;
using System.Collections.Generic;
using System.Globalization;

namespace Json.Schema;

/// <summary>
/// Allows configuration of the evaluation process.
/// </summary>
public class EvaluationOptions
{
	private HashSet<Type>? _ignoredAnnotationTypes;

	/// <summary>
	/// The default settings.
	/// </summary>
	public static EvaluationOptions Default { get; } = new();

	/// <summary>
	/// Indicates whether the schema should be validated against its `$schema` value.
	/// this is not typically necessary.
	/// </summary>
	public bool ValidateAgainstMetaSchema { get; set; }
	/// <summary>
	/// Specifies the output format.
	/// </summary>
	public OutputFormat OutputFormat { get; set; }

	/// <summary>
	/// Specifies whether the `format` keyword should be required to provide
	/// validation results.  Default is false, which just produces annotations
	/// for drafts 2019-09 and prior or follows the behavior set forth by the
	/// format-annotation vocabulary requirement in the `$vocabulary` keyword in
	/// a meta-schema declaring draft 2020-12.
	/// </summary>
	public bool RequireFormatValidation { get; set; }

	/// <summary>
	/// If enabled, annotations that are dropped as a result of a failing
	/// subschema will be reported in a `droppedAnnotations` property in
	/// the output.
	/// </summary>
	public bool PreserveDroppedAnnotations { get; set; }

	/// <summary>
	/// Outputs an annotation that lists any unknown keywords.  Can be
	/// useful for catching typos.
	/// </summary>
	public bool AddAnnotationForUnknownKeywords { get; set; }

	/// <summary>
	/// Gets the set of keyword types from which annotations will be ignored.
	/// </summary>
	public IEnumerable<Type>? IgnoredAnnotations => _ignoredAnnotationTypes;

	/// <summary>
	/// Gets or sets the culture for error messages.  Overrides <see cref="ErrorMessages.Culture"/>.
	/// </summary>
	public CultureInfo? Culture { get; set; }

	/// <summary>
	/// Determines how floating point values are processed.
	/// </summary>
	public NumberProcessing NumberProcessing { get; set; }

	/// <summary>
	/// Determines how integer values are processed.
	/// </summary>
	public IntegerProcessing IntegerProcessing { get; set; }

	/// <summary>
	/// Creates a deep copy of the options.
	/// </summary>
	/// <param name="other">The source options.</param>
	/// <returns>A new options instance with the same settings.</returns>
	public static EvaluationOptions From(EvaluationOptions other)
	{
		var options = new EvaluationOptions
		{
			OutputFormat = other.OutputFormat,
			ValidateAgainstMetaSchema = other.ValidateAgainstMetaSchema,
			RequireFormatValidation = other.RequireFormatValidation,
			PreserveDroppedAnnotations = other.PreserveDroppedAnnotations,
			AddAnnotationForUnknownKeywords = other.AddAnnotationForUnknownKeywords,
			Culture = other.Culture,
			_ignoredAnnotationTypes = other._ignoredAnnotationTypes == null
				? null
				: new(other._ignoredAnnotationTypes),
		};
		return options;
	}

	/// <summary>
	/// Ignores annotations from the specified keyword.
	/// </summary>
	/// <typeparam name="T">The keyword type which should not have annotations.</typeparam>
	public void IgnoreAnnotationsFrom<T>()
		where T : IKeywordHandler
	{
		_ignoredAnnotationTypes ??= [];

		_ignoredAnnotationTypes.Add(typeof(T));
	}

	/// <summary>
	/// Ignores annotations from all keywords.
	/// </summary>
	public void IgnoreAllAnnotations()
	{
		//_ignoredAnnotationTypes = new HashSet<Type>(SchemaKeywordRegistry.KeywordTypes);
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
		where T : IKeywordHandler
	{
		_ignoredAnnotationTypes?.Remove(typeof(T));
	}
}