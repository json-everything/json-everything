# [2.0.4](https://github.com/gregsdennis/json-everything/pull/323)

[#322](https://github.com/gregsdennis/json-everything/pull/322) - [@z4kn4fein](https://github.com/z4kn4fein) discovered and fixed an issue in the `move` operation logic.

# [2.0.3](https://github.com/gregsdennis/json-everything/pull/317)

[#315](https://github.com/gregsdennis/json-everything/pull/315) - [@z4kn4fein](https://github.com/z4kn4fein) noticed that the serializer options weren't actually being passed into the `.Apply()` call.

# 2.0.2 (no PR)

[#291](https://github.com/gregsdennis/json-everything/pull/291) - Improved patch generation for arrays.

# 2.0.1 (no PR)

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/280)

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

Updated all functionality to use `JsonNode` instead of `JsonElement`.

## Breaking Changes

_`JsonElement` -> `JsonNode` type exchange changes not listed._

- `JsonPatch.Apply()` now takes `JsonNode`
- `.CreatePatch(JsonDocument, JsonDocument)` removed
- `.CreatePatch(JsonElement, JsonElement)` replaced with `.CreatePatch(JsonNode?, JsonNode?)`
- `PatchOperation` converted to a class
- `PatchOperation` static methods which take `JsonElementProxy` removed as `JsonNode` defines implicit casts for the supported types
- `PatchResult.Result` update to `JsonNode?`

## Additional Changes

- `.Apply<T>()` extension method now takes optional serializer options

# [1.1.2](https://github.com/gregsdennis/json-everything/pull/196)

[#192](https://github.com/gregsdennis/json-everything/pull/192) - [@LordXaosa](https://github.com/LordXaosa) found some issues with patch generation.

# [1.1.1](https://github.com/gregsdennis/json-everything/pull/179)

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/163)

[#160](https://github.com/gregsdennis/json-everything/pull/160) - Added JSON Patch creation via comparison of objects or JSON data.  Credit for implementation to [@LordXaosa](https://github.com/LordXaosa).

Added `JsonElementProxy` overloads for `PatchOperation.Add()`, `PatchOperation.Replace()`, and `PatchOperation.Test()`.

# [1.0.6](https://github.com/gregsdennis/json-everything/pull/147)

[#132](https://github.com/gregsdennis/json-everything/pull/132) (Fixed on [#133](https://github.com/gregsdennis/json-everything/pull/133)) - Fixed some memory management issues around `JsonDocument` and `JsonElement`.  Thanks to [@ddunkin](https://github.com/ddunkin) for finding and fixing these.

[#146](https://github.com/gregsdennis/json-everything/issues/146) - Fixed an issue during operation construction that appeared when attempting to use JSON Patch in an MVC controller.

# [1.0.5](https://github.com/gregsdennis/json-everything/pull/75)

Added support for nullable reference types.

# [1.0.4](https://github.com/gregsdennis/json-everything/pull/61)

Signed the DLL for strong name compatibility.

# [1.0.3](https://github.com/gregsdennis/json-everything/commit/4b6c5900f4bfb45119a3dc5c3ce60b7d7a2e8c9e)

Bump for publish.  No functional change.

# [1.0.2](https://github.com/gregsdennis/json-everything/pull/45)

Added debug symbols to package.  No functional change.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/26)

Implemented patch equality.

# 1.0.0

Initial release.
