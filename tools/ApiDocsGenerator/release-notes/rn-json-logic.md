---
layout: page
title: JsonLogic
icon: fas fa-tag
order: "09.11"
---
# [5.2.1](https://github.com/gregsdennis/json-everything/pull/717) {#release-logic-5.2.1}

Fixed an exception message.

# [5.2.0.x](https://github.com/gregsdennis/json-everything/pull/712) {#release-logic-5.2.0.x}

[#711](https://github.com/gregsdennis/json-everything/issues/711) - Nuget package meta-data updates; no functional changes from previous version.

# [5.2.0](https://github.com/gregsdennis/json-everything/pull/708) {#release-logic-5.2.0}

Added new implementation that operates on `JsonNode` instead of a dedicated object model.  Cuts memory use and evaluation time in half!

# [5.1.0](https://github.com/gregsdennis/json-everything/pull/680) {#release-logic-5.1.0}

Added extension methods to rule types to allow readonly access to their internal data.

# [5.0.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-logic-5.0.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8 with Native AOT support, including updating _System.Text.Json_ from v6 to v8.

Most of the changes to support Native AOT involve either updating internal implementation or creating overloads that do.  Whatever could not be updated was marked as requiring reflection, which will generate warnings when building Native AOT applications.

## Breaking changes

- Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 
- `RuleRegistry.AddRule<T>()` marked as AOT-incompatible.  Replaced by `.AddRule<T>(JsonSerializerContext)`.
- Rules that returned `JsonNull.SignalNode` should now return null, as `JsonNull` has been removed from _Json.More.Net_.
- `JsonWriterExtensions` removed in favor of the new `JsonSerializerOptionsExtensions` methods in _Json.More.Net_.
- Custom rules MUST now implement `IWeaklyTypedJsonConverter` (or derive from `WeaklyTypedJsonConverter<T>` from _Json.More.Net_).

## Additions

`Rule.TypeInfoResolver` to expose all of the type resolvers contained in the library.  Can be used to create a combined `SerializationOptions` by using a `Json.More.TypeResolverOptionsManager` in your `JsonSerializerContext`.

# [4.0.4](https://github.com/gregsdennis/json-everything/pull/422) {#release-logic-4.0.4}

[#420](https://github.com/gregsdennis/json-everything/issues/404) - `<=` not working for the "between" case.  Thanks to [@alexkharuk](https://github.com/alexkharuk) for finding and fixing this issue.

# [4.0.3](https://github.com/gregsdennis/json-everything/pull/410) {#release-logic-4.0.3}

[#404](https://github.com/gregsdennis/json-everything/issues/404) - `missing` _doesn't_ check for truthiness, it special cases the empty string.

# [4.0.2](https://github.com/gregsdennis/json-everything/pull/409) {#release-logic-4.0.2}

[#404](https://github.com/gregsdennis/json-everything/issues/404) - `missing` checks for truthiness of the value at the location, not just that the location exists.  Thanks to [@alexkharuk](https://github.com/alexkharuk) for finding this issue.

# 4.0.1 (no PR) {#release-logic-4.0.0}

Empty strings "numberify" to 0.

# [4.0.0](https://github.com/gregsdennis/json-everything/pull/395) {#release-logic-4.0.0}

[#377](https://github.com/gregsdennis/json-everything/issues/377) - Alignment with JS version available online at https://jsonlogic.com/.

With 3.x, when a comparison or other operation didn't make sense given the types of arguments it received, a `JsonLogicException` would be thrown.  However, the online version doesn't throw errors for these cases.  Instead it returns various values because it's Javascript.  This release attempts to align with the online version.

Some of the results may not make sense from a .Net point of view, but alignment with the original implementation was the goal.

This is released as a major version update because a lot of expected exceptions were removed.  There is no change to the API.

# [3.3.2](https://github.com/gregsdennis/json-everything/pull/385) {#release-logic-3.3.2}

[#383](https://github.com/gregsdennis/json-everything/issues/383) - `all` and `some` rules should use a local scope for `var` resolution.  Thanks to [@Sykander](https://github.com/Sykander) for finding and reasoning out this issue.

# [3.3.1](https://github.com/gregsdennis/json-everything/pull/376) {#release-logic-3.3.1}

[#373](https://github.com/gregsdennis/json-everything/issues/373) - Updated `<` and `<=` rules for the three-input case ("between" rule) to properly coerce values.  Thanks to [@alexkharuk](https://github.com/alexkharuk) for finding and fixing this bug.

# [3.3.0](https://github.com/gregsdennis/json-everything/pull/370) {#release-logic-3.3.0}

[#368](https://github.com/gregsdennis/json-everything/issues/368)/[#369](https://github.com/gregsdennis/json-everything/issues/369) - Updated rule class accessibility to allow users to customize rule behavior.  Thanks to [@dcook-net](https://github.com/dcook-net) for suggesting and implementing this change.

# [3.2.0](https://github.com/gregsdennis/json-everything/pull/329) {#release-logic-3.2.0}

[#328](https://github.com/gregsdennis/json-everything/issues/328) - Added serialization support.

# [3.1.2](https://github.com/gregsdennis/json-everything/pull/320) {#release-logic-3.1.2}

[#318](https://github.com/gregsdennis/json-everything/issues/318)/[#319](https://github.com/gregsdennis/json-everything/pull/319) - Conversions to numbers shouldn't be culture-dependent.  Thanks to [@warappa](https://github.com/warappa) for reporting and fixing this.

# 3.1.1 (no PR) {#release-logic-3.1.1}

[#313](https://github.com/gregsdennis/json-everything/issues/313) - Deserialization of nulls resulted in an actual null rule instead of a variable rule with a null value.  Thanks to [@jhspinpanel](https://github.com/jhspinpanel) for reporting this.

# 3.1.0 (no PR) {#release-logic-3.1.0}

[#302](https://github.com/gregsdennis/json-everything/issues/302) - Predefined rule types should be public to enable analysis.

[#286](https://github.com/gregsdennis/json-everything/pull/286) - Remainder of operators aligned to function like the pre-existing playground.

# 3.0.1 (no PR) {#release-logic-3.0.1}

[#286](https://github.com/gregsdennis/json-everything/pull/286) - `in` operation throws exception when second arg is not array.  Original implementation returns false.

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.


# [3.0.0](https://github.com/gregsdennis/json-everything/issues/280) {#release-logic-3.0.0}

Updated all functionality to use `JsonNode` instead of `JsonElement`.

## Breaking Changes {#release-logic-3.0.0-breaks}

_`JsonElement` -> `JsonNode` type exchange changes not listed._

- Remove all `JsonLogic.Literal()` methods and replaced with a single `Liternal(JsonNode?)`.  Typed methods are unnecessary as `JsonNode` contains implicit casts from them.
- `JsonElementExtensions` converted into `JsonNodeExtensions`.  Same functionality exists, but for `JsonNode` instead.
- `Rule` and all of its subclasses now take and return `JsonNode?`

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/265) {#release-logic-2.0.0}

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

[#263](https://github.com/gregsdennis/json-everything/issues/263) - `{"var": ""}` is used by some operations to pass context data into inner rules, but the external data is also available.

## Breaking Changes {#release-logic-2.0.0-breaks}

- Added optional parameter to `Rule.Apply()` in order to supply context data.
- `"var"` will now prioritize context data over external data.  If the path yields no result for context data, it will search the external data.  This means that if both the context data and the external data have the given path, the context data will be used.

# [1.4.0](https://github.com/gregsdennis/json-everything/pull/205) {#release-logic-1.4.0}

[#183](https://github.com/gregsdennis/json-everything/pull/183) - Added handling of object truthiness: empty objects are falsy; non-empty are truthy.  This behavior isn't specified, but emulates the original JS library.

# [1.3.2](https://github.com/gregsdennis/json-everything/pull/182) {#release-logic-1.3.2}

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.3.1](https://github.com/gregsdennis/json-everything/pull/133) {#release-logic-1.3.1}

[#132](https://github.com/gregsdennis/json-everything/pull/132) - Fixed some memory management issues around `JsonDocument` and `JsonElement`.  Thanks to [@ddunkin](https://github.com/ddunkin) for finding and fixing these.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/92) {#release-logic-1.3.0}

- Exposed `JsonElementExtensions` so that it can be used in custom rules.
- Removed `new()` requirement from `RuleRegistry.AddRule<T>()`.

# [1.2.1](https://github.com/gregsdennis/json-everything/pull/75) {#release-logic-1.2.1}

Added support for nullable reference types.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/64) {#release-logic-1.2.0}

Added the ability to define and register custom rules.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/62) {#release-logic-1.1.0}

Prematurely released library.  It would parse and process fine, but the factory methods for building inline logic were incomplete.  Also added XML comments for everything.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/61) {#release-logic-1.0.1}

Signed the DLL for strong name compatibility.

# 1.0.0 {#release-logic-1.0.0}

Initial release.
