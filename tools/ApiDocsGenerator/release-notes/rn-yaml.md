---
layout: page
title: Yaml2JsonNode
icon: fas fa-tag
order: "09.14"
---
# [2.0.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-yaml-2.0.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8.

Most of the changes to support Native AOT involve either updating internal implementation or creating overloads that do.  Whatever could not be updated was marked as requiring reflection, which will generate warnings when building Native AOT applications.

## Breaking changes

Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 

# [1.2.4](https://github.com/gregsdennis/json-everything/pull/486) {#release-yaml-1.2.4}

Fixed an issue with converting nulls to `YamlNode`.

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
