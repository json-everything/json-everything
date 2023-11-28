---
layout: page
title: JsonSchema.Net.OpenApi
icon: fas fa-tag
order: "08.07"
---
# [2.1.0](https://github.com/gregsdennis/json-everything/pull/565) {#release-pointer-2.1.0}

[#549](https://github.com/gregsdennis/json-everything/issues/549) - Expose JSON converters for all types to support JSON source generation.

# [2.0.1](https://github.com/gregsdennis/json-everything/pull/316) {#release-schemadata-2.0.1}

Keywords that support null values need to be registered as such so that they can be handled properly.  Examples supports null values, but the registration of the keyword didn't include the null-value regisration.  This fix includes the null-value registration.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/316) {#release-schemadata-2.0.0}

Updated in accordance with JsonSchema.Net v5.

# 1.1.0 ([#326](https://github.com/gregsdennis/json-everything/pull/326) / [#389](https://github.com/gregsdennis/json-everything/pull/389)) {#release-schemaopenapi-1.1.0}

Updated to use JsonSchema.Net v4.

Added extensions for each keyword to enable easier keyword value access, e.g. `schema.GetExample()`.

# 1.0.0 {#release-schemaopenapi-1.0.0}

Initial release
