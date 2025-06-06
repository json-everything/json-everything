using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Pointer;

namespace Json.Patch;

/// <summary>
/// Provides extension methods for <see cref="JsonPatch"/>.
/// </summary>
public static class PatchExtensions
{
	private const string _aotMessage = "To ensure AOT compatibility, be sure to add an IJsonTypeInfoResolver to the options resolver chain for the object type.";

	/// <summary>
	/// Apply a patch to an object to transform its data.
	/// </summary>
	/// <param name="patch">The patch</param>
	/// <param name="obj">The object</param>
	/// <param name="options">Serializer options.</param>
	/// <typeparam name="T">The type of the object.</typeparam>
	/// <returns>New instance of patched object</returns>
	/// <exception cref="InvalidOperationException">Thrown when the patch cannot be applied.</exception>
	[RequiresDynamicCode(_aotMessage)]
	[RequiresUnreferencedCode(_aotMessage)]
	public static T? Apply<T>(this JsonPatch patch, T obj, JsonSerializerOptions? options = null)
	{
		return Apply<T, T>(patch, obj, options);
	}

	/// <summary>
	/// Apply a patch to an object to transform it to another type.
	/// </summary>
	/// <param name="patch">The patch</param>
	/// <param name="obj">The object</param>
	/// <param name="options">Serializer options.</param>
	/// <typeparam name="TOriginal">The type of the original object.</typeparam>
	/// <typeparam name="TTarget">The type of the target object.</typeparam>
	/// <returns>New instance of patched object</returns>
	/// <exception cref="InvalidOperationException">Thrown when the patch cannot be applied.</exception>
	[RequiresDynamicCode(_aotMessage)]
	[RequiresUnreferencedCode(_aotMessage)]
	public static TTarget? Apply<TOriginal, TTarget>(this JsonPatch patch, TOriginal obj, JsonSerializerOptions? options = null)
	{
		var node = JsonSerializer.SerializeToNode(obj, options);
		var patchResult = patch.Apply(node);
		if (!patchResult.IsSuccess)
			throw new InvalidOperationException($"{patchResult.Error} Operation: {patchResult.Operation}");
		var result = patchResult.Result.Deserialize<TTarget>(options);
		return result;
	}

	/// <summary>
	/// Creates a <see cref="JsonPatch"/> by analyzing the difference between original and target versions.
	/// </summary>
	/// <param name="original">The original object.</param>
	/// <param name="target">The target object.</param>
	/// <param name="options">Serializer options.</param>
	/// <typeparam name="TOriginal">The type of the original object.</typeparam>
	/// <typeparam name="TTarget">The type of target object.</typeparam>
	/// <returns>A <see cref="JsonPatch"/> that represents the transformation.</returns>
	[RequiresDynamicCode(_aotMessage)]
	[RequiresUnreferencedCode(_aotMessage)]
	public static JsonPatch CreatePatch<TOriginal, TTarget>(this TOriginal original, TTarget target, JsonSerializerOptions? options = null)
	{
		var originalJson = JsonSerializer.SerializeToNode(original, options);
		var targetJson = JsonSerializer.SerializeToNode(target, options);

		return CreatePatch(originalJson, targetJson);
	}

	/// <summary>
	/// Create JsonPatch from difference between original and target
	/// </summary>
	/// <param name="original">Original object</param>
	/// <param name="target">target object</param>
	/// <returns>JsonPatch</returns>
	/// <exception cref="ArgumentException">Only Object and Array type json object allowed</exception>
	public static JsonPatch CreatePatch(this JsonNode? original, JsonNode? target)
	{
		var patch = new List<PatchOperation>();
		CreatePatch(patch, original, target, JsonPointer.Empty);
		return new JsonPatch(patch);
	}

	private static void CreatePatch(List<PatchOperation> patch, JsonNode? original, JsonNode? target, JsonPointer path)
	{
		if (ReferenceEquals(original, target)) return;

		switch (original, target)
		{
			case (JsonObject origObj, JsonObject targetObj):
				PatchForObject(origObj, targetObj, patch, path);
				break;
			case (JsonArray origArr, JsonArray targetArr):
				PatchForArray(origArr, targetArr, patch, path);
				break;
			case (null, null):
				return;
			case (null, _):
				patch.Add(PatchOperation.Add(path, target));
				break;
			case (_, null):
				patch.Add(PatchOperation.Remove(path));
				break;
			default:
				if (!JsonNode.DeepEquals(original, target))
				{
					patch.Add(PatchOperation.Replace(path, target));
				}
				break;
		}
	}

	private static void PatchForObject(JsonObject original, JsonObject target, List<PatchOperation> patch, JsonPointer path)
	{
		// Handle removed and modified properties
		foreach (var kvp in original)
		{
			var newPath = path.Combine(JsonPointer.Create(kvp.Key));
			if (!target.TryGetPropertyValue(kvp.Key, out var targetValue))
			{
				patch.Add(PatchOperation.Remove(newPath));
			}
			else
			{
				CreatePatch(patch, kvp.Value, targetValue, newPath);
			}
		}

		// Handle added properties
		foreach (var kvp in target)
		{
			if (!original.TryGetPropertyValue(kvp.Key, out _))
			{
				var newPath = path.Combine(JsonPointer.Create(kvp.Key));
				patch.Add(PatchOperation.Add(newPath, kvp.Value));
			}
		}
	}

	private static void PatchForArray(JsonArray original, JsonArray target, List<PatchOperation> patch, JsonPointer path)
	{
		// If arrays are identical, no patch needed
		if (JsonNode.DeepEquals(original, target)) return;

		// For small arrays, use simple index-based comparison
		if (original.Count <= 10 && target.Count <= 10)
		{
			SimpleArrayPatch(original, target, patch, path);
			return;
		}

		// For larger arrays, use a more sophisticated diff algorithm
		var lcs = ComputeLongestCommonSubsequence(original, target);
		var operations = GenerateArrayOperations(original, target, lcs);

		// Apply operations in reverse order to maintain correct indices
		foreach (var op in operations.Reverse())
		{
			switch (op.Type)
			{
				case "add":
					patch.Add(PatchOperation.Add(path.Combine(JsonPointer.Parse($"/{op.Index}")), op.Value));
					break;
				case "remove":
					patch.Add(PatchOperation.Remove(path.Combine(JsonPointer.Parse($"/{op.Index}"))));
					break;
				case "replace":
					patch.Add(PatchOperation.Replace(path.Combine(JsonPointer.Parse($"/{op.Index}")), op.Value));
					break;
			}
		}
	}

	private static void SimpleArrayPatch(JsonArray original, JsonArray target, List<PatchOperation> patch, JsonPointer path)
	{
		// If target is empty and original is not, replace the entire array
		if (target.Count == 0 && original.Count > 0)
		{
			patch.Add(PatchOperation.Replace(path, target));
			return;
		}

		var minLength = Math.Min(original.Count, target.Count);
		
		// Compare elements up to the minimum length
		for (int i = 0; i < minLength; i++)
		{
			var newPath = path.Combine(JsonPointer.Parse($"/{i}"));
			CreatePatch(patch, original[i], target[i], newPath);
		}

		// Handle remaining elements
		if (original.Count > target.Count)
		{
			// If we need to remove multiple elements, replace the entire array
			if (original.Count - target.Count > 1)
			{
				patch.Add(PatchOperation.Replace(path, target));
			}
			else
			{
				// Remove single element
				var newPath = path.Combine(JsonPointer.Parse($"/{target.Count}"));
				patch.Add(PatchOperation.Remove(newPath));
			}
		}
		else if (target.Count > original.Count)
		{
			for (int i = original.Count; i < target.Count; i++)
			{
				var newPath = path.Combine(JsonPointer.Parse($"/{i}"));
				patch.Add(PatchOperation.Add(newPath, target[i]));
			}
		}
	}

	private static List<(int OriginalIndex, int TargetIndex)> ComputeLongestCommonSubsequence(JsonArray original, JsonArray target)
	{
		var matrix = new int[original.Count + 1, target.Count + 1];
		var lcs = new List<(int OriginalIndex, int TargetIndex)>();

		// Fill the LCS matrix
		for (int i = 1; i <= original.Count; i++)
		{
			for (int j = 1; j <= target.Count; j++)
			{
				if (JsonNode.DeepEquals(original[i - 1], target[j - 1]))
				{
					matrix[i, j] = matrix[i - 1, j - 1] + 1;
				}
				else
				{
					matrix[i, j] = Math.Max(matrix[i - 1, j], matrix[i, j - 1]);
				}
			}
		}

		// Backtrack to find the LCS
		int x = original.Count, y = target.Count;
		while (x > 0 && y > 0)
		{
			if (JsonNode.DeepEquals(original[x - 1], target[y - 1]))
			{
				lcs.Add((x - 1, y - 1));
				x--; y--;
			}
			else if (matrix[x - 1, y] > matrix[x, y - 1])
			{
				x--;
			}
			else
			{
				y--;
			}
		}

		lcs.Reverse();
		return lcs;
	}

	private static IEnumerable<(string Type, int Index, JsonNode? Value)> GenerateArrayOperations(
		JsonArray original, 
		JsonArray target, 
		List<(int OriginalIndex, int TargetIndex)> lcs)
	{
		var operations = new List<(string Type, int Index, JsonNode? Value)>();

		// Process removals and replacements
		for (int i = 0; i < original.Count; i++)
		{
			var lcsItem = lcs.FirstOrDefault(x => x.OriginalIndex == i);
			if (lcsItem.OriginalIndex == i)
			{
				// Element is in LCS, check if it needs replacement
				if (!JsonNode.DeepEquals(original[i], target[lcsItem.TargetIndex]))
				{
					operations.Add(("replace", i, target[lcsItem.TargetIndex]));
				}
			}
			else
			{
				// Element is not in LCS, remove it
				operations.Add(("remove", i, null));
			}
		}

		// Process additions
		for (int i = 0; i < target.Count; i++)
		{
			if (!lcs.Any(x => x.TargetIndex == i))
			{
				operations.Add(("add", i, target[i]));
			}
		}

		return operations;
	}
}