using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Json.Schema;

public static class TaskExtensions
{
	public static async Task<Task<T>?> WhenAny<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate, CancellationToken token)
	{
		var list = tasks.ToList();
		while (!token.IsCancellationRequested && list.Any())
		{
			var task = await Task.WhenAny(list);
			var result = task.Result;
			list.Remove(task);

			if (predicate(result)) return task;
		}

		return null;
	}
}