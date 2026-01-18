---
layout: page
title: Yaml2JsonNode
icon: fas fa-tag
order: "09.13"
---
# [3.0.0](https://github.com/gregsdennis/json-everything/pull/982) {#release-yaml-3.0.0}

Enacted Open Source Maintenance Fee.

See the [blog post](https://blog.json-everything.net/posts/expensive/) for more information.

# [2.3.0](https://github.com/gregsdennis/json-everything/pull/952) {#release-yaml-2.3.0}

- Add `JsonElement` support extensions.

# [2.3.0](https://github.com/gregsdennis/json-everything/pull/952) {#release-yaml-2.3.0}

- Add .Net 10.0 support.
- Update Nuget references.

# [2.2.0](https://github.com/gregsdennis/json-everything/pull/822) {#release-yaml-2.2.0}

- Add .Net 9.0 support.
- Update YamlDotNet to v16.

# [2.1.1](https://github.com/gregsdennis/json-everything/commit/782198a80220fc2e54dfe3e01c2e4da54ab4cad7) {#release-yaml-2.1.1}

Updating _System.Text.Json_ to 8.0.4 due to a vulnerability.

# [2.1.0](https://github.com/gregsdennis/json-everything/commit/7c20af32520ade11cf09edd3b90fbdace9905b84) {#release-yaml-2.1.0}

Updated to latest YamlDotNet (v12.2.1 to v15.1.2).

# [2.0.0.x](https://github.com/gregsdennis/json-everything/pull/712) {#release-yaml-2.0.0.x}

[#711](https://github.com/gregsdennis/json-everything/issues/711) - Nuget package meta-data updates; no functional changes from previous version.

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
