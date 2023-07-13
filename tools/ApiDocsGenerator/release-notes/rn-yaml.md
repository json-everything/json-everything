---
layout: page
title: Yaml2JsonNode
icon: fas fa-tag
order: "8.12"
---
# [1.2.3](https://github.com/gregsdennis/json-everything/pull/486) {#release-yaml-1.2.3}

[#485](https://github.com/gregsdennis/json-everything/issues/485) - YamlDotNet doesn't report nulls properly (see also their [#716](https://github.com/aaubry/YamlDotNet/issues/716) and [#810](https://github.com/aaubry/YamlDotNet/issues/810)).  Thanks to [@nickcampau](https://github.com/nickcampau) for reporting.

***NOTE** Nullability declaration of `.ToJsonNode()` has changed as this method can now return nulls.*

# [1.2.2](https://github.com/gregsdennis/json-everything/pull/479) {#release-yaml-1.2.2}

[#478](https://github.com/gregsdennis/json-everything/issues/478) - Converting numbers from YAML to JSON parses numbers against the machine's current culture, not the invariant culture.

# [1.2.1](https://github.com/gregsdennis/json-everything/pull/477) {#release-yaml-1.2.1}

[#476](https://github.com/gregsdennis/json-everything/issues/476) - [@amis92](https://github.com/amis92) discovered that JSON -> YAML -> JSON doesn't work when the data has string-encoded numbers and proposed the fix.

This change sets _all_ strings to have double-quotes unless the string itself has a double-quote, in which case it'll set for single-quotes.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/475) {#release-yaml-1.2.0}

Add `YamlSerializer` static class to provide more natural serializer methods.

# 1.1.1 (No PR) {#release-yaml-1.1.1}

Fixes issue with converting strings from JSON to YAML.  Previously, quotes were included.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/387) {#release-yaml-1.1.0}

[#381](https://github.com/gregsdennis/json-everything/issues/381) - Adds conversions from `JsonNode` back to YAML.

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/358) {#release-yaml-1.0.0}

Initial release.
