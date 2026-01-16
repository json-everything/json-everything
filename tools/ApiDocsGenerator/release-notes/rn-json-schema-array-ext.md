---
layout: page
title: JsonSchema.Net.ArrayExt
icon: fas fa-tag
order: "09.03"
---
# [4.0.0](https://github.com/gregsdennis/json-everything/pull/982) {#release-schemaarrayext-4.0.0}

Enacted Open Source Maintenance Fee.

See the [blog post](https://blog.json-everything.net/posts/expensive/) for more information.

# [3.0.1](https://github.com/gregsdennis/json-everything/pull/959) {#release-schemaarrayext-3.0.1}

Performance improvements for `uniqueKeys`.

# [3.0.0](https://github.com/gregsdennis/json-everything/pull/822) {#release-schemaarrayext-3.0.0}

Updated per _JsonSchema.Net_ v8.

## Breaking changes

- Keywords
  - now implement `IKeywordHandler`
  - are singletons
  - moved to `.Keywords` namespace
- `Vocabularies` static class renamed to `Vocabulary`
  - `.Register()` method moved to `MetaSchemas` class
- `OrderingSpecifier` now a struct, moved to `.Keywords` namespace
- `Direction` moved to `.Keywords` namespace
- Removed `JsonSchemaExtensions` (keyword access)
- Removed serialization support (no longer needed)

## Other changes

- Added `Dialect` static class.
- Add .Net 10.0 support.
- Update Nuget references.

# [2.2.0](https://github.com/gregsdennis/json-everything/pull/822) {#release-schemaarrayext-2.2.0}

Add .Net 9.0 support.

# [2.1.0](https://github.com/gregsdennis/json-everything/pull/719) {#release-schemaarrayext-2.1.0}

Updated to use _JsonSchema.Net_ v7.0.0, which contains breaking changes ([release notes](/rn-json-schema/#release-schema-7.0.0)).

# [2.0.0.x](https://github.com/gregsdennis/json-everything/pull/712) {#release-schemaarrayext-2.0.0.x}

[#711](https://github.com/gregsdennis/json-everything/issues/711) - Nuget package meta-data updates; no functional changes from previous version.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-schemaarrayext-2.0.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8 with Native AOT support, including updating _System.Text.Json_ from v6 to v8.

## Breaking changes

Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/120) {#release-schemaarrayext-1.0.0}

Initial release.

This release deprecates and builds on all functionality from _JsonSchema.Net.UniqueKeys v3.1.0_.