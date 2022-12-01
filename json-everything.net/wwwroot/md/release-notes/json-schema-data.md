# [3.1.0 (beta 1)](https://github.com/gregsdennis/json-everything/pull/326)

Updated to use JsonSchema.Net v4.

# [3.0.1](https://github.com/gregsdennis/json-everything/pull/316)

Fixed bug where URI-encoded JSON Pointers where being handled as instance locations instead of schema locations.  (Thanks new [test suite](https://github.com/gregsdennis/json-schema-vocab-test-suites)!)

# [3.0.0](https://github.com/gregsdennis/json-everything/pull/316)

Supports the 2022 version of the vocabulary.  This is a breaking change from the previous version.

## Changes

References no longer just support URIs.  Instead several formats are used to indicate the source document:

- JSON Pointers (not URI fragments) and Relative JSON Pointers indicate locations within the instance
- Fragment-only URI-encoded JSON Pointers indicate locations within the host schema (just like a `$ref` would)
- Absolute URIs (optionally with URI-encoded JSON Pointer fragments) are used to access external data

The first point is the primary change.

## Migration

URI-encoded JSON Pointer fragments used to resolve to the instance first.  These will need to be changed to plain JSON Pointers.

```
From
#/foo/1/bar

To
/foo/1/bar
```

Absolute URIs that indicate the host schema may be changed to URI-encoded JSON Pointer fragments.  This is not strictly necessary as the resolution of the full URI will still occur.

```
From
https://example.com/schema#/$defs/somedef

To
#/$defs/somedef
```

# 2.0.1 (no PR)

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/280)

Updated all functionality to use `JsonNode` instead of `JsonElement` in line with JsonSchema.Net v3.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/249)

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

[#190](https://github.com/gregsdennis/json-everything/issues/190) - Added support for custom and localized error messages.

# [1.0.5](https://github.com/gregsdennis/json-everything/pull/200)

Updated JsonSchema.Net reference to v2.0.0.  Please see [release notes](./json-schema.md) for that library as it contains breaking changes.

# [1.0.4](https://github.com/gregsdennis/json-everything/pull/???)

???

# [1.0.3](https://github.com/gregsdennis/json-everything/pull/182)

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.0.2](https://github.com/gregsdennis/json-everything/pull/120)

Added some logging for entering and exiting the `data` keyword.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/75)

Added support for nullable reference types.

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/72)

Initial release.