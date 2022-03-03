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
