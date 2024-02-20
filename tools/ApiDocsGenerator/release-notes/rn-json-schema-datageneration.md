---
layout: page
title: JsonSchema.Net.DataGeneration
icon: fas fa-tag
order: "09.06"
---
# [2.1.0](https://github.com/gregsdennis/json-everything/pull/665) {#release-schemadatagen-2.0.1}

[#617](https://github.com/gregsdennis/json-everything/issues/617) - Added support for `exclusiveMin`, `exclusiveMax`, and `$ref`.  (Note: `$ref` support does not include infinite recursion checking.)

# [2.0.1](https://github.com/gregsdennis/json-everything/pull/658) {#release-schemadatagen-2.0.1}

[#646](https://github.com/gregsdennis/json-everything/issues/646) & [#647](https://github.com/gregsdennis/json-everything/issues/647) - Improved handling of `not`, `oneOf`, and `unevaluatedProperties`.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-schemadatagen-2.0.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8 with Native AOT support, including updating _System.Text.Json_ from v6 to v8.

## Breaking changes

Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/582) {#release-schemadatagen-1.2.0}

[#580](https://github.com/gregsdennis/json-everything/issues/580) - Add support for `pattern` by incorporating [FARE library](https://github.com/moodmosaic/Fare).

Only simple subschemas with a single, simple `pattern` is supported.

- Combining multiple regular expressions using `allOf`, `anyOf`, or `oneOf` is not supported.
- Inverting regular expressions using `not` is not supported.
- Any regular expression not supported by the FARE library is not supported.

# 1.1.0 (no PR) {#release-schemadatagen-1.1.0}

Updated JsonSchema.Net reference to v4.0.0.

# 1.0.4 (no PR) {#release-schemadatagen-1.0.4}

[#405](https://github.com/gregsdennis/json-everything/issues/405) - v1.0.3 referenced beta version of JsonSchema.Net.  This update reverts that dependency to JsonSchema v3.3.2.

# [1.0.3](https://github.com/gregsdennis/json-everything/pull/386) {#release-schemadatagen-1.0.3}

[#384](https://github.com/gregsdennis/json-everything/issues/384) - Schemas with embedded `enum`s were re-using nodes instead of copying them, causing an `InvalidOperationException`.  Thanks to [@leonardo-gooten](https://github.com/leonardo-gooten) for reporting this.

# [1.0.2](https://github.com/gregsdennis/json-everything/pull/380) {#release-schemadatagen-1.0.2}

[#374](https://github.com/gregsdennis/json-everything/issues/374) - Generated property counts were not bounded properly, causing an exception to be thrown from the randomizer.

# 1.0.1 (no PR) {#release-schemadatagen-1.0.1}

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/280) {#release-schemadatagen-1.0.0}

Updated all functionality to use `JsonNode` instead of `JsonElement` in line with JsonSchema.Net v3.

Also... OFFICIAL RELEASE!!!  I feel confident enough in this library to release it under a real version!  Thought, the JSON Schema team still advises that this sort of functionality should be used as a development tool, not in production scenarios.

# [0.1.3](https://github.com/gregsdennis/json-everything/pull/262) {#release-schemadatagen-0.1.3}

Improved handling of conditionals and inferred types.

# [0.1.2](https://github.com/gregsdennis/json-everything/pull/259) {#release-schemadatagen-0.1.2}

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

Better support for const, specifically when it appears in a conditional (`if`/`then`/`else`) or under a `not`.

# 0.1.1 (No PR) {#release-schemadatagen-0.1.1}

Fixed a bug around property generation.

# [0.1.0](https://github.com/gregsdennis/json-everything/pull/218) {#release-schemadatagen-0.1.0}

Initial release.

Not supported:

- anything involving RegEx
- reference keywords (e.g. `$ref`, `$dynamicRef`, etc)
- annotation / metadata keywords (e.g. `title`, `description`)
- `content*` keywords
- `dependencies` / `dependent*` keywords

Instance generation isn't 100%, but works most of the time.
