using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema;

public static class DialectExtensions
{
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