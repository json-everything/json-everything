# [1.1.1](https://github.com/gregsdennis/json-everything/pull/196)

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
