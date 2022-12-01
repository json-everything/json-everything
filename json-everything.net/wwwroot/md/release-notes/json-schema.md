# [4.0.0 (beta 1)](https://github.com/gregsdennis/json-everything/pull/326)

This update adds support for the latest version of JSON Schema while also making a few architectural changes to ease the burden for implementing custom keywords.

It also renames several methods from "Validate" to "Evaluate" to reflect the idea that processing a schema has more uses than merely validation.  Extension methods have been created (and marked with `[Obsolete]`) to ease the transition in consuming code.  These extension methods will be removed with the next major version.

## Breaking Changes

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
- `ErrorMessages` & associated string resources
  - `Contains` replaced with `ContainsTooFew` and `ContainsTooMany`
  - `MaxContains` and `MinContains` removed
  - reference-related errors removed
- `JsonSchemaExtensions.Validate()` renamed to `JsonSchemaExtensions.Evaluate()`
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

### Changes Driven by JSON Schema

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

## Additional Changes

- Exposed static property `Name` on all keywords
- `JsonSchema`
  - `BaseUri` is now available
  - `IsResourceRoot` has been added
  - `DeclaredVersion` has been added
- Added `JsonSchemaExtensions.Validate()` extensions to help ease transition from "Validate" to "Evaluate"
- Added `PropertyDependenciesKeyword`
- Added `ICustomSchemaCollector` to handle keywords that contain subschemas but don't fit into one of the other "container" interfaces, e.g. `propertyDependencies`.
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

# [3.3.2](https://github.com/gregsdennis/json-everything/pull/347)

Fixed issue where annotation collection is skipped but shouldn't be when output format is configured to `flag` and `unevaluated*` keywords are present in the schema.

# [3.3.1](https://github.com/gregsdennis/json-everything/pull/346)

Fixed issue where dynamically-loaded schemas were not scanned for URI identifier keywords.  Was not an issue if all referenced schemas were explicitly reloaded.

# [3.3.0](https://github.com/gregsdennis/json-everything/pull/344)

[#340](https://github.com/gregsdennis/json-everything/issues/340) - Added `ValidationOptions.ProcessCustomKeywords` to allow custom keywords for schema versions 2019-09 and later.

# [3.2.1](https://github.com/gregsdennis/json-everything/pull/330)

Fixed absolute schema location in output.  The JSON Schema team identified some edge cases involving `$dynamicRef` where the wrong URI was reported.

# [3.2.0](https://github.com/gregsdennis/json-everything/pull/300)

Added `JsonSchemaBuilderExtensions.PatternProperties()` overloads that take strings in place of `Regex`es.  There is no syntax highlighting support for these overloads, however.

# [3.1.5](https://github.com/gregsdennis/json-everything/pull/299)

🤦There were multiple issues with the meta-schemas because the self-validation tests were initially disabled (equality hadn't been implemented yet).  Re-enabling these tests highlighted several problems.

- All meta-schemas adjusted to conform to online versions.
- It's `contentEncoding` not `contentMediaEncoding`...

# 3.1.4 (No PR)

Continuation of v3.1.3 - Fixed array-valued `items`.

# [3.1.3](https://github.com/gregsdennis/json-everything/pull/297)

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

# [3.1.2](https://github.com/gregsdennis/json-everything/pull/294)

Updated `min*`/`max*` keywords to accept numbers with decimals as long as they still represent non-negative integer values, e.g. `1.0`.

# 3.1.1 (no PR)

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [3.1.0](https://github.com/gregsdennis/json-everything/pull/285)

[#284](https://github.com/gregsdennis/json-everything/issues/284) - Added optional `JsonSerializerOptions` parameter in `JsonSchema.FromFile()`, `.FromText()`, and `.FromStream()`.

# [3.0.0](https://github.com/gregsdennis/json-everything/pull/280)

Updated all functionality to use `JsonNode` instead of `JsonElement`.

## Breaking Changes

_`JsonElement` -> `JsonNode` type exchange changes not listed._

- Removed obsolete members
  - `IRefResolvable.ResolvePointerSegment()` (and implementations)
  - `Format.Validate()` (and implementations)
  - `JsonSchema.OtherData` (and associated constructor parameter) which is now supported by `UnrecognizedKeyword`
- Removed all `JsonBuilderExtensions` methods which take `JsonElementProxy` since `JsonNode` defines implicit casts from the appropriate types
- `LogExtensions.WrongValueKind()` now takes `SchemaValueType` instead of `JsonValueKind`
- Removed `JsonSchemaExtensions(this JsonSchema, string, ValidationOptions)` since the implicit cast from `JsonNode` was taking precendence

    ***IMPORTANT** Removal of this extension will only manifest at runtime.  Code that called this extension method will still compile, but the validations will most likely fail.*

## Additional Changes

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

# [2.4.0](https://github.com/gregsdennis/json-everything/pull/270)

Added `UnrecognizedKeyword` to represent keywords that were not recognized by any known vocabulary.  The values of these keywords are then captured in the validation results as annotations.  As a result of this change `JsonSchema.OtherData` has been marked obsolete.  

# [2.3.0](https://github.com/gregsdennis/json-everything/pull/249)

[#190](https://github.com/gregsdennis/json-everything/issues/190) - Added support for custom and localized error messages.

Added

- `ErrorMessages` static class which exposes all error message properties and allows for customization.
- Embedded string resources which defines all error messages.

# [2.2.1](https://github.com/gregsdennis/json-everything/pull/247)

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

[#245](https://github.com/gregsdennis/json-everything/issues/245) - [@ItsVeryWindy](https://github.com/ItsVeryWindy) identified where `allOf` was reporting locations incorrectly in validation output.

[#246](https://github.com/gregsdennis/json-everything/issues/246) - [@YaroslavHuriev](https://github.com/YaroslavHuriev) noticed an issue with some formats where non-strings were not properly ignored.  As part of this fix, `Format.Validate(JsonElement)` was marked obsolete as it has been replaced and is no longer called.  It will be removed with the next major version.

# [2.2.0](https://github.com/gregsdennis/json-everything/pull/239)

[#238](https://github.com/gregsdennis/json-everything/pull/238) - [@WeihanLi](https://github.com/WeihanLi) added extenion methods to create schemas directly from `JsonDocument` and strings that contain JSON data.

# [2.1.2](https://github.com/gregsdennis/json-everything/pull/230)

[#220](https://github.com/gregsdennis/json-everything/issues/220) - References to deeply nested subschemas could not be resolved through keywords that contained a single schema.

Resolving the above resulted in not needing `IRefResolvable.ResolvePointerSegment(string)`.  This method has been marked obsolete on the interface and will be removed with the next major version.

# [2.1.1](https://github.com/gregsdennis/json-everything/pull/227)

[#226](https://github.com/gregsdennis/json-everything/issues/226) - Fixed an issue with including unneeded nodes in Basic output.

# [2.1.0](https://github.com/gregsdennis/json-everything/pull/218)

Added `JsonSchema.BoolValue` to easily detect boolean schemas.  Previously required checking `JsonSchema.Keywords` for null.

Fixed an issue with `duration` format parsing.

# [2.0.3](https://github.com/gregsdennis/json-everything/pull/217)

[#216](https://github.com/gregsdennis/json-everything/issues/208) - `additionalProperties` does not determine the correct property set to evaluate for drafts 6 and 7. Reported by [@joyse1](https://github.com/joyse1).

Fixed a bug in keyword filtering for subschemas.

# [2.0.2](https://github.com/gregsdennis/json-everything/pull/213)

[#208](https://github.com/gregsdennis/json-everything/issues/208) - [@ItsVeryWindy](https://github.com/ItsVeryWindy) discovered a regression where resolve local references to definitions located inside a bundled schema fail to resolve.

# [2.0.1](https://github.com/gregsdennis/json-everything/pull/210)

[#207](https://github.com/gregsdennis/json-everything/issues/207) - [@ItsVeryWindy](https://github.com/ItsVeryWindy) discovered a regression where bundling (embedding a schema of one draft inside a schema of another) fails to process the required keyword set.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/200)

Primary change is around how `ValidationContext` is handled throughout the validation process in order to significantly reduce memory footprint.  Thanks to [@dazerdude](https://github.com/dazerdude) for reporting the issue and making suggestions that led to resolving this.

The majority of this change affects how keywords are implemented.  Unless you're implementing your own keywords, it's unlikely that this change will break your code.

The validation context now only contains data required for validation.  The validation result is now set directly on the validation result, which is also now exposed by the context.

## Removed

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


## Changed

- Keywords now _require_ a `[SchemaDraft()]` attribute to declare compatible drafts.
- `Draft` is now a `[Flags]` enum and can be combined with the bitwise OR `|` operator.
- `EnumKeyword` now uses a hash set to store values (improves performance).  This means that it requires unique values.  An `ArgumentException` will now be thrown if attempting to deserialize a schema with duplicate values inside an `enum`.
- `ValidationContext.SchemaRoot` is now read-only.  This is set by the initial call to `.Validate()` and shouldn't need to be set by the client.

## Added

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

<details>
  <summary class="fw-bold fst-italic">1.x versions</summary>

# [1.13.0](https://github.com/gregsdennis/json-everything/pull/204)

[#203](https://github.com/gregsdennis/json-everything/issues/203) - [@coderpatros](https://github.com/coderpatros) discovered a regression in `format` validation when the format is unkonwn.

Added option `OnlyKnownKeywords` to support validation failures on unknown keywords.

***NOTE** Since this is fixing a regression, I'm only bumping the minor version.

# [1.12.0](https://github.com/gregsdennis/json-everything/pull/196)

[#191](https://github.com/gregsdennis/json-everything/issues/191) - Self-referencing meta-schemas cause an infinite loop during validation.

[#193](https://github.com/gregsdennis/json-everything/issues/193) - Custom format validations don't provide sufficient information and don't cause validation to fail when assertion is required.

- Added `UnknownFormat` and `Format.CreateUnknown`.
- Added `JsonSchemaBuilder.Format(string)` which looks up the format key.

# [1.11.6](https://github.com/gregsdennis/json-everything/pull/189)

Fixed bugs highlighted by updating test suite repository.

- Advanced use cases for `$dynamicRef` and `$dynamicAnchor`
- Added `PatternKeyword.InvalidPattern` and `PatternPropertiesKeyword.InvalidPatterns` to handle text that cannot be deserialized as a `RegEx`.  These will now generate validation failures rather than deserialization exceptions.
- Keywords are now properly filtered by their vocabularies if the meta-schema declared by `$schema` includes a `$vocabulary` keyword.
- Added `VocabularyRegistry.Get(Uri)`.

# [1.11.5](https://github.com/gregsdennis/json-everything/pull/179)

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.11.4](https://github.com/gregsdennis/json-everything/pull/176)

[#175](https://github.com/gregsdennis/json-everything/pull/175) - [@ranma42](https://github.com/ranma42) tidied up the schema registry initialization code, also resolving some concurrency issues.

# [1.11.3](https://github.com/gregsdennis/json-everything/pull/166)

[#165](https://github.com/gregsdennis/json-everything/issues/165) - [@pbajrak](https://github.com/pbajrak) discovered a copy/paste error in the message for `minProperties`.

# [1.11.2](https://github.com/gregsdennis/json-everything/pull/157)

Fixed minor issue with `$dynamicRef`/`$dynamicAnchor` and resolution within specific scopes.

# [1.11.1](https://github.com/gregsdennis/json-everything/pull/156)

Some `format` validations were incorrectly failing validations for incompatible types.  For example, `uri` would fail on integers, but should pass (ignore non-strings).

# [1.11.0](https://github.com/gregsdennis/json-everything/pull/155)

`format` incorporates error messages from custom format validation, when provided.

# [1.10.8](https://github.com/gregsdennis/json-everything/pull/152)

`format` now adds an error message.

# [1.10.7](https://github.com/gregsdennis/json-everything/pull/145)

Revenge of [#138](https://github.com/gregsdennis/json-everything/issues/138) - [@yohny](https://github.com/yohny) found that the initialization issue still existed when not supplying custom validation options.

# [1.10.6](https://github.com/gregsdennis/json-everything/pull/140)

[#138](https://github.com/gregsdennis/json-everything/issues/138) - [@wazzamatazz](https://github.com/wazzamatazz) found an initialization issue that resulted in a `NullReferenceException`.

# [1.10.5](https://github.com/gregsdennis/json-everything/pull/135)

Fixed an issue with reference resolution when `$ref` and `$id` are sibling keywords in drafts 6 and 7.

# [1.10.4](https://github.com/gregsdennis/json-everything/pull/133)

[#132](https://github.com/gregsdennis/json-everything/pull/132) - Fixed some memory management issues around `JsonDocument` and `JsonElement`.  Thanks to [@ddunkin](https://github.com/ddunkin) for finding and fixing these.

# [1.10.3](https://github.com/gregsdennis/json-everything/pull/120)

Added overload for `ExitKeyword` logging extension to pull the validition result from the context rather than having to pass it in.

# [1.10.2](https://github.com/gregsdennis/json-everything/pull/102)

In 2020-12, `unevaluatedItems` should consider `contains`.

# [1.10.1](https://github.com/gregsdennis/json-everything/pull/98)

[#97](https://github.com/gregsdennis/json-everything/issues/97) - Added support for circular reference detection.

# [1.10.0](https://github.com/gregsdennis/json-everything/pull/93)

[#90](https://github.com/gregsdennis/json-everything/issues/90) - Added `JsonSchemaBuilder.Get<T>()` to support the discussion around incrementally adding properties while building schemas.

Added optional logging around schema processing decisions.

# [1.9.4](https://github.com/gregsdennis/json-everything/pull/92)

Register the 2020-12 meta-schemas and vocabularies.

Fixed several issues with reusing options objects between validations.

Fixed `$dynamic*` keywords based on new tests in test suite.

# [1.9.3](https://github.com/gregsdennis/json-everything/pull/83)

[#82](https://github.com/gregsdennis/json-everything/pull/82) - Fixing issues managing `ValidationContext.CurrentUri`.  Credit to [@amosonn](https://github.com/amosonn).

# [1.9.2](https://github.com/gregsdennis/json-everything/pull/80)

[#79](https://github.com/gregsdennis/json-everything/issues/79) - `$ref` was calling out to resolve a parent URI.

# [1.9.1](https://github.com/gregsdennis/json-everything/pull/77)

Fixes found by the release of the 2020-12 test suite.

- `$dynamicAnchor` and `$dynamicRef` behavior.
- make `items` and `unevaluatedItems` respect annotations from `prefixItems`.

# [1.9.0](https://github.com/gregsdennis/json-everything/pull/75)

Added support for nullable reference types.

Fixed various evaluation bugs around the various keywords involved with references.

Fixed `content*` keywords so that they only validate if the instance is a string.

Fixed [#76](https://github.com/gregsdennis/json-everything/issues/76) - `.GetHashCode()` is inconsistent for schemas and keywords.  Credit to [@amosonn](https://github.com/amosonn) for pointing me to a good hash code method in the wild.

`RecursiveRef` parameterless constructor marked obsolete in favor of new constructor which takes a boolean value.  Schema builder extension also updated to include a parameter which defaults to `true` to maintain current behavior.

# [1.8.0](https://github.com/gregsdennis/json-everything/pull/72)

Exposed `UriExtensions` to help support new keywords that manipulate URIs.

# [1.7.1](https://github.com/gregsdennis/json-everything/pull/68)

Fixed [#67](https://github.com/gregsdennis/json-everything/issues/67) - `enum` keyword serializes as `examples` due to a copy-paste error.  Credit to [@dannyheard7](https://github.com/dannyheard7) for finding and fixing.  Also fixed several other related serialization issues.

# [1.7.0](https://github.com/gregsdennis/json-everything/pull/65)

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

# [1.6.1](https://github.com/gregsdennis/json-everything/pull/61)

Signed the DLL for strong name compatibility.

# [1.6.0](https://github.com/gregsdennis/json-everything/pull/52)

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

# [1.5.4](https://github.com/gregsdennis/json-everything/pull/45)

Added debug symbols to package.  No functional change.

# [1.5.3](https://github.com/gregsdennis/json-everything/pull/41)

Fixed `NullReferenceException` when comparing two schemas.

# [1.5.2](https://github.com/gregsdennis/json-everything/pull/40)

Updated wording for `enum` error message.  This must remain generic and cannot include the expected values because the list could be long and the values could be quite large.

# [1.5.1](https://github.com/gregsdennis/json-everything/pull/34)

[#35](https://github.com/gregsdennis/json-everything/issues/35) `JsonSchema.FromFile()` handles file paths as URIs incorrectly in non-Windows systems. 

# [1.5.0](https://github.com/gregsdennis/json-everything/pull/34)

[#33](https://github.com/gregsdennis/json-everything/issues/33) Added `ValidationOptions.ValidateFormat` which allows configuration of whether to validate the `format` keyword.  Also fixes a bug where the `format` keyword was validated by default for draft 2019-09 which specifies that it should only generate annotations by default.  Because this library favors the latest draft, this is the default behavior for all drafts.

As a further followup to #27 (below), basic output has been refined.

# [1.4.0](https://github.com/gregsdennis/json-everything/pull/31)

[#27](https://github.com/gregsdennis/json-everything/issues/27) (reopened) Better reduction of detailed output format which eliminates the notion that any nodes *must* be kept.

[#29](https://github.com/gregsdennis/json-everything/issues/29) Relative `$id` keyword at root of schema was not supported.  Added `ValidationOptions.DefaultBaseUri` to be used when no other absolute URI is defined by the `$id` keyword.  Also now supports assuming the base URI from the file name.

# [1.3.1](https://github.com/gregsdennis/json-everything/pull/28)

[#27](https://github.com/gregsdennis/json-everything/issues/27) Nodes in the basic and detailed output formats that match the overall outcome should be removed.  This also addresses several other bugs involving the output such as `absoluteKeywordLocation`.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/25)

[#15](https://github.com/gregsdennis/json-everything/issues/15) Easier navigation of the schema and its subschemas. Added `ISchemaContainer`, `ISchemaCollector`, and `IKeyedSchemaCollector` for the varying sets of subschemas that keywords can have.  Added `SchemaKeywordExtensions.GetSubschemas()` extension method.

[#19](https://github.com/gregsdennis/json-everything/issues/19) Keyword filtering doesn't consider declared draft or `ValidationOptions.ValidateAs`.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/17)

([json-schema<nsp>.org #358](https://github.com/json-schema-org/json-schema-org.github.io/pull/358)) Published draft 06 meta-schema doesn't match the copy in the spec repo.

[#16](https://github.com/gregsdennis/json-everything/issues/16) `JsonSchema` equality checking.  Along with this, added `IEquatable<T>` to `SchemaKeywordRegistry.Register<T>()`.

[#18](https://github.com/gregsdennis/json-everything/issues/18) `properties` keyword is processed with same priority as `additionalProperties` making keyword order important, but it shouldn't be.

Added `EnumerableExtensions.ContentsEqual()`.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/11)

Added `SchemaRegistry.Fetch` property to enable automatic downloading of referenced schemas.

# [1.0.3](https://github.com/gregsdennis/json-everything/pull/11)

[#9](https://github.com/gregsdennis/json-everything/pull/11) `if`/`then`/`else` are processed in serialized order instead of processing `if` first.

[#10](https://github.com/gregsdennis/json-everything/pull/10) Bug fix around deserialization of `readonly` keyword.

# [1.0.2](https://github.com/gregsdennis/json-everything/pull/7)

Updated format `json-pointer` to require plain pointers.  URI-encoded pointers are invalid.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/6)

Updated validation of formats `hostname`, `iri`, `uri`, `regex`, and `time`.

Fixed issue resolving references (`$ref` & `$recursiveRef`) to miscellaneous (non-keyword) schema data.

# 1.0.0

Initial release.

</details>