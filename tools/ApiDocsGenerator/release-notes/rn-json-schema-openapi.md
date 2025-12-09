---
layout: page
title: JsonSchema.Net.OpenApi
icon: fas fa-tag
order: "09.04"
---
# [4.0.0](https://github.com/gregsdennis/json-everything/pull/952) {#release-schemaopenapi-4.0.0}

Updated per _JsonSchema.Net_ v8.

## Breaking changes

- Keywords
  - now implement `IKeywordHandler`
  - are singletons
  - moved to `.Keywords` namespace
- `Vocabularies` static class renamed to `Vocabulary`
  - `.Register()` method moved to `MetaSchemas` class
- Removed `JsonSchemaExtensions` (keyword access)
- Removed serialization support (no longer needed)

## Other changes

- Added `Dialect` static class.
- Add .Net 10.0 support.
- Update Nuget references.

# [3.2.0](https://github.com/gregsdennis/json-everything/pull/822) {#release-schemaopenapi-3.2.0}

Add .Net 9.0 support.

# [3.1.0](https://github.com/gregsdennis/json-everything/pull/719) {#release-schemaopenapi-3.1.0}

Updated to use _JsonSchema.Net_ v7.0.0, which contains breaking changes ([release notes](/rn-json-schema/#release-schema-7.0.0)).

# [3.0.1](https://github.com/gregsdennis/json-everything/pull/721) {#release-schemaopenapi-3.0.1}

[#720](https://github.com/gregsdennis/json-everything/issues/720) - `DiscriminatorKeyword` constructor throws `InvalidCastException`.

# [3.0.0.x](https://github.com/gregsdennis/json-everything/pull/712) {#release-schemaopenapi-3.0.0.x}

[#711](https://github.com/gregsdennis/json-everything/issues/711) - Nuget package meta-data updates; no functional changes from previous version.

# [3.0.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-schemaopenapi-3.0.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8 with Native AOT support, including updating _System.Text.Json_ from v6 to v8.

## Breaking changes

Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 

# [2.1.0](https://github.com/gregsdennis/json-everything/pull/565) {#release-schemaopenapi-2.1.0}

[#549](https://github.com/gregsdennis/json-everything/issues/549) - Expose JSON converters for all types to support JSON source generation.

# [2.0.1](https://github.com/gregsdennis/json-everything/pull/316) {#release-schemaopenapi-2.0.1}

Keywords that support null values need to be registered as such so that they can be handled properly.  Examples supports null values, but the registration of the keyword didn't include the null-value regisration.  This fix includes the null-value registration.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/316) {#release-schemaopenapi-2.0.0}

Updated in accordance with JsonSchema.Net v5.

# 1.1.0 ([#326](https://github.com/gregsdennis/json-everything/pull/326) / [#389](https://github.com/gregsdennis/json-everything/pull/389)) {#release-schemaopenapi-1.1.0}

Updated to use JsonSchema.Net v4.

Added extensions for each keyword to enable easier keyword value access, e.g. `schema.GetExample()`.

# 1.0.0 {#release-schemaopenapi-1.0.0}

Initial release
