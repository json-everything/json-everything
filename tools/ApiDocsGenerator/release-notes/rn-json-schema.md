---
layout: page
title: JsonSchema.Net
icon: fas fa-tag
order: "09.01"
---
# [7.2.3](https://github.com/gregsdennis/json-everything/pull/792) {#release-schema-7.2.3}

[#791](https://github.com/gregsdennis/json-everything/issues/791) - Fixes an issue with `ValidatingConverter` where options weren't being honored for explicitly mapped types.  Thanks to [@pian0](https://github.com/pian0) for reporting and creating replicating tests.

# [7.2.2](https://github.com/gregsdennis/json-everything/pull/782) {#release-schema-7.2.2}

Fixes an issue with `unevaluated*` keyword output where the instance location would contain repeated pointer segments.

# [7.2.1](https://github.com/gregsdennis/json-everything/pull/781) {#release-schema-7.2.1}

[#778](https://github.com/gregsdennis/json-everything/issues/778) - Addresses a potential `IndexOutOfRangeException` that could occur for large schemas.  Thanks to [@Laniusexcubitor](https://github.com/Laniusexcubitor) for bouncing ideas and testing.

# [7.2.0](https://github.com/gregsdennis/json-everything/pull/776) {#release-schema-7.2.0}

[#773](https://github.com/gregsdennis/json-everything/issues/773) - [@rikbosch](https://github.com/rikbosch) recommended and patched a performance improvement around regular expression handling.

Adds new methods which access patterns with strings instead of `Regex` instances.  This allows the `Regex` class to handle caching, which can reduce memory usage in various cases including using the same patterns across multiple schemas.

> `PatternKeyword.InvalidPattern` and `PatternProperties.InvalidPatterns` are no longer used.  Deserialization of invalid patterns will now throw `RegexParseException`, which aligns better with the `JsonSchemaBuilder` experience (invalid patterns passed to `.Pattern()` or `.PatternProperties` will not compile).
{: .prompt-warning }

# [7.1.2](https://github.com/gregsdennis/json-everything/pull/759) {#release-schema-7.1.2}

Update to use _JsonPointer.Net_ v5.0.2.

# [7.1.1](https://github.com/gregsdennis/json-everything/pull/755) {#release-schema-7.1.1}

Added a backdoor to allow Graeae to support draft 4 schemas.

# [7.1.0](https://github.com/gregsdennis/json-everything/pull/752) {#release-schema-7.1.0}

Adds `EvaluationOptions.AllowReferencesIntoUnknownKeywords` to optionally disallow `$ref` navigation into unknown keywords.

# [7.0.4](https://github.com/gregsdennis/json-everything/pull/746) {#release-schema-7.0.4}

`minContains` and `maxContains` annotations were still being discovered by `contains` when validating as Draft 6/7 even though `minContains` and `maxContains` are Draft 2019-90+ keywords. 

# [7.0.3](https://github.com/gregsdennis/json-everything/commit/f63f73cb0b4d1fc3158e01cac1df5b8acb5e092c) {#release-schema-7.0.3}

Issue identified via [Bowtie](https://bowtie.report) where `SchemaRegistry.Fetch()` would just return the document, ignoring any anchors.

# [7.0.2](https://github.com/gregsdennis/json-everything/pull/730) {#release-schema-7.0.2}

[#726](https://github.com/gregsdennis/json-everything/issues/726) - [@filzrev](https://github.com/filzrev) discovered a thread-safety issue.

# [7.0.1](https://github.com/gregsdennis/json-everything/pull/724) {#release-schema-7.0.1}

Fixes an issue where URI lookups consider the fragment.  This can cause issues if the user has supplied their own `SchemaRegistry.Fetch` method and performs a lookup by merely `.ToString()`-ing the URI and performing string equality.

# [7.0.0](https://github.com/gregsdennis/json-everything/pull/719) {#release-schema-7.0.0}

> Versions 7.0.0.x are just repackaging of 7.0.0.  There were some issues getting the dependencies right.  I've unlisted all but 7.0.0.3.  Thanks to [@na1307](https://github.com/na1307) for reporting the issue and helping verify the fix.
{: .prompt-info }

Updated to use _JsonPointer.Net_ v5.0.0, which contains breaking changes ([release notes](/rn-json-pointer/#release-pointer-5.0.0)).

Also includes numerous refactors for better evaluation times and memory management.

Benchmark of the JSON Schema Test Suite before and after changes:

| Version | n  | Mean       | Error    | StdDev   | Gen0        | Gen1       | Allocated |
|-------- |--- |-----------:|---------:|---------:|------------:|-----------:|----------:|
| 6.1.2   | 1  |   412.7 ms | 14.16 ms | 41.30 ms |  27000.0000 |  1000.0000 |  82.66 MB |
| 7.0.0   | 1  |   296.5 ms |  5.82 ms | 10.03 ms |  21000.0000 |  4000.0000 |  72.81 MB |
| 6.1.2   | 10 | 1,074.7 ms | 22.24 ms | 63.82 ms | 218000.0000 | 11000.0000 | 476.56 MB |
| 7.0.0   | 10 |   903.0 ms | 17.96 ms | 40.91 ms | 202000.0000 |  9000.0000 | 443.65 MB |

## Breaking changes

- `EvaluationOptions`
  - `EvaluateAs` is now init-only
  - `VocabularyRegistry` removed
- `VocabularyRegistry` is now a static class
- `SchemaRegistry`
  - parameterless constructor removed (was marked obsolete)
  - `Get(uri)` will either return an `IBaseDocument` instance or throw `RefResolutionException` (previously would return null)
- `IJsonSchemaKeyword.GetConstraint(SchemaConstraint, IReadOnlyList<KeywordConstraint>, EvaluationContext)` updated to `GetConstraint(SchemaConstraint, ReadOnlySpan<KeywordConstraint>, EvaluationContext)`
- `EvaluationExtensions.GetKeywordConstraint<T>(IEnumerable<KeywordConstraint>)` updated to `GetKeywordConstraint<T>(ReadOnlySpan<KeywordConstraint>)`
- `JsonSchemaExtensions` methods with return type of `IReadOnlyCollection<T>` updated to `IReadOnlyList<T>` to match keyword output
- `JsonPointers` static class renamed to `CommonJsonPointers`
- `KeywordExtensions` static class consolidated into `SchemaKeywordRegistry`

## Additions

- `CommonJsonPointers.GetNumberSegment(int)` for easy re-use of numeric pointer segments
- (.Net Standard 2.0) `NetStandardExtensions.GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>, TKey)` to mirror same method available in .Net 8
- `RefResolutionException` (derives from `JsonSchemaException`) thrown when a reference cannot be resolved
- `VocabularyRegistry.Unregister(Vocabulary)` to remove a registration

## Other changes

There was a lot of refactoring internally.  Most impactfully, the schema registry now performs a lot more analysis on registration, like identifying anchors, dialects, and spec version as well as propagating base URIs.  Previously this was done on initial evaluation.

# 6.1.2 (No PR) {#release-schema-6.1.2}

Revert changes from [v6.1.0](#release-schema-6.1.0) as they broke functionality in satellite packages.  Will reintroduce with next major version.  Until then, the returns from these methods can be safely cast to `IReadOnlyList<T>`.

# [6.1.1](https://github.com/gregsdennis/json-everything/pull/717) {#release-schema-6.1.1}

Fixed an exception message.

# [6.1.0.x](https://github.com/gregsdennis/json-everything/pull/712) {#release-schema-6.1.0.x}

[#711](https://github.com/gregsdennis/json-everything/issues/711) - Nuget package meta-data updates; no functional changes from previous version.

# [6.1.0](https://github.com/gregsdennis/json-everything/pull/709) {#release-schema-6.1.0}

[#704](https://github.com/gregsdennis/json-everything/issues/704) - Several builder extension return types expanded from `IReadOnlyCollection<T>` to `IReadOnlyList<T>`.  Thanks to [@SGStino](https://github.com/SGStino) for identifying and implementing this.

# [6.0.7](https://github.com/gregsdennis/json-everything/pull/698) {#release-schema-6.0.7}

[#697](https://github.com/gregsdennis/json-everything/issues/697) - `allOf`, `anyOf`, `oneOf`, and `prefixItems` all require at least one subschema, but deserialization of these keywords didn't enforce this.  Fix will throw `ArgumentException` when attempting to deserialize empty subschema sets.  Thanks to [@Era-cell](https://github.com/Era-cell) for reporting this.

# [6.0.6](https://github.com/gregsdennis/json-everything/pull/695) {#release-schema-6.0.6}

Marked `Pre202012EvaluationResultsJsonConverter` as obsolete.  The 2019-09/2020-12 output structure isn't very well specified, and making the current `EvaluationResults` architecture serialize to these formats isn't worth the effort.

# [6.0.5](https://github.com/gregsdennis/json-everything/pull/691) {#release-schema-6.0.5}

[#690](https://github.com/gregsdennis/json-everything/issues/690) - Fixes an issue with `$anchor` validation in 2020-12.  Thanks to [@Era-cell](https://github.com/Era-cell) for reporting this.

Fixed an issue with legacy output serialization.

# [6.0.4](https://github.com/gregsdennis/json-everything/pull/678) {#release-schema-6.0.4}

[#677](https://github.com/gregsdennis/json-everything/issues/677) - Fixes an issue with serializing schemas that use custom keywords when those keywords are not registered with a `JsonTypeInfo` because reflection is okay.  Thanks to [@ProTip](https://github.com/ProTip) for reporting this and providing a reproduction.

# [6.0.3](https://github.com/gregsdennis/json-everything/pull/660) {#release-schema-6.0.3}

Fixes an issue with repeated evaluations of the same schema with different or changed options.  The solution was to recalculate any static analysis when an options change was detected.

# [6.0.2](https://github.com/gregsdennis/json-everything/pull/651) {#release-schema-6.0.2}

Reverted usages of `DependsOnAnnotationsFromAttribute<T>` to non-generic form in order to support .Net Framework, which doesn't support generic attributes.

> The generic attribute is still defined and usable, but only do so if you're not targeting .Net Framework.
{: .prompt-info }

# [6.0.0 & 6.0.1](https://github.com/gregsdennis/json-everything/pull/619) {#release-schema-6.0.0}

> v6.0.0 is missing an explicit .Net 8 DLL.  The package was repaired and republished as v6.0.1.

Updated for multi-framework support with .Net Standard 2.0 and .Net 8 with Native AOT support, including updating _System.Text.Json_ from v6 to v8.

Most of the changes to support Native AOT involve either updating internal implementation or creating overloads that do.  Whatever could not be updated was marked as requiring reflection, which will generate warnings when building Native AOT applications.

## Breaking changes

- Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 
- Keyword converters are no longer responsible for writing their property name when serializing.  This is now handled by `JsonSchema`'s converter.
- `ErrorMessages.Dependencies` removed.  Was previously marked obsolete.  Use `.DependentSchemas` and `.DependentRequired` when customizing the message for `dependencies`.
- `ErrorMessages.InvalidItemsForm` removed.  Invalid form of `items` results in an exception.
- `ErrorMessages.InvalidPattern` removed.  Invalid patterns result in an exception.
- `JsonSchemaBuilderExtensions.Validate()` removed.  Was previously marked obsolete.
- `JsonSchemaExtensions.Validate()` and overloads removed.  Were previously marked obsolete.
- `JsonNodeExtensions.VerifyJsonObject()` removed.  Duplicate keys throws an exception.
- Custom keywords MUST now implement `IWeaklyTypedJsonConverter` (or derive from `WeaklyTypedJsonConverter<T>` from _Json.More.Net_).

## Additions

- `JsonSchema.TypeInfoResolver` to expose all of the type resolvers contained in the library.  Can be used to create a combined `SerializationOptions` by using a `Json.More.TypeResolverOptionsManager` in your `JsonSerializerContext`.
- `DependsOnAnnotationsFromAttribute<T>` (non-generic version still exists)

## Other chanages

- `JsonSchema.FromFile(string, JsonSerializerOptions?)` marked AOT-incompatible; added `.FromFile(string)`.
- `JsonSchema.FromText(string, JsonSerializerOptions?)` marked AOT-incompatible; added `.FromText(string)`.
- `JsonSchema.FromStream(string, JsonSerializerOptions?)` marked AOT-incompatible; added `.FromStream(string)`.
- `SchemaKeywordRegistry.Register<T>()` marked AOT-incompatible, usages to be replaced with `.Register<T>(JsonSerializerContext)`.
- `[JsonSchema]` attribute marked AOT-incompatible.
- Methods in `ValidatingConverter` marked AOT-incompatible.

# [5.5.1](https://github.com/gregsdennis/json-everything/pull/616) {#release-schema-5.5.1}

Fix typo in `minProperties` error message.  Thanks to [@ArmaanMcleod](https://github.com/ArmaanMcleod) for submitting this fix.

# [5.5.0](https://github.com/gregsdennis/json-everything/pull/606) {#release-schema-5.5.0}

[#604](https://github.com/gregsdennis/json-everything/pull/604) - Performance improvements.  Thanks to [@danielstarck](https://github.com/danielstarck) for contributing these.

[#605](https://github.com/gregsdennis/json-everything/pull/605) - Support external types for validating JSON converter.

# [5.4.3](https://github.com/gregsdennis/json-everything/pull/601) {#release-schema-5.4.3}

[#600](https://github.com/gregsdennis/json-everything/issues/600) - Reported schema location not correct for JSON Schemas embedded within non-schema JSON documents (e.g. OpenAPI).  Thanks to [@Fresa](https://github.com/Fresa) for reporting.

# [5.4.2](https://github.com/gregsdennis/json-everything/pull/575) {#release-schema-5.4.2}

[#574](https://github.com/gregsdennis/json-everything/issues/574) - Bundling multi-schema recursion throws an exception.  Thanks to [@Tommyknocker1982](https://github.com/Tommyknocker1982) for reporting.

# [5.4.1](https://github.com/gregsdennis/json-everything/pull/572) {#release-schema-5.4.1}

[#570](https://github.com/gregsdennis/json-everything/issues/570) - RFC 3339 date/times require the time zone offset.  Thanks to [@danielstarck](https://github.com/danielstarck) for identifying and fixing this issue.

# [5.4.0](https://github.com/gregsdennis/json-everything/pull/565) {#release-schema-5.4.0}

[#549](https://github.com/gregsdennis/json-everything/issues/549) - Expose JSON converters for all types to support JSON source generation.

# [5.3.1](https://github.com/gregsdennis/json-everything/pull/534) {#release-schema-5.3.1}

[#550](https://github.com/gregsdennis/json-everything/issues/550) - Fixed a stack overflow that would occur with `ValidatingJsonConverter` when using multiple options objects.  Thanks to [@EmmyDream](https://github.com/EmmyDream) for reporting.

# [5.3.0](https://github.com/gregsdennis/json-everything/pull/534) {#release-schema-5.3.0}

Added `EvaluationOptions.AddAnnotationForUnknownKeywords` which adds an annotation that contains an array of unknown keywords in the schema.

# [5.2.7](https://github.com/gregsdennis/json-everything/pull/534) {#release-schema-5.2.7}

Fixed an issue with resolving `$ref`s that point into supported non-applicator keywords.  (JSON Schema optional behavior)

Fixed deserialization bug where `allOf`, `anyOf`, and `oneOf` would allow a single schema instead of an array of schemas.  This now throws a `JsonException` as expected.

# [5.2.6](https://github.com/gregsdennis/json-everything/pull/534) {#release-schema-5.2.6}

Fixed an issue where subschemas that declared different `$schema` from their parent couldn't use features that were undefined by parent dialect but defined by the subschema dialect.

# [5.2.5](https://github.com/gregsdennis/json-everything/pull/522) {#release-schema-5.2.5}

Found an issue with constraint generation in parallel processing scenarios.  Constraints should only need to be generated once.  Thanks to [@MODUSCarstenScholling](https://github.com/MODUSCarstenScholling) for providing data to replicate the issue.

Also updates _Json.More.Net_ reference to v1.9.0.

# [5.2.4](https://github.com/gregsdennis/json-everything/commit/825e59424e55600da5be3f52c86b7ef68be91bba) {#release-schema-5.2.4}

`minItmes` was using the `maxItems` error message.  Classic copy/paste error.

# [5.2.3](https://github.com/gregsdennis/json-everything/pull/521) {#release-schema-5.2.3}

Unrecognized keywords were not being processed (no annotations produced) for schemas that included a `$schema` keyword.

> If you're also using _JsonSchema.Net.Data_, you'll need to update to v4.0.1 of that library.
{: .prompt-warning }

# [5.2.2](https://github.com/gregsdennis/json-everything/pull/518) {#release-schema-5.2.2}

[#516](https://github.com/gregsdennis/json-everything/issues/516) - `JsonSchemaBuilder.Build()` throws an exception when multiple unrecognized keywords have been added.  Thanks to [@mdekrey](https://github.com/mdekrey) for finding this and suggesting the fix.
[#517](https://github.com/gregsdennis/json-everything/issues/517) - Deserialization of `JsonSchema` does not correctly report line numbers and positions in source JSON text.  Thanks to [@mdekrey](https://github.com/mdekrey) for finding this and implementing the fix.

# [5.2.1](https://github.com/gregsdennis/json-everything/pull/514) {#release-schema-5.2.1}

[#511](https://github.com/gregsdennis/json-everything/pull/511) - `JsonNodeBaseDocument` had some concurrency issues.  Thanks to [@Fresa](https://github.com/Fresa) for finding and fixing this issue.

# [5.2.0](https://github.com/gregsdennis/json-everything/pull/505) {#release-schema-5.2.0}

Added `JsonSchema.GetAnchor()`.

`oneOf` now produces an annotation of the number of successful validations (not required by the spec).

Fixed bug in keyword access extensions.  These methods should return null if the keyword is not found, but instead would throw exceptions.

[#504](https://github.com/gregsdennis/json-everything/issues/504) - Fixed bug for `JsonSchema.GetKeyword()` and `JsonSchema[string]`.  These should return null if the keyword doesn't exist but would throw exceptions.  Thanks to [@mirkoSekulic](https://github.com/mirkoSekulic) for reporting this.

[#506](https://github.com/gregsdennis/json-everything/issues/506) - Fixed bug for `JsonSchema.FindSubschema()` where the pointer lead into a keyword that held a single subschema, e.g. `additionalProperties`.  Thanks to [@Splamy](https://github.com/Splamy) for reporting this.

# [5.1.3](https://github.com/gregsdennis/json-everything/commit/fe728123a712a43bad6f8a51a625e41f9d459925) {#release-schema-5.1.3}

Just expose the local schema that you're already storing, you doof.

Adds `SchemaConstraint.LocalSchema`.  Removed method since it didn't actually work.  Bad idea.

# [5.1.2](https://github.com/gregsdennis/json-everything/commit/d3a3838882747bb1130860df43cf8ce1fbc48e24) (unlisted) {#release-schema-5.1.2}

This one, too.

Fixing 👇.  Test your code, folks!

# [5.1.1](https://github.com/gregsdennis/json-everything/commit/db49b4c2c062c0ab6f258f5fe6e355d947157e83) (unlisted) {#release-schema-5.1.1}

Just ignore this.

Added `SchemaConstraint.GetLocalSchema()` for accessing the local schema.  (To be used as a last resort, but may be necessary.)

# [5.1.0](https://github.com/gregsdennis/json-everything/pull/499) {#release-schema-5.1.0}

[#493](https://github.com/gregsdennis/json-everything/issues/493) - Add support for reporting error messages in multiple cultures in parallel.  Thanks to [@m-adamkiewicz](https://github.com/m-adamkiewicz) for the suggestion.

# [5.0.0](https://github.com/gregsdennis/json-everything/pull/494) {#release-schema-5.0.0}

New architecture for keyword evaluation that uses static analysis to save some evaluation work, thus reducing execution times and memory allocations significantly.

## Breaking Changes {#release-schema-5.0.0-breaks}

- `IJsonSchemaKeyword.Evaluate()` removed, replaced by `IJsonSchemaKeyword.GetConstraint()`.  All keywords updated.
- `EvaluationContext` completely reworked; now carries a lot less information.
- `IEquatable<T>` no longer required for keyword implementations.  Equality methods removed from all keywords and from `JsonSchema`.
- `EvaluationResults.Ignore()` removed as keyword evaluations can be skipped using other means.
- `[SchemaPriority]` attribute removed as priorities are now calculated based on usage of `[DependsOnAnnotationsFrom]` attribute.

## Additions {#release-schema-5.0.0-adds}

- `IJsonSchemaKeyword.GetConstraint()`
- `SchemaConstraint`
- `SchemaEvaluation`
- `KeywordConstraint`
- `KeywordEvaluation`

## Other Changes {#release-schema-5.0.0-other}

- Updated English language resource file for clearer error messages.

# [4.1.8](https://github.com/gregsdennis/json-everything/pull/490) {#release-schema-4.1.8}

Fixes several issues where keyword names were not reported in the evaluation path in the output.

# [4.1.7](https://github.com/gregsdennis/json-everything/commit/21b1fe73393508d84c90d00cc8f22aeaa958a2b5) {#release-schema-4.1.7}

Update keyword selection for custom drafts (support for upcoming OpenAPI project).

# [4.1.6](https://github.com/gregsdennis/json-everything/commit/6adac438c32d5324f3b9826db758a5f8b73ffe0f) {#release-schema-4.1.6}

`schemaLocation` in the draft-next output formatting requires a pointer fragment, even if that fragment is empty (refers to the root).

# 4.1.5 (no PR) {#release-schema-4.1.5}

Republished, splitting language packs into their own Nuget packages.

# [4.1.4](https://github.com/gregsdennis/json-everything/pull/473) {#release-schema-4.1.4}

The JSON Schema spec states that the value of the `enum` keyword SHOULD contain unique values, but this isn't a hard requirement.  This change allows duplicate values.

# [4.1.3](https://github.com/gregsdennis/json-everything/pull/470) {#release-schema-4.1.3}

[#457](https://github.com/gregsdennis/json-everything/issues/457) - [@pawel-wachalski](https://github.com/pawel-wachalski) added Norwegian and Swedish as supported translations for error messages.

# [4.1.2](https://github.com/gregsdennis/json-everything/pull/469) {#release-schema-4.1.2}

[#468](https://github.com/gregsdennis/json-everything/issues/468) - [@Fresa](https://github.com/Fresa) found that `JsonNodeBaseDocument` doesn't properly resolve references in schemas that it contatins and proposed the fix.

# [4.1.1](https://github.com/gregsdennis/json-everything/pull/447) {#release-schema-4.1.1}

[Powershell PR #19610](https://github.com/PowerShell/PowerShell/pull/19610) - Support for \*nix-based file paths caused an error in setting the base URI that resulted in the first folder in the path being interpreted as the URI host.

# [4.1.0](https://github.com/gregsdennis/json-everything/pull/443) {#release-schema-4.1.0}

Adds support for schema validation during deserialization.

- Adds `[JsonSchema]` attribute
- Adds `ValidatingJsonConverter`

# [4.0.7](https://github.com/gregsdennis/json-everything/pull/442) {#release-schema-4.0.7}

v4.0.1 used an ISO 8601 regex to enable higher-precision support.  This has been corrected to use an RFC 3339 regex instead for proper JSON Schema support.  However, as reported in [#439](https://github.com/gregsdennis/json-everything/issues/439), .Net doesn't like deserialization of RFC 3339 date-times.

# [4.0.6](https://github.com/gregsdennis/json-everything/pull/436) {#release-schema-4.0.6}

Schemas loaded with `.FromFile()` have a bad URI set that prevents fragments being added.  This is a [.Net bug](https://github.com/dotnet/runtime/issues/85229).  The result of this bug is that schemas loaded with `.FromFile()` cannot resolve references correctly.  This update includes a workaround to fix the behavior.  Thanks to [@faldor20](https://github.com/faldor20) for working on the issue initially, and to [@jpmikkers](https://github.com/jpmikkers) for finding a scenario that led to finding the root cause.

# [4.0.5](https://github.com/gregsdennis/json-everything/pull/434) {#release-schema-4.0.5}

[#432](https://github.com/gregsdennis/json-everything/issues/432) - Local `$ref`s that occur at the root of a draft 6 or 7 schema cause nested `$ref`s in the same schema resource to fail.  Thanks to [@Jeroen-VDW](https://github.com/Jeroen-VDW) for following up.

# [4.0.4](https://github.com/gregsdennis/json-everything/pull/433) {#release-schema-4.0.4}

[#432](https://github.com/gregsdennis/json-everything/issues/432) - Local `$ref`s that occur at the root of a draft 6 or 7 schema don't resolve.  Thanks to [@Jeroen-VDW](https://github.com/Jeroen-VDW) for reporting this issue.

# [4.0.3](https://github.com/gregsdennis/json-everything/pull/427) {#release-schema-4.0.3}

[#426](https://github.com/gregsdennis/json-everything/issues/426) - `$ref`s that pointed to schemas with `file://` URIs don't resolve correctly.  Thanks to [@faldor20](https://github.com/faldor20) for finding and fixing this issue.

# [4.0.2](https://github.com/gregsdennis/json-everything/pull/423) {#release-schema-4.0.2}

[#419](https://github.com/gregsdennis/json-everything/issues/419) - Fixed an issue with schema registration and populating IDs.  Thanks to [@lennver](https://github.com/lennver) for providing reproduction for this issue.

# [4.0.1](https://github.com/gregsdennis/json-everything/pull/418) {#release-schema-4.0.1}

- Add `IIdKeyword` to potentially support using an ID keyword other than `$id` (foot gun, generally, please don't use)
- Fix bundling issue when the schema contains `items` - thanks to [@amis92](https://github.com/amis92) for finding and reporting this bug
- `JsonSchema.GetType()` extension is blocked by `object.GetType()`, so it has been renamed to `.GetJsonType()`
- Add implicit conversions from `JsonSchema`, `List<string>`, and `string[]` to `SchemaOrPropertyList` to help with `JsonSchemaBuilder.Dependencies()` syntax
- Add support for higher-precision `date-time` formats - thanks to [@faldor20](https://github.com/faldor20) for submitting this

# [4.0.0](https://github.com/gregsdennis/json-everything/pull/326) {#release-schema-4.0.0}

This update adds support for the latest version of JSON Schema while also making a few architectural changes to ease the burden for implementing custom keywords.

It also renames several methods from "Validate" to "Evaluate" to reflect the idea that processing a schema has more uses than merely validation.  Extension methods have been created (and marked with `[Obsolete]`) to ease the transition in consuming code.  These extension methods will be removed with the next major version.

## Breaking Changes {#release-schema-4.0.0-breaks}

- `JsonSchema`
  - `Validate()` renamed to `Evaluate()`
  - `ValidateSubschema()` removed, replaced with `EvaluationContext.Evaluate()`
- `ValidationOptions`
  - renamed to `EvaluationOptions`
  - `ValidateAs` renamed to `EvaluateAs`
  - `ValidateMetaSchema` renamed to `ValidateAgainstMetaSchema`
  - `DefaultBaseUri` removed.  Schemas now generate a random base URI upon creation.  The pre-evaluation scan has been updated to set base URIs for all schemas and subschemas.
- `ValidationResults`
  - renamed to `EvaluationResults`
  - `ConsolidateAnnotations()` removed
  - `TryGetAnnotation()` updated to well-known "try" pattern: `bool TryGetAnnotations(string, out JsonNode)`
  - `Pass()` removed (keyword is assumed to pass unless `Fail` is called)
- `Annotation` type removed (annotations are simply saved as `JsonNode`s in `EvaluationResults`)
- `ValidationContext`
  - renamed to `EvaluationContext`
  - `ValidationOptions` renamed to `EvaluationOptions`
  - `Push()` optional parameters removed and overloads created
    - `Push(JsonPointer instanceLocation, JsonNode? instance, JsonPointer evaluationPath, JsonSchema subschema, Uri? newUri = null)` used for new schema & instance locations (e.g. keywords like `properties`)
    - `Push(JsonPointer evaluationPath, JsonSchema subschema, Uri? newUri = null)` used for new schema locations evaluating the same instance location (e.g. keywords like `anyOf`)
- `IJsonSchemaKeyword.Validate()` renamed to `IJsonSchemaKeyword.Evaluate()` (applies to all keyword types as well)
- `JsonSchemaExtensions.Validate()` renamed to `JsonSchemaExtensions.Evaluate()`
- `ErrorMessages` & associated string resources
  - `Contains` replaced with `ContainsTooFew` and `ContainsTooMany`
  - `MaxContains` and `MinContains` removed
  - reference-related errors removed
- `Draft` renamed to `SpecVersion` to align with the JSON Schema movement toward a stable specification
  - `SchemaDraftAttribute` renamed to `SchemaSpecVersionAttribute`
  - related methods and properties have also been updated
- `ApplicatorAttribute` has been removed as it was unused
  - `KeywordExtensions.IsApplicator()` also removed
- `IAnchorProvider` has been removed as its function has been internalized
- `IRefResolvable` has been removed as its function has been internalized
- `SchemaRegistry`
  - `RegisterAnchor()` remove as its function has been internalized
  - `anchor` parameter removed from `Get()`

### Changes Driven by JSON Schema {#release-schema-4.0.0-spec}

- Output formats are now `Flag`, `List`, and `Hierarchical`
  - `ToBasic()` has been renamed to `ToList()`
  - `ToDetailed()` has been removed
  - `Pre202012EvaluationResultsJsonConverter` has been added to serialize output as per draft 2020-12 and previous (with the exception of `Detailed`).
- Output structure now modelled after JSON Schema `draft-next` output
  - `ValidationResults.SchemaLocation` renamed to `EvaluationResults.EvaluationPath`
  - `ValidationResults.AbsoluteSchemaLocation` renamed to `EvaluationResults.SchemaLocation` (note the same property name and different function)
- A new exception `JsonSchemaException` will now be thrown for various scenarios where previously a failed validation would have been returned
  - failure to resolve `$schema`, `$ref`, `$recursiveRef`, and `$dynamicRef`
  - a keyword's value is in a form that the current specification version does not allow (e.g. array form of `items` in draft 2020-12)

## Additional Changes {#release-schema-4.0.0-other}

- Exposed static property `Name` on all keywords
- Added `IBaseDocument` to represent any identifiable JSON document that may house a JSON Schema
- Added `JsonNodeBaseDocument` as a general-use base document for any `JsonNode`
- `JsonSchema`
  - Implements `IBaseDocument`
    - `BaseUri` is now public
    - `FindSubschema()` is now public
  - `IsResourceRoot` has been added
  - `DeclaredVersion` has been added
- `JsonSchemaExtensions`
  - Added extensions for each keyword to enable easier keyword value access, e.g. `schema.GetTitle()`
  - Added `.Bundle()` to generate a bundled schema that includes all referenced schemas
  - Added `.Validate()` extensions to help ease transition from `.Validate()` to `.Evaluate()`
- Added `PropertyDependenciesKeyword`
- Added `ICustomSchemaCollector` to handle keywords that contain subschemas but don't fit into one of the other "container" interfaces, e.g. `propertyDependencies`
- Added `MetaSchemas.DraftNext` and associated properties
- Added `Vocabularies.DraftNext` and associated properties
- Added `EvaluationOptions.IgnoredAnnotations` to ignore annotations from specified keywords.  These keywords are managed using:
  - `IgnoreAnnotationsFrom<T>() where T : IJsonSchemaKeyword`
  - `IgnoreAllAnnotations()`
  - `ClearIgnoredAnnotations()`
  - `CollectAnnotationsFrom<T>()`
- Added `DependsOnAnnotationsFromAttribute` to support collecting annotations that have been configured to be excluded but are required for other keywords to function
- Added `Vocabularies.DraftNext`, `MetaSchemas.DraftNext` and associated vocabs and meta-schemas
  - these will be renamed to match the next release of the JSON Schema specification before v4.0 is fully released

Also includes various performance enhancements, both increasing processing speed and reducing memory allocations.

# [3.3.2](https://github.com/gregsdennis/json-everything/pull/347) {#release-schema-3.3.2}

Fixed issue where annotation collection is skipped but shouldn't be when output format is configured to `flag` and `unevaluated*` keywords are present in the schema.

# [3.3.1](https://github.com/gregsdennis/json-everything/pull/346) {#release-schema-3.3.1}

Fixed issue where dynamically-loaded schemas were not scanned for URI identifier keywords.  Was not an issue if all referenced schemas were explicitly reloaded.

# [3.3.0](https://github.com/gregsdennis/json-everything/pull/344) {#release-schema-3.3.0}

[#340](https://github.com/gregsdennis/json-everything/issues/340) - Added `ValidationOptions.ProcessCustomKeywords` to allow custom keywords for schema versions 2019-09 and later.

# [3.2.1](https://github.com/gregsdennis/json-everything/pull/330) {#release-schema-3.2.1}

Fixed absolute schema location in output.  The JSON Schema team identified some edge cases involving `$dynamicRef` where the wrong URI was reported.

# [3.2.0](https://github.com/gregsdennis/json-everything/pull/300) {#release-schema-3.2.0}

Added `JsonSchemaBuilderExtensions.PatternProperties()` overloads that take strings in place of `Regex`es.  There is no syntax highlighting support for these overloads, however.

# [3.1.5](https://github.com/gregsdennis/json-everything/pull/299) {#release-schema-3.1.5}

🤦There were multiple issues with the meta-schemas because the self-validation tests were initially disabled (equality hadn't been implemented yet).  Re-enabling these tests highlighted several problems.

- All meta-schemas adjusted to conform to online versions.
- It's `contentEncoding` not `contentMediaEncoding`...

# 3.1.4 (No PR) {#release-schema-3.1.4}

Continuation of v3.1.3 - Fixed array-valued `items`.

# [3.1.3](https://github.com/gregsdennis/json-everything/pull/297) {#release-schema-3.1.3}

Fixed an issue with handling JSON null in instance data for keywords which change the instance location:

- `additionalItems`
- `additionalProperties`
- `contains`
- `items`
- `patternProperties`
- `prefixItems`
- `properties`
- `unevaluatedItems`
- `unevaluatedProperties`

# [3.1.2](https://github.com/gregsdennis/json-everything/pull/294) {#release-schema-3.1.2}

Updated `min*`/`max*` keywords to accept numbers with decimals as long as they still represent non-negative integer values, e.g. `1.0`.

# 3.1.1 (no PR) {#release-schema-3.1.1}

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [3.1.0](https://github.com/gregsdennis/json-everything/pull/285) {#release-schema-3.1.0}

[#284](https://github.com/gregsdennis/json-everything/issues/284) - Added optional `JsonSerializerOptions` parameter in `JsonSchema.FromFile()`, `.FromText()`, and `.FromStream()`.

# [3.0.0](https://github.com/gregsdennis/json-everything/pull/280) {#release-schema-3.0.0}

Updated all functionality to use `JsonNode` instead of `JsonElement`.

## Breaking Changes {#release-schema-3.0.0-breaks}

_`JsonElement` -> `JsonNode` type exchange changes not listed._

- Removed obsolete members
  - `IRefResolvable.ResolvePointerSegment()` (and implementations)
  - `Format.Validate()` (and implementations)
  - `JsonSchema.OtherData` (and associated constructor parameter) which is now supported by `UnrecognizedKeyword`
- Removed all `JsonBuilderExtensions` methods which take `JsonElementProxy` since `JsonNode` defines implicit casts from the appropriate types
- `LogExtensions.WrongValueKind()` now takes `SchemaValueType` instead of `JsonValueKind`
- Removed `JsonSchemaExtensions(this JsonSchema, string, ValidationOptions)` since the implicit cast from `JsonNode` was taking precendence

    ***IMPORTANT** Removal of this extension will only manifest at runtime.  Code that called this extension method will still compile, but the validations will most likely fail.*

## Additional Changes {#release-schema-3.0.0-new}

- Added `JsonNodeExtensions` to provide schema-specific functionality on top of what's provided in Json.More.Net
  - `.GetSchemaValueType()` to get the JSON Schema type represented by a node
  - `.VerifyJsonObject()` to verify that the underlying data of a `JsonObject` can be processed.  See [this issue](https://github.com/dotnet/runtime/issues/70604) for more information.
- Added `JsonSchemaExtensions.Validate(this JsonSchema, JsonElement, ValidationOptions)` to continue supporting elements
- Added `JsonSchemaBuilderExtensions.Validate(this JsonSchemaBuilder, JsonNode, ValidationOptions)` as a convenience method
    ```c#
    // instead of
    var schema = builder.Build();
    var results = schema.Validate(json);

    // you can do
    var results = builder.Validate(json);
    ```

# [2.4.0](https://github.com/gregsdennis/json-everything/pull/270) {#release-schema-2.4.0}

Added `UnrecognizedKeyword` to represent keywords that were not recognized by any known vocabulary.  The values of these keywords are then captured in the validation results as annotations.  As a result of this change `JsonSchema.OtherData` has been marked obsolete.  

# [2.3.0](https://github.com/gregsdennis/json-everything/pull/249) {#release-schema-2.3.0}

[#190](https://github.com/gregsdennis/json-everything/issues/190) - Added support for custom and localized error messages.

Added

- `ErrorMessages` static class which exposes all error message properties and allows for customization.
- Embedded string resources which defines all error messages.

# [2.2.1](https://github.com/gregsdennis/json-everything/pull/247) {#release-schema-2.2.1}

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

[#245](https://github.com/gregsdennis/json-everything/issues/245) - [@ItsVeryWindy](https://github.com/ItsVeryWindy) identified where `allOf` was reporting locations incorrectly in validation output.

[#246](https://github.com/gregsdennis/json-everything/issues/246) - [@YaroslavHuriev](https://github.com/YaroslavHuriev) noticed an issue with some formats where non-strings were not properly ignored.  As part of this fix, `Format.Validate(JsonElement)` was marked obsolete as it has been replaced and is no longer called.  It will be removed with the next major version.

# [2.2.0](https://github.com/gregsdennis/json-everything/pull/239) {#release-schema-2.2.0}

[#238](https://github.com/gregsdennis/json-everything/pull/238) - [@WeihanLi](https://github.com/WeihanLi) added extenion methods to create schemas directly from `JsonDocument` and strings that contain JSON data.

# [2.1.2](https://github.com/gregsdennis/json-everything/pull/230) {#release-schema-2.1.2}

[#220](https://github.com/gregsdennis/json-everything/issues/220) - References to deeply nested subschemas could not be resolved through keywords that contained a single schema.

Resolving the above resulted in not needing `IRefResolvable.ResolvePointerSegment(string)`.  This method has been marked obsolete on the interface and will be removed with the next major version.

# [2.1.1](https://github.com/gregsdennis/json-everything/pull/227) {#release-schema-2.1.1}

[#226](https://github.com/gregsdennis/json-everything/issues/226) - Fixed an issue with including unneeded nodes in Basic output.

# [2.1.0](https://github.com/gregsdennis/json-everything/pull/218) {#release-schema-2.1.0}

Added `JsonSchema.BoolValue` to easily detect boolean schemas.  Previously required checking `JsonSchema.Keywords` for null.

Fixed an issue with `duration` format parsing.

# [2.0.3](https://github.com/gregsdennis/json-everything/pull/217) {#release-schema-2.0.3}

[#216](https://github.com/gregsdennis/json-everything/issues/208) - `additionalProperties` does not determine the correct property set to evaluate for drafts 6 and 7. Reported by [@joyse1](https://github.com/joyse1).

Fixed a bug in keyword filtering for subschemas.

# [2.0.2](https://github.com/gregsdennis/json-everything/pull/213) {#release-schema-2.0.2}

[#208](https://github.com/gregsdennis/json-everything/issues/208) - [@ItsVeryWindy](https://github.com/ItsVeryWindy) discovered a regression where resolve local references to definitions located inside a bundled schema fail to resolve.

# [2.0.1](https://github.com/gregsdennis/json-everything/pull/210) {#release-schema-2.0.1}

[#207](https://github.com/gregsdennis/json-everything/issues/207) - [@ItsVeryWindy](https://github.com/ItsVeryWindy) discovered a regression where bundling (embedding a schema of one draft inside a schema of another) fails to process the required keyword set.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/200) {#release-schema-2.0.0}

Primary change is around how `ValidationContext` is handled throughout the validation process in order to significantly reduce memory footprint.  Thanks to [@dazerdude](https://github.com/dazerdude) for reporting the issue and making suggestions that led to resolving this.

The majority of this change affects how keywords are implemented.  Unless you're implementing your own keywords, it's unlikely that this change will break your code.

The validation context now only contains data required for validation.  The validation result is now set directly on the validation result, which is also now exposed by the context.

## Removed {#release-schema-2.0.0-removed}

- `ValidationContext`
  - `IsValid`
  - `Ignore`
  - `Message`
  - `Annotations`
  - `NestedContexts`
  - `HasNestedContexts`
  - `SiblingContexts`
  - `HasSiblingContexts`
  - `DynamicAnchors` (was marked obsolete)
  - `From()` static method
  - `ConsolidateAnnotations()` (moved to `ValidationResults`)
  - `SetAnnotation()` (moved to `ValidationResults`)
  - `TryGetAnntation()` (moved to `ValidationResults`)
  - `RegisterConsolidationMethod()` (moved to `ValidationResults`)
  - `ContextConsolidator` delegate (moved to `ValidationResults`)
- `ValidationOptions`
  - `ValidationFormat` (was marked obsolete)


## Changed {#release-schema-2.0.0-changed}

- Keywords now _require_ a `[SchemaDraft()]` attribute to declare compatible drafts.
- `Draft` is now a `[Flags]` enum and can be combined with the bitwise OR `|` operator.
- `EnumKeyword` now uses a hash set to store values (improves performance).  This means that it requires unique values.  An `ArgumentException` will now be thrown if attempting to deserialize a schema with duplicate values inside an `enum`.
- `ValidationContext.SchemaRoot` is now read-only.  This is set by the initial call to `.Validate()` and shouldn't need to be set by the client.

## Added {#release-schema-2.0.0-new}

- `KeywordExtentions`
  - `SupportsDraft()`
- `ValidationContext`
  - `Push()`
  - `Pop()`
- `ValidationResults`
  - `HasNestedResults`
  - `HasAnnotations`
  - `Parent`
  - `ConsolidateAnnotations()` (moved from `ValidationContext`)
  - `SetAnnotation()` (moved from `ValidationContext`)
  - `TryGetAnntation()` (moved from `ValidationContext`)
  - `RegisterConsolidationMethod()` (moved from `ValidationContext`)
  - `ContextConsolidator` delegate (moved from `ValidationContext`)
  - `GetAllAnnotations<T>()`
  - `Pass()`
  - `Fail()`

# [1.13.0](https://github.com/gregsdennis/json-everything/pull/204) {#release-schema-1.13.0}

[#203](https://github.com/gregsdennis/json-everything/issues/203) - [@coderpatros](https://github.com/coderpatros) discovered a regression in `format` validation when the format is unkonwn.

Added option `OnlyKnownKeywords` to support validation failures on unknown keywords.

***NOTE** Since this is fixing a regression, I'm only bumping the minor version.

# [1.12.0](https://github.com/gregsdennis/json-everything/pull/196) {#release-schema-1.12.0}

[#191](https://github.com/gregsdennis/json-everything/issues/191) - Self-referencing meta-schemas cause an infinite loop during validation.

[#193](https://github.com/gregsdennis/json-everything/issues/193) - Custom format validations don't provide sufficient information and don't cause validation to fail when assertion is required.

- Added `UnknownFormat` and `Format.CreateUnknown`.
- Added `JsonSchemaBuilder.Format(string)` which looks up the format key.

# [1.11.6](https://github.com/gregsdennis/json-everything/pull/189) {#release-schema-1.11.6}

Fixed bugs highlighted by updating test suite repository.

- Advanced use cases for `$dynamicRef` and `$dynamicAnchor`
- Added `PatternKeyword.InvalidPattern` and `PatternPropertiesKeyword.InvalidPatterns` to handle text that cannot be deserialized as a `RegEx`.  These will now generate validation failures rather than deserialization exceptions.
- Keywords are now properly filtered by their vocabularies if the meta-schema declared by `$schema` includes a `$vocabulary` keyword.
- Added `VocabularyRegistry.Get(Uri)`.

# [1.11.5](https://github.com/gregsdennis/json-everything/pull/179) {#release-schema-1.11.5}

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.11.4](https://github.com/gregsdennis/json-everything/pull/176) {#release-schema-1.11.4}

[#175](https://github.com/gregsdennis/json-everything/pull/175) - [@ranma42](https://github.com/ranma42) tidied up the schema registry initialization code, also resolving some concurrency issues.

# [1.11.3](https://github.com/gregsdennis/json-everything/pull/166) {#release-schema-1.11.3}

[#165](https://github.com/gregsdennis/json-everything/issues/165) - [@pbajrak](https://github.com/pbajrak) discovered a copy/paste error in the message for `minProperties`.

# [1.11.2](https://github.com/gregsdennis/json-everything/pull/157) {#release-schema-1.11.2}

Fixed minor issue with `$dynamicRef`/`$dynamicAnchor` and resolution within specific scopes.

# [1.11.1](https://github.com/gregsdennis/json-everything/pull/156) {#release-schema-1.11.1}

Some `format` validations were incorrectly failing validations for incompatible types.  For example, `uri` would fail on integers, but should pass (ignore non-strings).

# [1.11.0](https://github.com/gregsdennis/json-everything/pull/155) {#release-schema-1.11.0}

`format` incorporates error messages from custom format validation, when provided.

# [1.10.8](https://github.com/gregsdennis/json-everything/pull/152) {#release-schema-1.10.8}

`format` now adds an error message.

# [1.10.7](https://github.com/gregsdennis/json-everything/pull/145) {#release-schema-1.10.7}

Revenge of [#138](https://github.com/gregsdennis/json-everything/issues/138) - [@yohny](https://github.com/yohny) found that the initialization issue still existed when not supplying custom validation options.

# [1.10.6](https://github.com/gregsdennis/json-everything/pull/140) {#release-schema-1.10.6}

[#138](https://github.com/gregsdennis/json-everything/issues/138) - [@wazzamatazz](https://github.com/wazzamatazz) found an initialization issue that resulted in a `NullReferenceException`.

# [1.10.5](https://github.com/gregsdennis/json-everything/pull/135) {#release-schema-1.10.5}

Fixed an issue with reference resolution when `$ref` and `$id` are sibling keywords in drafts 6 and 7.

# [1.10.4](https://github.com/gregsdennis/json-everything/pull/133) {#release-schema-1.10.4}

[#132](https://github.com/gregsdennis/json-everything/pull/132) - Fixed some memory management issues around `JsonDocument` and `JsonElement`.  Thanks to [@ddunkin](https://github.com/ddunkin) for finding and fixing these.

# [1.10.3](https://github.com/gregsdennis/json-everything/pull/120) {#release-schema-1.10.3}

Added overload for `ExitKeyword` logging extension to pull the validition result from the context rather than having to pass it in.

# [1.10.2](https://github.com/gregsdennis/json-everything/pull/102) {#release-schema-1.10.2}

In 2020-12, `unevaluatedItems` should consider `contains`.

# [1.10.1](https://github.com/gregsdennis/json-everything/pull/98) {#release-schema-1.10.1}

[#97](https://github.com/gregsdennis/json-everything/issues/97) - Added support for circular reference detection.

# [1.10.0](https://github.com/gregsdennis/json-everything/pull/93) {#release-schema-1.10.0}

[#90](https://github.com/gregsdennis/json-everything/issues/90) - Added `JsonSchemaBuilder.Get<T>()` to support the discussion around incrementally adding properties while building schemas.

Added optional logging around schema processing decisions.

# [1.9.4](https://github.com/gregsdennis/json-everything/pull/92) {#release-schema-1.9.4}

Register the 2020-12 meta-schemas and vocabularies.

Fixed several issues with reusing options objects between validations.

Fixed `$dynamic*` keywords based on new tests in test suite.

# [1.9.3](https://github.com/gregsdennis/json-everything/pull/83) {#release-schema-1.9.3}

[#82](https://github.com/gregsdennis/json-everything/pull/82) - Fixing issues managing `ValidationContext.CurrentUri`.  Credit to [@amosonn](https://github.com/amosonn).

# [1.9.2](https://github.com/gregsdennis/json-everything/pull/80) {#release-schema-1.9.2}

[#79](https://github.com/gregsdennis/json-everything/issues/79) - `$ref` was calling out to resolve a parent URI.

# [1.9.1](https://github.com/gregsdennis/json-everything/pull/77) {#release-schema-1.9.1}

Fixes found by the release of the 2020-12 test suite.

- `$dynamicAnchor` and `$dynamicRef` behavior.
- make `items` and `unevaluatedItems` respect annotations from `prefixItems`.

# [1.9.0](https://github.com/gregsdennis/json-everything/pull/75) {#release-schema-1.9.0}

Added support for nullable reference types.

Fixed various evaluation bugs around the various keywords involved with references.

Fixed `content*` keywords so that they only validate if the instance is a string.

Fixed [#76](https://github.com/gregsdennis/json-everything/issues/76) - `.GetHashCode()` is inconsistent for schemas and keywords.  Credit to [@amosonn](https://github.com/amosonn) for pointing me to a good hash code method in the wild.

`RecursiveRef` parameterless constructor marked obsolete in favor of new constructor which takes a boolean value.  Schema builder extension also updated to include a parameter which defaults to `true` to maintain current behavior.

# [1.8.0](https://github.com/gregsdennis/json-everything/pull/72) {#release-schema-1.8.0}

Exposed `UriExtensions` to help support new keywords that manipulate URIs.

# [1.7.1](https://github.com/gregsdennis/json-everything/pull/68) {#release-schema-1.7.1}

Fixed [#67](https://github.com/gregsdennis/json-everything/issues/67) - `enum` keyword serializes as `examples` due to a copy-paste error.  Credit to [@dannyheard7](https://github.com/dannyheard7) for finding and fixing.  Also fixed several other related serialization issues.

# [1.7.0](https://github.com/gregsdennis/json-everything/pull/65) {#release-schema-1.7.0}

- Updated hard-coded 2020-12 meta-schemas to match the published documents.
- Added the following `JsonSchemaBuilder` extension method overloads:
  - `.DynamicRef(string)`
  - `.Const(JsonElementProxy)`
  - `.Default(JsonElementProxy)`
  - `.Enum(IEnumerable<JsonElementProxy>)`
  - `.Enum(params JsonElementProxy[])`
  - `.Examples(IEnumerable<JsonElement>)`
  - `.Examples(IEnumerable<JsonElementProxy>)`
  - `.Examples(params JsonElementProxy[])`
  
(See [Json.More.Net v1.3.0](json-more.md) for more information on `JsonElementProxy`.)

# [1.6.1](https://github.com/gregsdennis/json-everything/pull/61) {#release-schema-1.6.1}

Signed the DLL for strong name compatibility.

# [1.6.0](https://github.com/gregsdennis/json-everything/pull/52) {#release-schema-1.6.0}

Added support for Draft 2020-12.

- Meta-schema validation now always occurs superficially in that it validates that the meta-schema is known.  The `ValidationOptions.ValidateMetaSchema` option now only controls whether a full meta-schema validation of the schema will occur.  This should only affect clients validating schemas declaring custom meta-schemas with the `$schema` keyword.  Custom meta-schemas will now need to be loaded into the system manually or `SchemaRegistry.Fetch` will need to be set to retrieve it automatically.
- Added all new vocabularies and meta-schemas.
- New keywords:
  - `$dynamicRef` - replaces/augments `$recursiveRef`
  - `$dynamicAnchor` - replaces/augments `$recursivAnchor`
  - `prefixItems` - replaces array-form `items`
- Added `JsonSchemaBuilder` extension method for `$anchor` which should have been added for draft 2019-09 support.
- `ValidationOptions.ValidateFormat` has been obsoleted and replaced by `ValidationOptions.RequireFormatValidation` with the same semantics and default.
- `FormatKeyword` now responds to the presence of the format vocabularies in the meta-schema declared by the `$schema` keyword as well as the `ValidationOptions.RequireFormatValidation` option.  (Includes a bug fix for draft 2019-09 schemas that use a meta-schema that declare the format vocabulary with a value of `true`.)

# [1.5.4](https://github.com/gregsdennis/json-everything/pull/45) {#release-schema-1.5.4}

Added debug symbols to package.  No functional change.

# [1.5.3](https://github.com/gregsdennis/json-everything/pull/41) {#release-schema-1.5.3}

Fixed `NullReferenceException` when comparing two schemas.

# [1.5.2](https://github.com/gregsdennis/json-everything/pull/40) {#release-schema-1.5.2}

Updated wording for `enum` error message.  This must remain generic and cannot include the expected values because the list could be long and the values could be quite large.

# [1.5.1](https://github.com/gregsdennis/json-everything/pull/34) {#release-schema-1.5.1}

[#35](https://github.com/gregsdennis/json-everything/issues/35) `JsonSchema.FromFile()` handles file paths as URIs incorrectly in non-Windows systems. 

# [1.5.0](https://github.com/gregsdennis/json-everything/pull/34) {#release-schema-1.5.0}

[#33](https://github.com/gregsdennis/json-everything/issues/33) Added `ValidationOptions.ValidateFormat` which allows configuration of whether to validate the `format` keyword.  Also fixes a bug where the `format` keyword was validated by default for draft 2019-09 which specifies that it should only generate annotations by default.  Because this library favors the latest draft, this is the default behavior for all drafts.

As a further followup to #27 (below), basic output has been refined.

# [1.4.0](https://github.com/gregsdennis/json-everything/pull/31) {#release-schema-1.4.0}

[#27](https://github.com/gregsdennis/json-everything/issues/27) (reopened) Better reduction of detailed output format which eliminates the notion that any nodes *must* be kept.

[#29](https://github.com/gregsdennis/json-everything/issues/29) Relative `$id` keyword at root of schema was not supported.  Added `ValidationOptions.DefaultBaseUri` to be used when no other absolute URI is defined by the `$id` keyword.  Also now supports assuming the base URI from the file name.

# [1.3.1](https://github.com/gregsdennis/json-everything/pull/28) {#release-schema-1.3.1}

[#27](https://github.com/gregsdennis/json-everything/issues/27) Nodes in the basic and detailed output formats that match the overall outcome should be removed.  This also addresses several other bugs involving the output such as `absoluteKeywordLocation`.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/25) {#release-schema-1.3.0}

[#15](https://github.com/gregsdennis/json-everything/issues/15) Easier navigation of the schema and its subschemas. Added `ISchemaContainer`, `ISchemaCollector`, and `IKeyedSchemaCollector` for the varying sets of subschemas that keywords can have.  Added `SchemaKeywordExtensions.GetSubschemas()` extension method.

[#19](https://github.com/gregsdennis/json-everything/issues/19) Keyword filtering doesn't consider declared draft or `ValidationOptions.ValidateAs`.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/17) {#release-schema-1.2.0}

([json-schema<nsp>.org #358](https://github.com/json-schema-org/json-schema-org.github.io/pull/358)) Published draft 06 meta-schema doesn't match the copy in the spec repo.

[#16](https://github.com/gregsdennis/json-everything/issues/16) `JsonSchema` equality checking.  Along with this, added `IEquatable<T>` to `SchemaKeywordRegistry.Register<T>()`.

[#18](https://github.com/gregsdennis/json-everything/issues/18) `properties` keyword is processed with same priority as `additionalProperties` making keyword order important, but it shouldn't be.

Added `EnumerableExtensions.ContentsEqual()`.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/11) {#release-schema-1.1.0}

Added `SchemaRegistry.Fetch` property to enable automatic downloading of referenced schemas.

# [1.0.3](https://github.com/gregsdennis/json-everything/pull/11) {#release-schema-1.0.3}

[#9](https://github.com/gregsdennis/json-everything/pull/11) `if`/`then`/`else` are processed in serialized order instead of processing `if` first.

[#10](https://github.com/gregsdennis/json-everything/pull/10) Bug fix around deserialization of `readonly` keyword.

# [1.0.2](https://github.com/gregsdennis/json-everything/pull/7) {#release-schema-1.0.2}

Updated format `json-pointer` to require plain pointers.  URI-encoded pointers are invalid.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/6) {#release-schema-1.0.1}

Updated validation of formats `hostname`, `iri`, `uri`, `regex`, and `time`.

Fixed issue resolving references (`$ref` & `$recursiveRef`) to miscellaneous (non-keyword) schema data.

# 1.0.0 {#release-schema-1.0.0}

Initial release.
