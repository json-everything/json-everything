using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

internal static class GeneralExtensions
{
	public static decimal ClampToDecimal(this double value)
	{
		return value <= (double)decimal.MinValue
			? decimal.MinValue
			: (double)decimal.MaxValue <= value
				? decimal.MaxValue
				: Convert.ToDecimal(value);
	}

	public static List<ISchemaKeywordIntent> AsNullable(this List<ISchemaKeywordIntent> source)
	{
		var newList = new List<ISchemaKeywordIntent>(source);
		
		var enumIntent = newList.OfType<EnumIntent>().FirstOrDefault();
		if (enumIntent is not null)
		{
			var index = newList.IndexOf(enumIntent);
			var newIntent = new EnumIntent([..enumIntent.Names, (JsonNode?)null]);
			newList[index] = newIntent;
		}

		var typeIntent = newList.OfType<TypeIntent>().FirstOrDefault();
		if (typeIntent is not null)
		{
			var index = newList.IndexOf(typeIntent);
			var newIntent = new TypeIntent(typeIntent.Type | SchemaValueType.Null);
			newList[index] = newIntent;
		}

		return newList;
	}
}