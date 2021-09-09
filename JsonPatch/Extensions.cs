using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Patch
{
	public static class Extensions
	{
		/// <summary>
		/// Create list of PatchOperation from difference between original and modified
		/// </summary>
		/// <param name="originalObject">Original object</param>
		/// <param name="modifiedObject">Modified object</param>
		/// <param name="options">Json serializer options</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<PatchOperation> CreatePatch<T>(this T originalObject,
            T modifiedObject,
            JsonSerializerOptions options = null)
        {
            var original = originalObject.ToJsonDocument(options);
            var modified = modifiedObject.ToJsonDocument(options);

            return CreatePatch(original, modified);
        }

		/// <summary>
		/// Create list of PatchOperation from difference between original and modified
		/// </summary>
		/// <param name="originalObject">Original object</param>
		/// <param name="modifiedObject">Modified object</param>
		/// <returns></returns>
        public static List<PatchOperation> CreatePatch(this JsonDocument originalObject, JsonDocument modifiedObject)
        {
            return CreatePatch(originalObject.RootElement, modifiedObject.RootElement);
        }

		/// <summary>
		/// Create list of PatchOperation from difference between original and modified
		/// </summary>
		/// <param name="originalObject">Original object</param>
		/// <param name="modifiedObject">Modified object</param>
		/// <returns></returns>
        public static List<PatchOperation> CreatePatch(this JsonElement originalObject, JsonElement modifiedObject)
        {
            var patch = new List<PatchOperation>();
            if (originalObject.ValueKind == JsonValueKind.Object)
                PatchForObject(originalObject, modifiedObject, patch, "/");
            else if (originalObject.ValueKind == JsonValueKind.Array)
                PatchForArray(originalObject, modifiedObject, patch, "/");
            else
                throw new ArgumentException($"{nameof(originalObject)} should be Object or Array Type.");
            return patch;
        }

		/// <summary>
		/// Append PatchOperations between objects
		/// </summary>
		/// <param name="orig">Original object</param>
		/// <param name="mod">Modified object</param>
		/// <param name="patch">Existing list of PatchOperations</param>
		/// <param name="path">Current JsonPointer path</param>
        private static void PatchForObject(JsonElement orig,
            JsonElement mod,
            List<PatchOperation> patch,
            string path)
        {
            var origNames = orig.EnumerateObject().Select(x => x.Name).ToArray();
            var modNames = mod.EnumerateObject().Select(x => x.Name).ToArray();

            foreach (var k in origNames.Except(modNames))
                patch.Add(PatchOperation.Remove(JsonPointer.Parse(path + k)));

            foreach (var k in modNames.Except(origNames))
            {
                var prop = mod.EnumerateObject().First(p => p.NameEquals(k));
                patch.Add(PatchOperation.Add(JsonPointer.Parse(path + k), prop.Value));
            }

            foreach (var k in origNames.Intersect(modNames))
            {
                var origProp = orig.EnumerateObject().First(p => p.NameEquals(k));
                var modProp = mod.EnumerateObject().First(p => p.NameEquals(k));

                if (origProp.Value.ValueKind != modProp.Value.ValueKind)
                {
                    patch.Add(PatchOperation.Replace(JsonPointer.Parse(path + modProp.Name), modProp.Value));
                }
                else if (!string.Equals(
                    origProp.Value.ToString(),
                    modProp.Value.ToString()))
                {
                    if (origProp.Value.ValueKind == JsonValueKind.Object)
                        PatchForObject(origProp.Value, modProp.Value, patch, path + modProp.Name + "/");
                    else if (origProp.Value.ValueKind == JsonValueKind.Array)
                        PatchForArray(origProp.Value, modProp.Value, patch, path + modProp.Name + "/");
                    else
                        patch.Add(PatchOperation.Replace(JsonPointer.Parse(path + modProp.Name), modProp.Value));
                }
            }
        }
		
		/// <summary>
		/// Append PatchOperations between arrays
		/// </summary>
		/// <param name="orig">Original object</param>
		/// <param name="mod">Modified object</param>
		/// <param name="patch">Existing list of PatchOperations</param>
		/// <param name="path">Current JsonPointer path</param>
        private static void PatchForArray(JsonElement orig,
            JsonElement mod,
            List<PatchOperation> patch,
            string path)
        {
            for (int i = 0; i < Math.Max(orig.GetArrayLength(), mod.GetArrayLength()); i++)
            {
                if (i >= orig.GetArrayLength())
                {
                    patch.Add(PatchOperation.Add(JsonPointer.Parse(path + i), mod[i]));
                    continue;
                }

                if (i >= mod.GetArrayLength())
                {
                    patch.Add(PatchOperation.Remove(JsonPointer.Parse(path + i)));
                    continue;
                }

                var origObject = orig[i];
                var modObject = mod[i];

                if (origObject.ValueKind != modObject.ValueKind)
                {
                    patch.Add(PatchOperation.Replace(JsonPointer.Parse(path + "/" + i), modObject));
                }
                else if (!string.Equals(
                    origObject.ToString(),
                    modObject.ToString()))
                {
                    if (origObject.ValueKind == JsonValueKind.Object)
                        PatchForObject(origObject, modObject, patch, path + i + "/");
                    else if (origObject.ValueKind == JsonValueKind.Array)
                        PatchForArray(origObject, modObject, patch, path + i + "/");
                    else
                        patch.Add(PatchOperation.Replace(JsonPointer.Parse(path + i), modObject));
                }
            }
        }
	}
}