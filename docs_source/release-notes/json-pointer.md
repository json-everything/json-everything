# [2.1.0](https://github.com/gregsdennis/json-everything/pull/185)

[#184](https://github.com/gregsdennis/json-everything/issues/184) - [@JanKnipp](https://github.com/JanKnipp) added array (e.g. `int[]`) support to Linq expression construction of pointers.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/179)

[#171](https://github.com/gregsdennis/json-everything/issues/171) - Updated `JsonPointer`, `PointerSegment`, and `RelativeJsonPointer` from struct to class since it holds by-reference data anyway.  Removes parameterless constructor for these types.  These changes make them better immutable models.

# [1.4.0](https://github.com/gregsdennis/json-everything/pull/162)

- Added implicit casts from `int` and `string` to `PointerSegment`
- Added params overload for `JsonPointer.Create()`.

# [1.3.3](https://github.com/gregsdennis/json-everything/pull/130)

[#123](https://github.com/gregsdennis/json-everything/pull/123) - Removed a copy/paste error that shows up while deserializing relative pointers.  Thanks to [@bastiaantenklooster](https://github.com/bastiaantenklooster) for finding this and creating a PR to fix it.

# [1.3.2](https://github.com/gregsdennis/json-everything/pull/75)

Added support for nullable reference types.

# [1.3.1](https://github.com/gregsdennis/json-everything/pull/61)

Signed the DLL for strong name compatibility.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/58)

Added support pointer creation using lambda expressions, e.g. `x => x.foo[5].bar` to create `/foo/5/bar`.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/52)

Added support for array index manipulation in Relative JSON Pointer 2 (spec updated alongside JSON Schema Draft 2020-12).

# [1.1.2](https://github.com/gregsdennis/json-everything/pull/45)

Added debug symbols to package.  No functional change.

# [1.1.1](https://github.com/gregsdennis/json-everything/pull/12)

Updated reference to Json.More<nsp>.Net.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/7)

Added `JsonPointerKind` enumeration.

Added `JsonPointer.Kind` property.

Added optional parameters to `JsonPointer.Parse()` and `JsonPointer.TryParse()`.

# 1.0.0

Initial release.
