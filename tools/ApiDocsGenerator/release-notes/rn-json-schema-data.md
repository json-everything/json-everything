---
layout: page
title: JsonSchema.Net.Data
icon: fas fa-tag
order: "09.05"
---
# [5.0.1](https://github.com/gregsdennis/json-everything/pull/565) {#release-pointer-5.0.1}

Corrected meta-schema URIs.

Was:

- Vocab meta-schema: `https://json-everything.net/schema/meta/vocab/data-2023`
- 2020-12 meta-schema: `https://json-everything.net/schema/meta/data-2023`

Is:

- Vocab meta-schema: `https://json-everything.net/meta/vocab/data-2023`
- 2020-12 meta-schema: `https://json-everything.net/meta/data-2023`

# [5.0.0](https://github.com/gregsdennis/json-everything/pull/565) {#release-pointer-5.0.0}

Updated to support new Data 2023 vocabulary.

## Breaking changes {#release-pointer-5.0.0-breaks}

- Library has been updated to .Net Standard 2.1 in order to reference _JsonPath.Net_.
- Metaschema has been split into one for the vocabulary itself (`.Data`) and one for the 2020-12 extension (`.Data_202012`).
- All URIs updated from `*-2022` to `*-2023`.

## Migration {#release-pointer-5.0.0-migration}

References to `MetaSchemas.Data` will need to be changed to `MetaSchemas.Data_202012`.

## Additions

`data` now supports JSON Path references.  See [vocabulary spec](https://docs.json-everything.net/schema/vocabs/data-2023/) for an example.

# [4.1.0](https://github.com/gregsdennis/json-everything/pull/565) {#release-pointer-4.1.0}

[#549](https://github.com/gregsdennis/json-everything/issues/549) - Expose JSON converters for all types to support JSON source generation.

# [4.0.1](https://github.com/gregsdennis/json-everything/pull/521) {#release-schemadata-4.0.1}

Fixes an internal reflection issue caused by updating _JsonSchema.Net_ to v5.2.3.

# [4.0.0](https://github.com/gregsdennis/json-everything/pull/316) {#release-schemadata-4.0.0}

Updated in accordance with JsonSchema.Net v5.

# 3.1.0 ([#326](https://github.com/gregsdennis/json-everything/pull/326) / [#389](https://github.com/gregsdennis/json-everything/pull/389) / [#392](https://github.com/gregsdennis/json-everything/pull/392)) {#release-schemadata-3.1.0}

Updated to use JsonSchema.Net v4.

Fixed deserialization bug.

`DataKeyword.ExternalDataRegistry` is actually used.  Previously, checking this registry was skipped (an oversight) before moving on to fetching the data.  Also fixed the type of the property to store `JsonNode` instead of `JsonValue` to support the full range of JSON.

Added `JsonSchemaExtensions.GetData()` to enable easy access to `data` keyword.

# [3.0.1](https://github.com/gregsdennis/json-everything/pull/316) {#release-schemadata-3.0.1}

Fixed bug where URI-encoded JSON Pointers where being handled as instance locations instead of schema locations.  (Thanks new [test suite](https://github.com/gregsdennis/json-schema-vocab-test-suites)!)

# [3.0.0](https://github.com/gregsdennis/json-everything/pull/316) {#release-schemadata-3.0.0}

Supports the 2022 version of the vocabulary.  This is a breaking change from the previous version.

## Breaking changes {#release-schemadata-3.0.0-breaks}

References no longer just support URIs.  Instead several formats are used to indicate the source document:

- JSON Pointers (not URI fragments) and Relative JSON Pointers indicate locations within the instance
- Fragment-only URI-encoded JSON Pointers indicate locations within the host schema (just like a `$ref` would)
- Absolute URIs (optionally with URI-encoded JSON Pointer fragments) are used to access external data

The first point is the primary change.

## Migration {#release-schemadata-3.0.0-migration}

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

# 2.0.1 (no PR) {#release-schemadata-2.0.1}

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/280) {#release-schemadata-2.0.0}

Updated all functionality to use `JsonNode` instead of `JsonElement` in line with JsonSchema.Net v3.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/249) {#release-schemadata-1.1.0}

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

[#190](https://github.com/gregsdennis/json-everything/issues/190) - Added support for custom and localized error messages.

# [1.0.5](https://github.com/gregsdennis/json-everything/pull/200) {#release-schemadata-1.0.5}

Updated JsonSchema.Net reference to v2.0.0.  Please see [release notes](./json-schema.md) for that library as it contains breaking changes.

# [1.0.4](https://github.com/gregsdennis/json-everything/pull/???) {#release-schemadata-1.0.4}

???

# [1.0.3](https://github.com/gregsdennis/json-everything/pull/182) {#release-schemadata-1.0.3}

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.0.2](https://github.com/gregsdennis/json-everything/pull/120) {#release-schemadata-1.0.2}

Added some logging for entering and exiting the `data` keyword.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/75) {#release-schemadata-1.0.1}

Added support for nullable reference types.

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/72) {#release-schemadata-1.0.0}

Initial release.
