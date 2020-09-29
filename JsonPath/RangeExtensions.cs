using System;

namespace Json.Path
{
	internal static class RangeExtensions
	{
		public static string ToPathString(this Index index)
		{
			return index.IsFromEnd ? $"-{index.Value}" : index.Value.ToString();
		}
	}
}