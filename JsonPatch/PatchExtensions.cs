using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Patch
{
	public static class PatchExtensions
	{
		/// <summary>
		/// Apply patch to object
		/// </summary>
		/// <param name="patch">JsonPatch object</param>
		/// <param name="obj">Object to patch</param>
		/// <param name="options">Options</param>
		/// <typeparam name="T">Any object</typeparam>
		/// <returns>New instance of patched object</returns>
		public static T ApplyPatch<T>(this Json.Patch.JsonPatch patch, T obj, JsonSerializerOptions options = null)
		{
			using var doc = JsonDocument.Parse(JsonSerializer.Serialize(obj, options));
			var res = patch.Apply(doc.RootElement).Result;
			var result = JsonSerializer.Deserialize<T>(res.GetRawText(), options);
			return result;
		}
		/// <summary>
		/// Create JsonPatch from difference between original and modified
		/// </summary>
		/// <param name="originalObject">Original object</param>
		/// <param name="modifiedObject">Modified object</param>
		/// <param name="options">Json serializer options</param>
		/// <typeparam name="T">Any object</typeparam>
		/// <returns>JsonPatch</returns>
		public static JsonPatch CreatePatch<T>(this T originalObject, T modifiedObject, JsonSerializerOptions options = null)
        {
            var original = originalObject.ToJsonDocument(options);
            var modified = modifiedObject.ToJsonDocument(options);

            return CreatePatch(original, modified);
        }

		/// <summary>
		/// Create JsonPatch from difference between original and modified
		/// </summary>
		/// <param name="originalObject">Original object</param>
		/// <param name="modifiedObject">Modified object</param>
		/// <returns>JsonPatch</returns>
        public static JsonPatch CreatePatch(this JsonDocument originalObject, JsonDocument modifiedObject)
        {
            return CreatePatch(originalObject.RootElement, modifiedObject.RootElement);
        }

		/// <summary>
		/// Create JsonPatch from difference between original and modified
		/// </summary>
		/// <param name="originalObject">Original object</param>
		/// <param name="modifiedObject">Modified object</param>
		/// <returns>JsonPatch</returns>
		/// <exception cref="ArgumentException">Only Object and Array type json object allowed</exception>
        public static JsonPatch CreatePatch(this JsonElement originalObject, JsonElement modifiedObject)
        {
            var patch = new List<PatchOperation>();
            if (originalObject.ValueKind == JsonValueKind.Object)
                PatchForObject(originalObject, modifiedObject, patch, JsonPointer.Empty);
            else if (originalObject.ValueKind == JsonValueKind.Array)
                PatchForArray(originalObject, modifiedObject, patch, JsonPointer.Empty);
            else
                throw new ArgumentException($"{nameof(originalObject)} should be Object or Array Type.");
            return new JsonPatch(patch);
        }

        private static void PatchForObject(JsonElement orig, JsonElement mod, List<PatchOperation> patch, JsonPointer path)
        {
            var origNames = orig.EnumerateObject().Select(x => x.Name).ToArray();
            var modNames = mod.EnumerateObject().Select(x => x.Name).ToArray();

            foreach (var k in origNames.Except(modNames))
                patch.Add(PatchOperation.Remove(path.Combine(k)));

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
                {
                    patch.Add(PatchOperation.Replace(JsonPointer.Parse(path + modProp.Name), modProp.Value));
                }
                else if (!string.Equals(origProp.Value.ToString(), modProp.Value.ToString()))
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
                if (i >= orig.GetArrayLength())
                {
                    patch.Add(PatchOperation.Add(path.Combine(i), mod[i]));
                    continue;
                }

                if (i >= mod.GetArrayLength())
                {
                    patch.Add(PatchOperation.Remove(path.Combine(i)));
                    continue;
                }

                var origObject = orig[i];
                var modObject = mod[i];

                if (origObject.ValueKind != modObject.ValueKind)
                {
                    patch.Add(PatchOperation.Replace(path.Combine("/" + i), modObject));
                }
                else if (!string.Equals(origObject.ToString(), modObject.ToString()))
                {
                    if (origObject.ValueKind == JsonValueKind.Object)
                        PatchForObject(origObject, modObject, patch, path.Combine(i));
                    else if (origObject.ValueKind == JsonValueKind.Array)
                        PatchForArray(origObject, modObject, patch, path.Combine(i));
                    else
                        patch.Add(PatchOperation.Replace(path.Combine(i), modObject));
                }
            }
        }
	}
}