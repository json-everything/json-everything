using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Json.Schema;

public static class TaskEx
{
	public static async Task<Task<T>?> WhenAny<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate)
	{
		var list = tasks.ToList();
		T result;
		Task<T> task;
		do
		{
			task = await Task.WhenAny(list);
			result = task.Result;
			list.Remove(task);
		} while (list.Any() && !predicate(result));

		return list.Any() ? task : null;
	}
}