---
layout: page
title: JsonE.Net
icon: fas fa-tag
order: "09.12"
---
# [2.0.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-e-2.0.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8 with Native AOT support, including updating _System.Text.Json_ from v6 to v8.

## Breaking changes

Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 

## Additions

`JsonNodeExtensions.Clone()` to handle nodes that contain functions, which are not handled properly by `JsonNode.DeepCopy()`.

# [1.0.2](https://github.com/gregsdennis/json-everything/pull/621) {#release-e-1.0.2}

Updated string indexing to look at text elements instead of chars to better support surrogate pairs.

`split()` function now properly returns source string when split char is not provided.

`$let` operator now errors if `in` is not provided.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/569) {#release-e-1.0.1}

Improved error messaging so that it's closer in alignment with other implementations.

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/566) {#release-e-1.0.0}

Initial release.
