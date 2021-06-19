# [1.3.3](https://github.com/gregsdennis/json-everything/pull/130)

[#123](https://github.com/gregsdennis/json-everything/pull/123) - Removed a copy/paste error that shows up while deserializing relative pointers.  Thanks to [@bastiaantenklooster](https://github.com/bastiaantenklooster) for finding this and creating a PR to fix it.

# [1.3.2](https://github.com/gregsdennis/json-everything/pull/75)

Added support for nullable reference types.

# [1.3.1](https://github.com/gregsdennis/json-everything/pull/61)

Signed the DLL for strong name compatibility.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/???)

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
