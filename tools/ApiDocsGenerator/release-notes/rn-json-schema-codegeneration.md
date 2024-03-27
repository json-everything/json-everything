---
layout: page
title: JsonSchema.Net.CodeGeneration
icon: fas fa-tag
order: "09.15"
---
> **DEPRECATION NOTICE**
>
> This library is no longer being developed.
> 
> Please see [https://blog.json-everything.net/posts/dropping-codegen/](https://blog.json-everything.net/posts/dropping-codegen/) for more information.
{: .prompt-danger }

# [0.2.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-schemacodegen-0.2.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8 with Native AOT support, including updating _System.Text.Json_ from v6 to v8.

## Breaking changes

Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 

# [0.1.3](https://github.com/gregsdennis/json-everything/pull/535) {#release-schemacodegen-0.1.3}

Added evaluation results from supported-pattern recognition process.

# [0.1.2](https://github.com/gregsdennis/json-everything/pull/509) {#release-schemacodegen-0.1.2}

Added support for `additionalProperties: false` to generate sealed classes.

# [0.1.1](https://github.com/gregsdennis/json-everything/pull/509) {#release-schemacodegen-0.1.1}

Added `[JsonPropertyName]` attribute usage to support custom JSON keys during deserialization.

Add support for `readOnly` and `writeOnly` to generate getter/setter-only properties.

Improved testing to ensure code actually compiles and is usable.

# [0.1.0](https://github.com/gregsdennis/json-everything/pull/505) {#release-schemacodegen-0.1.0}

Initial release.
