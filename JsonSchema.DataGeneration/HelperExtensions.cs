using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.DataGeneration
{
	internal static class HelperExtensions
	{
		public static string RequireOne(string pattern) => $@"(?=.*({pattern}))";
		private static string ForbidOne(string pattern) => $@"(?!.*({pattern}))";

		public static string Require(IEnumerable<string> patterns) => string.Concat(patterns.Select(RequireOne));
		public static string Forbid(IEnumerable<string> patterns) => string.Concat(patterns.Select(ForbidOne));
	}
}