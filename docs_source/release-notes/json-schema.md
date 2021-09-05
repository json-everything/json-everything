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
