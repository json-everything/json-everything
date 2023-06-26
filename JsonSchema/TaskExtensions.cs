using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Json.Schema;

public static class TaskExtensions
{
	public static async Task<Task<T>?> WhenAny<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate)
	{
		var list = tasks.ToList();
		while (list.Any())
		{
			var task = await Task.WhenAny(list);
			var result = task.Result;
			list.Remove(task);

			if (predicate(result)) return task;
		}

		return null;
	}
}