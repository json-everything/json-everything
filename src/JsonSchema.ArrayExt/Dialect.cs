using System;
using Json.Schema.ArrayExt.Keywords;

namespace Json.Schema.ArrayExt;

public static class Dialect
{
	public static readonly Uri ArrayExtId = new("https://json-everything.net/meta/vocab/array-ext");

	public static Schema.Dialect ArrayExt_202012 =
		Schema.Dialect.Draft202012.With([
				OrderingKeyword.Instance,
				UniqueKeysKeyword.Instance
			],
			ArrayExtId,
			false,
			true);
}