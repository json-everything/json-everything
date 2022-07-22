# 3.0.1 (no PR)

[#302](https://github.com/gregsdennis/json-everything/issues/302) - Predefined rule types should be public to enable analysis.

[#286](https://github.com/gregsdennis/json-everything/pull/286) - Remainder of operators aligned to function like the pre-existing playground.

# 3.0.1 (no PR)

[#286](https://github.com/gregsdennis/json-everything/pull/286) - `in` operation throws exception when second arg is not array.  Original implementation returns false.

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.


# [3.0.0](https://github.com/gregsdennis/json-everything/issues/280)

Updated all functionality to use `JsonNode` instead of `JsonElement`.

## Breaking Changes

_`JsonElement` -> `JsonNode` type exchange changes not listed._

- Remove all `JsonLogic.Literal()` methods and replaced with a single `Liternal(JsonNode?)`.  Typed methods are unnecessary as `JsonNode` contains implicit casts from them.
- `JsonElementExtensions` converted into `JsonNodeExtensions`.  Same functionality exists, but for `JsonNode` instead.
- `Rule` and all of its subclasses now take and return `JsonNode?`

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/265)

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

[#263](https://github.com/gregsdennis/json-everything/issues/263) - `{"var": ""}` is used by some operations to pass context data into inner rules, but the external data is also available.

## Breaking Changes

- Added optional parameter to `Rule.Apply()` in order to supply context data.
- `"var"` will now prioritize context data over external data.  If the path yields no result for context data, it will search the external data.  This means that if both the context data and the external data have the given path, the context data will be used.

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