using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Json.Schema;

/// <summary>
/// Custom task management functions.
/// </summary>
public static class TaskExtensions
{
	/// <summary>
	/// Runs a collection of tasks and returns when the first that meets a given criteria completes.
	/// </summary>
	/// <typeparam name="T">The return type of the tasks.</typeparam>
	/// <param name="tasks">The tasks.</param>
	/// <param name="predicate">The return condition, based on the return type of the tasks.</param>
	/// <param name="token">A cancellation token.</param>
	/// <returns>The task which completed with a return type that passes the criteria.</returns>
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