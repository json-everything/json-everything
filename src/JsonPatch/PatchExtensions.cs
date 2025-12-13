using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		if (original is JsonObject originalObj && target is JsonObject targetObj)
			PatchForObject(originalObj, targetObj, patch, path);
		else if (original is JsonArray originalArr && target is JsonArray targetArr)
			PatchForArray(originalArr, targetArr, patch, path);
		else if (!original.IsEquivalentTo(target))
			patch.Add(PatchOperation.Replace(path, target));
	}

	private static void PatchForObject(JsonObject original, JsonObject target, List<PatchOperation> patch, JsonPointer path)
	{
		foreach (var (key, origValue) in original)
		{
			if (target.TryGetPropertyValue(key, out var targetValue))
			{
				// Original and Target keys intersect. Compare their values.
				CreatePatch(patch, origValue, targetValue, path.Combine(key));
			}
			else
			{
				// Original key is not in Target. Remove it.
				patch.Add(PatchOperation.Remove(path.Combine(key)));
			}
		}
		foreach (var (key, targetValue) in target)
		{
			if (!original.ContainsKey(key))
			{
				// Target key is not in Original. Add it.
				patch.Add(PatchOperation.Add(path.Combine(key), targetValue));
			}
		}
	}

	private static void PatchForArray(JsonArray original, JsonArray target, List<PatchOperation> patch, JsonPointer path)
	{
		if (target.Count >= original.Count)
		{
			for (int i = 0; i < target.Count; i++)
			{
				if (i >= original.Count)
				{
					patch.Add(PatchOperation.Add(path.Combine(i), target[i]));
					continue;
				}

				PatchForArrayIndex(i);
			}
		}
		else if (target.Count == 0)
		{
			patch.Add(PatchOperation.Replace(path, target));
		}
		else
		{
			int i = original.Count;
			while (--i >= 0)
			{
				var ui = (uint)i;
				if (i >= target.Count)
				{
					patch.Add(PatchOperation.Remove(path.Combine(i)));
					continue;
				}
				PatchForArrayIndex(i);
			}
		}

		void PatchForArrayIndex(int i)
		{
			var origValue = original[i];
			var modValue = target[i];

			CreatePatch(patch, origValue, modValue, path.Combine(i));
		}
	}

#if NETSTANDARD2_0
	/// <remarks>
	/// https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.keyvaluepair-2.deconstruct
	/// </remarks>
	private static void Deconstruct(this KeyValuePair<string, JsonNode?> kvp, out string key, out JsonNode? value)
	{
		key = kvp.Key;
		value = kvp.Value;
	}
#endif
}