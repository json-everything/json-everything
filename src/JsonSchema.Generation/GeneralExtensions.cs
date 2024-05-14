using System;

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
}