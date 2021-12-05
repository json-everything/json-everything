using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Patch
{
	/// <summary>
	/// Provides extension methods for <see cref="JsonPatch"/>.
	/// </summary>
	public static class PatchExtensions
	{
		/// <summary>
		/// Apply a patch to an object to transform its data.
		/// </summary>
		/// <param name="patch">The patch</param>
		/// <param name="obj">The object</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		/// <returns>New instance of patched object</returns>
		/// <exception cref="InvalidOperationException">Thrown when the patch cannot be applied.</exception>
		public static T Apply<T>(this JsonPatch patch, T obj)
		{
			return Apply<T, T>(patch, obj);
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
		public static TTarget Apply<TOriginal, TTarget>(this JsonPatch patch, TOriginal obj, JsonSerializerOptions? options = null)
		{
			using var doc = JsonDocument.Parse(JsonSerializer.Serialize(obj, options));
			var patchResult = patch.Apply(doc.RootElement);
			if (!patchResult.IsSuccess)
				throw new InvalidOperationException($"{patchResult.Error} Operation: {patchResult.Operation}");
			var result = JsonSerializer.Deserialize<TTarget>(patchResult.Result.GetRawText(), options);
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
		public static JsonPatch CreatePatch<TOriginal, TTarget>(this TOriginal original, TTarget target, JsonSerializerOptions? options = null)
		{
			var originalJson = original.ToJsonDocument(options);
			var targetJson = target.ToJsonDocument(options);

			return CreatePatch(originalJson, targetJson);
		}

		/// <summary>
		/// Create a <see cref="JsonPatch"/> from difference between original and target
		/// </summary>
		/// <param name="originalObject">Original object</param>
		/// <param name="targetObject">target object</param>
		/// <returns>JsonPatch</returns>
		public static JsonPatch CreatePatch(this JsonDocument originalObject, JsonDocument targetObject)
		{
			return CreatePatch(originalObject.RootElement, targetObject.RootElement);
		}

		/// <summary>
		/// Create JsonPatch from difference between original and target
		/// </summary>
		/// <param name="originalObject">Original object</param>
		/// <param name="targetObject">target object</param>
		/// <returns>JsonPatch</returns>
		/// <exception cref="ArgumentException">Only Object and Array type json object allowed</exception>
		public static JsonPatch CreatePatch(this JsonElement originalObject, JsonElement targetObject)
		{
			var patch = new List<PatchOperation>();
			if (originalObject.ValueKind == JsonValueKind.Object)
				PatchForObject(originalObject, targetObject, patch, JsonPointer.Empty);
			else if (originalObject.ValueKind == JsonValueKind.Array)
				PatchForArray(originalObject, targetObject, patch, JsonPointer.Empty);
			else
				throw new ArgumentException($"{nameof(originalObject)} should be Object or Array Type.");
			return new JsonPatch(patch);
		}

		private static void PatchForObject(JsonElement orig, JsonElement mod, List<PatchOperation> patch, JsonPointer path)
		{
			var origNames = orig.EnumerateObject().Select(x => x.Name).ToArray();
			var modNames = mod.EnumerateObject().Select(x => x.Name).ToArray();

			foreach (var k in origNames.Except(modNames))
			{
				patch.Add(PatchOperation.Remove(path.Combine(k)));
			}

			foreach (var k in modNames.Except(origNames))
			{
				var prop = mod.EnumerateObject().First(p => p.NameEquals(k));
				patch.Add(PatchOperation.Add(path.Combine(k), prop.Value));
			}

			foreach (var k in origNames.Intersect(modNames))
			{
				var origProp = orig.EnumerateObject().First(p => p.NameEquals(k));
				var modProp = mod.EnumerateObject().First(p => p.NameEquals(k));

				if (origProp.Value.ValueKind != modProp.Value.ValueKind)
					patch.Add(PatchOperation.Replace(path.Combine(modProp.Name), modProp.Value));
				else if (!string.Equals(origProp.Value.ToString(), modProp.Value.ToString())) // TODO
				{
					if (origProp.Value.ValueKind == JsonValueKind.Object)
						PatchForObject(origProp.Value, modProp.Value, patch, path.Combine(modProp.Name));
					else if (origProp.Value.ValueKind == JsonValueKind.Array)
						PatchForArray(origProp.Value, modProp.Value, patch, path.Combine(modProp.Name));
					else
						patch.Add(PatchOperation.Replace(path.Combine(modProp.Name), modProp.Value));
				}
			}
		}

		private static void PatchForArray(JsonElement orig, JsonElement mod, List<PatchOperation> patch, JsonPointer path)
		{
			for (int i = 0; i < Math.Max(orig.GetArrayLength(), mod.GetArrayLength()); i++)
			{
				var ui = (uint) i;
				if (i >= orig.GetArrayLength())
				{
					patch.Add(PatchOperation.Add(path.Combine(ui), mod[i]));
					continue;
				}

				if (i >= mod.GetArrayLength())
				{
					patch.Add(PatchOperation.Remove(path.Combine(ui)));
					continue;
				}

				var origObject = orig[i];
				var modObject = mod[i];

				if (origObject.ValueKind != modObject.ValueKind)
					patch.Add(PatchOperation.Replace(path.Combine("/" + i), modObject));
				else if (!string.Equals(origObject.ToString(), modObject.ToString())) // TODO
				{
					if (origObject.ValueKind == JsonValueKind.Object)
						PatchForObject(origObject, modObject, patch, path.Combine(ui));
					else if (origObject.ValueKind == JsonValueKind.Array)
						PatchForArray(origObject, modObject, patch, path.Combine(ui));
					else
						patch.Add(PatchOperation.Replace(path.Combine(ui), modObject));
				}
			}
		}
	}
}