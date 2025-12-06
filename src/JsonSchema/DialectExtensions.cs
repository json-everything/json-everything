using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

/// <summary>
/// Provides extension methods for creating modified instances of the <see cref="Dialect"/> type by adding or removing
/// keyword handlers.
/// </summary>
/// <remarks>These methods enable fluent customization of <see cref="Dialect"/> objects without altering the
/// original instance. Use <see cref="With"/> to add keyword handlers and <see cref="Without"/> to remove them,
/// optionally overriding dialect properties such as <c>Id</c>, <c>RefIgnoresSiblingKeywords</c>, and
/// <c>AllowUnknownKeywords</c> in the resulting instance.</remarks>
public static class DialectExtensions
{
	/// <summary>
	/// Creates a new Dialect instance by extending the specified dialect with additional keyword handlers and optional
	/// configuration overrides.
	/// </summary>
	/// <remarks>This method is useful for creating customized dialects based on an existing one, allowing selective
	/// addition of keyword handlers and configuration overrides. The original dialect remains unchanged.</remarks>
	/// <param name="dialect">The base Dialect to extend. Cannot be null.</param>
	/// <param name="add">A collection of keyword handlers to add to the new Dialect. Cannot be null.</param>
	/// <param name="id">An optional identifier for the new Dialect. If null, the identifier from the base dialect is used.</param>
	/// <param name="refIgnoresSiblingKeywords">An optional value indicating whether '$ref' keywords ignore sibling keywords. If null, the value from the base
	/// dialect is used.</param>
	/// <param name="allowUnknownKeywords">An optional value indicating whether unknown keywords are allowed. If null, the value from the base dialect is
	/// used.</param>
	/// <returns>A new Dialect instance containing the combined keyword handlers and configuration settings.</returns>
	public static Dialect With(this Dialect dialect,
		IEnumerable<IKeywordHandler> add,
		Uri? id = null,
		bool? refIgnoresSiblingKeywords = null,
		bool? allowUnknownKeywords = null) =>
		new(dialect.GetKeywords().Concat(add))
		{
			Id = id,
			RefIgnoresSiblingKeywords = refIgnoresSiblingKeywords ?? dialect.RefIgnoresSiblingKeywords,
			AllowUnknownKeywords = allowUnknownKeywords ?? dialect.AllowUnknownKeywords
		};

	/// <summary>
	/// Creates a new Dialect instance by removing the specified keyword handlers from the given dialect.
	/// </summary>
	/// <remarks>The returned Dialect retains all configuration options from the source dialect unless overridden by
	/// the optional parameters. This method does not modify the original Dialect instance.</remarks>
	/// <param name="dialect">The source Dialect from which keyword handlers will be removed.</param>
	/// <param name="remove">A collection of keyword handlers to exclude from the resulting Dialect. Each handler in this collection will be
	/// omitted.</param>
	/// <param name="id">An optional identifier to assign to the new Dialect. If null, the identifier from the source dialect is not set.</param>
	/// <param name="refIgnoresSiblingKeywords">An optional value indicating whether '$ref' should ignore sibling keywords in the new Dialect. If null, the value
	/// from the source dialect is used.</param>
	/// <param name="allowUnknownKeywords">An optional value indicating whether unknown keywords are allowed in the new Dialect. If null, the value from the
	/// source dialect is used.</param>
	/// <returns>A new Dialect instance with the specified keyword handlers removed and updated configuration options as provided.</returns>
	public static Dialect Without(this Dialect dialect,
		IEnumerable<IKeywordHandler> remove,
		Uri? id = null,
		bool? refIgnoresSiblingKeywords = null,
		bool? allowUnknownKeywords = null) =>
		new(dialect.GetKeywords().Except(remove))
		{
			Id = id,
			RefIgnoresSiblingKeywords = refIgnoresSiblingKeywords ?? dialect.RefIgnoresSiblingKeywords,
			AllowUnknownKeywords = allowUnknownKeywords ?? dialect.AllowUnknownKeywords
		};
}