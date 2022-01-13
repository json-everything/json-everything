# [1.4.0](https://github.com/gregsdennis/json-everything/pull/205)

[#183](https://github.com/gregsdennis/json-everything/pull/183) - Added handling of object truthiness: empty objects are falsy; non-empty are truthy.  This behavior isn't specified, but emulates the original JS library.

# [1.3.2](https://github.com/gregsdennis/json-everything/pull/182)

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.3.1](https://github.com/gregsdennis/json-everything/pull/133)

[#132](https://github.com/gregsdennis/json-everything/pull/132) - Fixed some memory management issues around `JsonDocument` and `JsonElement`.  Thanks to [@ddunkin](https://github.com/ddunkin) for finding and fixing these.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/92)

- Exposed `JsonElementExtensions` so that it can be used in custom rules.
- Removed `new()` requirement from `RuleRegistry.AddRule<T>()`.

# [1.2.1](https://github.com/gregsdennis/json-everything/pull/75)

Added support for nullable reference types.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/64)

Added the ability to define and register custom rules.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/62)

Prematurely released library.  It would parse and process fine, but the factory methods for building inline logic were incomplete.  Also added XML comments for everything.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/61)

Signed the DLL for strong name compatibility.

# 1.0.0

Initial release.