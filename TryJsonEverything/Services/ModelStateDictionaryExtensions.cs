using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TryJsonEverything.Services
{
	public static class ModelStateDictionaryExtensions
	{
		public static IEnumerable<string> GetErrors(this ModelStateEntry entry)
		{
			foreach (var error in entry.Errors)
			{
				yield return error.ErrorMessage;
			}

			if (entry.Children == null || entry.Children.Count == 0) yield break;
			
			foreach (var child in entry.Children)
			{
				foreach (var error in GetErrors(child))
				{
					yield return error;
				}
			}
		}
	}
}
