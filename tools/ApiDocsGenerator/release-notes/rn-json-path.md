---
layout: page
title: JsonPath.Net
icon: fas fa-tag
order: "09.08"
---
# [1.1.1](https://github.com/gregsdennis/json-everything/pull/750) {#release-path-1.1.1}

[JSON Path Test Suite #83](https://github.com/jsonpath-standard/jsonpath-compliance-test-suite/pull/83) - Fixes some order of operations issues with parsing filter expressions with boolean operators.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/740) {#release-path-1.1.0}

[#738](https://github.com/gregsdennis/json-everything/issues/738) - Adds additional support for AOT and trimming.  Thanks to [@jevansaks](https://github.com/jevansaks) for implementing this update.

# [1.0.5](https://github.com/gregsdennis/json-everything/commit/3dac71f99ae3958482bfe9dc32e098a234a91c11) {#release-path-1.0.5}

An update to the test suite revealed an issue with the filter expression parser.

# [1.0.4](https://github.com/gregsdennis/json-everything/pull/731) {#release-path-1.0.4}

[#728](https://github.com/gregsdennis/json-everything/issues/728) - Failure to parse escaped single quote in single-quoted string in filter expressions.  Thanks to [@ashek-simeon](https://github.com/ashek-simeon) for reporting this.

# [1.0.3](https://github.com/gregsdennis/json-everything/pull/730) {#release-path-1.0.3}

[Test Suite PR #77](https://github.com/jsonpath-standard/jsonpath-compliance-test-suite/pull/77) added some tests to check for a dot `.` inside square brackets, where it should be considered a literal and not transformed.

# [1.0.2](https://github.com/gregsdennis/json-everything/pull/725) {#release-path-1.0.2}

Fixes a rare issue with `match()` and `search()` functions.

# [1.0.1.x](https://github.com/gregsdennis/json-everything/pull/712) {#release-path-1.0.1.x}

[#711](https://github.com/gregsdennis/json-everything/issues/711) - Nuget package meta-data updates; no functional changes from previous version.

# [1.0.1](https://github.com/gregsdennis/json-everything/commit/550f4d97d52c90644cf033ae235fa6d80b3fbda3) {#release-path-1.0.1}

Fixed an issue around operator precedence in boolean expressions.

# [1.0.0](https://github.com/gregsdennis/json-everything/commit/550f4d97d52c90644cf033ae235fa6d80b3fbda3) {#release-path-1.0.0}

JSON Path RFC 9535 has been released!  (No functional changes from 0.8.0.)

# [0.8.1](https://github.com/gregsdennis/json-everything/pull/655) {#release-path-0.8.0}

[#653](https://github.com/gregsdennis/json-everything/pull/653) - [@Emcrank](https://github.com/Emcrank) applied modifications to support .Net Standard 2.0.

# [0.8.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-path-0.8.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8 with Native AOT support, including updating _System.Text.Json_ from v6 to v8.

## Breaking changes

- Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 
- `JsonNodeExtensions.TryGetSingleValue(this JsonNode?)` and `.TryGetSingleValue(this NodeList?)` updated to the `bool Try...(out ...)` pattern.

## Additions

`JsonPath.TypeInfoResolver` to expose all of the type resolvers contained in the library.  Can be used to create a combined `SerializationOptions` by using a `Json.More.TypeResolverOptionsManager` in your `JsonSerializerContext`.

# [0.7.0](https://github.com/gregsdennis/json-everything/pull/591) {#release-path-0.7.0}

[#583](https://github.com/gregsdennis/json-everything/issues/583) - Updated `NodeList` to implement `IEnumerable<T>` instead of `IReadOnlyList<T>`, which enables follow-on Linq queries, making the entire JSON Path evaluation a deferred execution query.  Thanks to [@qt-kaneko](https://github.com/qt-kaneko) for explaining a use case that led to this change.

Technically this is a breaking change.  The `.Count` property and indexer defined by `IReadOnlyList<T>` were left implemented on `NodeList`, so this _shouldn't_ impact consuming code, but there is a possibility.

It's also important to note that `.Count` is now backed by Linq's `.Count()` method, and the indexer is backed by Linq's `.ElementAt()` method, so calling these isn't zero-cost, like they once were.  They will now invoke an evaluation of the path.

# [0.6.7](https://github.com/gregsdennis/json-everything/pull/526) {#release-path-0.6.7}

Whitespace between multiple selectors inside brackets was supported for `.Parse()` but not `.TryParse()`.  Thanks to [@Andrej-Peplinski](https://github.com/Andrej-Peplinski) for reporting this.

# [0.6.6](https://github.com/gregsdennis/json-everything/pull/524) {#release-path-0.6.6}

Adds support for `+` in number literals, e.g. `+24` and `1e+5`.

# [0.6.5](https://github.com/gregsdennis/json-everything/pull/515) {#release-path-0.6.5}

Update `.AsJsonPointer()` to allow `$[1,"1"]` pattern discussed in https://blog.json-everything.net/posts/paths-and-pointers-correction/.

# [0.6.4](https://github.com/gregsdennis/json-everything/pull/496) {#release-path-0.6.4}

[#495](https://github.com/gregsdennis/json-everything/issues/495) - `.TryParse()` with empty string throws exception.  Thanks to [@ben-wckdrzr](https://github.com/ben-wckdrzr) for reporting this.

# [0.6.3](https://github.com/gregsdennis/json-everything/commit/2b6e2599e5513069cc1dbb3523d267d241966111) {#release-path-0.6.3}

[Test Suite #41](https://github.com/jsonpath-standard/jsonpath-compliance-test-suite/pull/41) - ANBF allows for whitespace between segments.

# [0.6.2](https://github.com/gregsdennis/json-everything/pull/467) {#release-path-0.6.2}

[#465](https://github.com/gregsdennis/json-everything/issues/465) - Exposes more of the JSON Path object model, while making the exposed API more consistent.

# [0.6.1](https://github.com/gregsdennis/json-everything/pull/464) {#release-path-0.6.1}

[#463](https://github.com/gregsdennis/json-everything/issues/463) - Fixed implementation for string comparisons in filter expressions.  Thanks to [@idefy](https://github.com/idefy) for reporting this.

# [0.6.0](https://github.com/gregsdennis/json-everything/pull/459) {#release-path-0.6.0}

Updated support for whitespace in all the right places.

Changed `PathParsingOptions.TolerateSurroundingWhitespace` to `TolerateExtraWhitespace` which now enables support for whitespace pretty much everywhere.

# [0.5.2](https://github.com/gregsdennis/json-everything/pull/431) {#release-path-0.5.2}

Improved support for regular expressions matching unicode text.

# [0.5.1](https://github.com/gregsdennis/json-everything/pull/412) {#release-path-0.5.1}

Added optional support for:

- allowing leading and trailing whitespace
- `in` operator in expressions
- JSON constructs as literals in expressions
- relative paths (starting a path with `@`)

# [0.5.0](https://github.com/gregsdennis/json-everything/pull/407) {#release-path-0.5.0}

Updated to meet the requirements in [draft 11](https://www.ietf.org/archive/id/draft-ietf-jsonpath-base-11.html).

- Update function definition
  - only name is required by the interface
  - functions must now inherit from `ValueFunctionDefinition`, `LogicalFunctionDefinition`, or `NodeListFunctionDefinition`
  - `Evaluate()` is now detected via reflection at registration time
    - return type must conform to the base class
    - any count of parameters are supported, but they must be of type `JsonNode`, `bool`, or `NodeList`
    - only one evaluation method per function class
- Added `value()` function
- Moved parsing of math operators behind `PathParsingOptions.AllowMathOperations`, which defaults to `false`
- `FunctionType` now matches the type system in the spec
- `ParameterType` removed and usages replaced with `FunctionType`
- `NodeList` implicit casts removed as they are no longer defined by the spec
- Fixed a parsing issue that allowed non-singular paths in filter expressions

# [0.4.0](https://github.com/gregsdennis/json-everything/pull/372) {#release-path-0.4.0}

- Updated function definition
  - list parameter sets to support type checking and overloads rather than a minimum and maximum parameter count
  - add return type to support type checking within expressions
- fixes around `TryParse()` to ensure that it doesn't throw exceptions (at least not from this code)
- more fixes based on [expanded tests](https://github.com/jsonpath-standard/jsonpath-compliance-test-suite/pull/12) in the compliance test suite

# [0.3.3](https://github.com/gregsdennis/json-everything/pull/367) {#release-path-0.3.3}

I added a bunch of tests to the JSON Path test suite and found a few bugs in filter expressions. This release fixes those bugs.

- Some copy/paste/forgot-to-edit issues with `<`/`<=`/`>`/`>=`
- Not extracting string values correctly in comparisons
- Not treating `null` values correctly when evaluating local paths (e.g. `@.a`)

# [0.3.2](https://github.com/gregsdennis/json-everything/pull/366) {#release-path-0.3.2}

[#364](https://github.com/gregsdennis/json-everything/issues/364) - `count()`, `match()`, and `search()` functions are not registered and so cannot be parsed.  Thanks to [@amis92](https://github.com/amis92) for finding and reporting this.

# [0.3.1](https://github.com/gregsdennis/json-everything/commit/3248441c00dd97cf4a8c66fff6b2462682a3bb8e) {#release-path-0.3.1}

Fixed an issue in `search` and `match` function evaluation.

# [0.3.0](https://github.com/gregsdennis/json-everything/pull/362) {#release-path-0.3.0}

[#281](https://github.com/gregsdennis/json-everything/issues/281) - Complete rewrite to support `JsonNode` and align with specification.
[#351](https://github.com/gregsdennis/json-everything/issues/351) - Fixes issues parsing `$.['foo']` (basically removes support since the `.[]` syntax is invalid).

Changes of note:

- Expression functions are supported and custom functions can be created.
- `.length` has been replaced by `length()` function.
- Available functions:
  - `length()` - Length of values (objects, arrays, & strings)
  - `count()` - Length of nodelists
  - `match()` - Precise regex matching (uses implicit anchoring)
  - `search()` - General regex matching (does not use implicit anchoring, more like JSON Schema's `pattern` keyword)
- Expression engine is more robust.
- No context object needed for evaluation.
- Removed experimental feature of `$ref` resolution since it's not in the spec.  (See the JSON Referencing proposals from the JSON Schema group at https://github.com/json-schema-org/referencing)

# [0.2.1](https://github.com/gregsdennis/json-everything/commit/1ba018b86c14dc94c55206062250735356491460) {#release-path-0.2.1}

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

Fixed issue with parsing recursive descents into quoted-index property names, e.g. `$..['foo']`.  Previous versions added two recursive descent selectors, which resulted in duplicate entries in the results.

# [0.2.0](https://github.com/gregsdennis/json-everything/pull/242) {#release-path-0.2.0}

Result locations should be expressed in Normalized Paths, not JSON Pointers.

# [0.1.14](https://github.com/gregsdennis/json-everything/pull/182) {#release-path-0.1.14}

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [0.1.13](https://github.com/gregsdennis/json-everything/pull/161) {#release-path-0.1.13}

Added experimential support for referencing data in secondary documents.

# [0.1.12](https://github.com/gregsdennis/json-everything/pull/161) {#release-path-0.1.12}

Added support for unary-not operator, e.g.

# [0.1.11](https://github.com/gregsdennis/json-everything/pull/159) {#release-path-0.1.11}

- Parenthetical support for nested expressions.
- Better stringifying.

# [0.1.10](https://github.com/gregsdennis/json-everything/pull/158) {#release-path-0.1.10}

Added support for comparisons against all JSON literals in expressions.

# [0.1.9](https://github.com/gregsdennis/json-everything/pull/155) {#release-path-0.1.9}

Better error reporting during parsing.

# [0.1.8](https://github.com/gregsdennis/json-everything/pull/150) {#release-path-0.1.8}

[#151](https://github.com/gregsdennis/json-everything/pull/151) - Parsing expressions with numeric values that contain `9` fails.

# [0.1.7](https://github.com/gregsdennis/json-everything/pull/133) {#release-path-0.1.7}

[#132](https://github.com/gregsdennis/json-everything/pull/132) - Fixed some memory management issues around `JsonDocument` and `JsonElement`.  Thanks to [@ddunkin](https://github.com/ddunkin) for finding and fixing these.

# [0.1.6](https://github.com/gregsdennis/json-everything/pull/122) {#release-path-0.1.6}

Updated expression parsing to handle whitespace better.

# [0.1.5](https://github.com/gregsdennis/json-everything/pull/75) {#release-path-0.1.5}

Added support for nullable reference types.

# [0.1.4](https://github.com/gregsdennis/json-everything/pull/61) {#release-path-0.1.4}

Signed the DLL for strong name compatibility.

# 0.1.3 (no PR) {#release-path-0.1.3}

Fixed issue with parsing property path segments that end in `z`, `Z`, and `9` (e.g. `$.baz`).

# [0.1.2](https://github.com/gregsdennis/json-everything/pull/45) {#release-path-0.1.2}

Added debug symbols to package.  No functional change.

# [0.1.1](https://github.com/gregsdennis/json-everything/pull/13) {#release-path-0.1.1}

Better single-quote support.

# 0.1.0 {#release-path-0.1.0}

Initial release.
