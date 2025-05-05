---
layout: page
title: JsonSchema.Net.Generation
icon: fas fa-tag
order: "09.05"
---
# [5.0.2](https://github.com/json-everything/json-everything/pull/887) {#release-schemagen-5.0.2}

Rerelease of 5.0.1 with fixed package dependencies.  Thanks to [@arturcic](https://github.com/arturcic) for reporting.

# [5.0.1](https://github.com/json-everything/json-everything/pull/887) {#release-schemagen-5.0.1}

Fixed missing `$defs` in the produced JSON schema if `SchemaGeneratorConfiguration.RegisterXmlCommentFile()` is used on a type which has a `<summary>` xmldoc.

# [5.0.0](https://github.com/gregsdennis/json-everything/pull/822) {#release-schemagen-5.0.0}

API update which addresses numerous design issues.

- Revamped nullability so that the code is respected and `[Nullable]` is merely an override rather than the primary specifier of behavior.
- Optimization can no longer be disabled, and has been updated to only collect types instead of common schemas.  `$ref`s are minimized.
- Added explicit configuration to assign an attribute to a generic parameter.
- Support for C# `required` keyword.

## Breaking changes

- `IContextContainer` removed as it only supported optimization, which is now performed differently.
  - `.Replace()` removed from `AdditionalItemsIntent`, `AdditionaPropertiesIntent`, `ItemsIntent`, `PropertiesIntent`, `PropertyNamesIntent`, and `UnevaluatedPropertiesIntent`. 
- `Nullability` enum removed.
- `[Nullable]` attribute can no longer be applied to types; now only fields and properties.
- `SchemaGenerationContextBase`:
  - `.Type` is now abstract.
  - `.Hash` removed.
  - `.ReferenceCount` moved to `TypeGenerationContext`.
  - `.GenerateIntents()` is now abstract.
- `MemberGenerationContext.BasedOn` is now explicitly a `TypeGenerationContext`.
- `SchemaGenerationContextCache.Get()` now explicitly returns a `TypeGenerationContext`.
- `SchemaGenerationContextOptimizer` removed.
  - `.TypeNameGenerator` static property moved to `ScheamGenerationConfiguration` as instance property.
- `SchemaGeneratorConfiguration`:
  - `.Nullability` removed.
  - `.Optimize` removed.

## Additions

- `INestableAttribute` interface which allows specifying which generic parameter the attribute applies to.
  - Most built-in attributes support this interface.
- `Context` property now settable on `AdditionalPropertiesIntent`, `ItemsIntent`, and `UnevaluatedPropertiesIntent`.    
- `MemberGenerationContext.NullableRef` which indicates that the member is declared as a nullable reference type.
- `MemberGenerationContext.Parameter` which indicates the type nesting level ("nesting" as in how a generic parameter is nested).
- C# `required` keyword will now add the property to a `required` JSON Schema keyword.

## Other changes

# [4.6.0](https://github.com/gregsdennis/json-everything/pull/822) {#release-schemagen-4.6.0}

Add .Net 9.0 support.

# [4.5.1](https://github.com/gregsdennis/json-everything/pull/796) {#release-schemagen-4.5.1}

[#795](https://github.com/gregsdennis/json-everything/issues/795) - Added support for `[JsonExclude]` and `[JsonIgnore]` (_System.Text.Json_) for enum members.  Thanks to [@2BitSalute](https://github.com/2BitSalute) for suggesting and implementing the idea.

# [4.5.0](https://github.com/gregsdennis/json-everything/pull/772) {#release-schemagen-4.5.0}

[#768](https://github.com/gregsdennis/json-everything/issues/768) - Added two ways to allow external references.  Thanks to [@luisjones](https://github.com/luisjones) for suggesting the idea.

- Added `[Id]` attribute
- Added `SchemaGeneratorConfiguration.ExternalReferences`

# [4.4.0](https://github.com/gregsdennis/json-everything/pull/770) {#release-schemagen-4.4.0}

Updated to use _JsonSchema.Net_ v7.0.0, which contains breaking changes ([release notes](/rn-json-schema/#release-schema-7.0.0)).

[#767](https://github.com/gregsdennis/json-everything/issues/767) - Missing `description` keyword from XML comments on multi-nested types.  Thanks to [@parnic-sks](https://github.com/parnic-sks) for reporting this.

# [4.3.0.x](https://github.com/gregsdennis/json-everything/pull/712) {#release-schemagen-4.3.0.x}

[#711](https://github.com/gregsdennis/json-everything/issues/711) - Nuget package meta-data updates; no functional changes from previous version.

# [4.3.0](https://github.com/gregsdennis/json-everything/pull/698) {#release-schemagen-4.3.0}

- Added `EnumIntent` constructors that take `JsonNodes`.
- Added `NotIntent`.

# [4.2.0](https://github.com/gregsdennis/json-everything/pull/698) {#release-schemagen-4.2.0}

[#696](https://github.com/gregsdennis/json-everything/issues/696) - Number-based constraints ignored for nullable number types.  Thanks to [@rhwork](https://github.com/rhwork) for reporting this.

The following have been updated:

- `[Minimum]`
- `[ExclusiveMinimum]`
- `[Maximum]`
- `[ExclusiveMaximum]`
- `[IfMin]`
- `[IfMax]`

Added extension method `bool IsNullableNumber(this Type)`.

# [4.1.1](https://github.com/gregsdennis/json-everything/pull/670)

Add support for multiple `[If]` attributes for the same property under the same group creating an `enum` keyword with all of the values.

# [4.1.0](https://github.com/gregsdennis/json-everything/pull/670) {#release-schemagen-4.1.0}

[#667](https://github.com/gregsdennis/json-everything/issues/667) - Add support for `description` keyword creation from XML comments `<Summary>`.  Thanks to [@zlepper](https://github.com/zlepper) for applying pressure on this feature.

# [4.0.0](https://github.com/gregsdennis/json-everything/pull/619) {#release-schemagen-4.0.0}

Updated for multi-framework support with .Net Standard 2.0 and .Net 8, including updating _System.Text.Json_ from v6 to v8.

AOT support is not currently supported for this library.  Appropriate attributes have been added to public API.

## Breaking changes

- Dropping support for .Net Standard 3.1 - May still be used, but exact behavior cannot be guaranteed. 
- `PropertyNamingMethods` removed.  Previously marked obsolete.
- `SchemaGeneratorConfiguration.PropertyNamingMethod` removed.  Previously marked obsolete.

# [3.5.0](https://github.com/gregsdennis/json-everything/pull/581) {#release-schemagen-3.5.0}

[#579](https://github.com/gregsdennis/json-everything/issues/579) - Types which contained properties with both nullable and non-nullable versions of the same value type would attribute all properties as nullable.  Thanks to [@tjochums](https://github.com/tjochums) for reporting.

Also fixed a hash code collision issue with property ordering.

# [3.5.0](https://github.com/gregsdennis/json-everything/pull/557) {#release-schemagen-3.5.0}

[#548](https://github.com/gregsdennis/json-everything/issues/551) - Add `[Default]` attribute to generate `default` keyword.

# [3.4.2](https://github.com/gregsdennis/json-everything/pull/553) {#release-schemagen-3.4.2}

[#551](https://github.com/gregsdennis/json-everything/issues/551) - Strings implement `IEnumerable<T>` and thus were considered arrays.  As a result `minItems` and `maxItems` were being applied.  Thanks to [@domchlouba](https://github.com/domchlouba) for reporting this and recommending the fix.

# [3.4.1](https://github.com/gregsdennis/json-everything/pull/546) {#release-schemagen-3.4.1}

[#544](https://github.com/gregsdennis/json-everything/issues/544) - Objects that have two properties of the same type, one of which is marked with an `[Obsolete]` attribute will cause all properties to have a `deprecated: true` added to their schemas.  Thanks to [@filzrev](https://github.com/filzrev) for reporting this.

Also affected by this were `[JsonPropertyName]` and `[JsonIgnore]`.

# [3.4.0](https://github.com/gregsdennis/json-everything/pull/529) {#release-schemagen-3.4.0}

[#528](https://github.com/gregsdennis/json-everything/pull/528) - [@juwens](https://github.com/juwens) suggesting adding `PropertyNameResolver` for property naming that takes the entire `MemberInfo` (instead of just the member name) in place of `PropertyNamingMethod` so that additional information can be used to determine a name for the JSON property.

`SchemaGeneratorConfiguration.PropertyNamingMethod` has been deprecated.  It's still functional but will be removed with the next major version.

# [3.3.2](https://github.com/gregsdennis/json-everything/pull/513) {#release-schemagen-3.3.2}

[#512](https://github.com/gregsdennis/json-everything/issues/512) - There was no support for `JsonNode`, explicitly.  Instead it would attempt to generate via reflection.  This update adds that support and just generates a `true` schema.  Thanks to [@ashek-simeon](https://github.com/ashek-simeon) for reporting and recommending a solution.

# [3.3.1](https://github.com/gregsdennis/json-everything/pull/491) {#release-schemagen-3.3.1}

[#488](https://github.com/gregsdennis/json-everything/issues/488) - `[Maximum(double.MaxValue)]` throws an overflow exception when attempting to convert to decimal.  This fix updates all min/max-related attributes.

# [3.3.0](https://github.com/gregsdennis/json-everything/pull/466) {#release-schemagen-3.3.0}

Added conditional schema generation using new attributes to define condition groups and then assigning other attributes to those groups.

# [3.2.0](https://github.com/gregsdennis/json-everything/pull/456) {#release-schemagen-3.2.0}

[#455](https://github.com/gregsdennis/json-everything/issues/455) - Better naming for `$defs` keys.

Also added `ITypeNameGenerator` and `SchemaGenerationContextOptimizer.TypeNameGenerator` to allow custom naming.

# [3.1.1](https://github.com/gregsdennis/json-everything/pull/454) {#release-schemagen-3.1.1}

[#450](https://github.com/gregsdennis/json-everything/issues/450) - Fixed an edge case issue where a recursive class structure defining the same property type caused a stack overflow when the outer class property was decorated with `[Nullable]`.

# 3.1.0 (no PR) {#release-schemagen-3.1.0}

Updated JsonSchema.Net reference to v4.0.0.

# [3.0.3](https://github.com/gregsdennis/json-everything/pull/327) {#release-schemagen-3.0.3}

[#325](https://github.com/gregsdennis/json-everything/issues/325) - [@jnystad](https://github.com/jnystad) found an issue with `[Nullable]` where it was propagating to other properties of the same type and suggested a fix.

# [3.0.2](https://github.com/gregsdennis/json-everything/pull/301) {#release-schemagen-3.0.2}

[#296](https://github.com/gregsdennis/json-everything/pull/296) - Updated handling logic for `[JsonIgnore]` to consider the attribute's `Condition` property.

# 3.0.1 (no PR) {#release-schemagen-3.0.1}

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [3.0.0](https://github.com/gregsdennis/json-everything/pull/280) {#release-schemagen-3.0.0}

Updated all functionality to use `JsonNode` instead of `JsonElement` in line with JsonSchema.Net v3.

## Breaking Changes {#release-schemagen-3.0.0-breaks}

_`JsonElement` -> `JsonNode` type exchange changes not listed._

- `IContextContainer.GetContexts()` and implementations removed as it was obsolete
- `TypeExtensions.IsInteger()`, `.IsFloatingPoint()`, and `.IsNumber()` moved to Json.More.Net

# [2.1.1](https://github.com/gregsdennis/json-everything/pull/275) {#release-schemagen-2.1.1}

[#274](https://github.com/gregsdennis/json-everything/issues/274) - Added support for `JsonObject`, `JsonArray`, and `JsonValue`.

# [2.2.0](https://github.com/gregsdennis/json-everything/pull/273) {#release-schemagen-2.2.0}

[#272](https://github.com/gregsdennis/json-everything/issues/272) - Miscellaneous attributes can cause incorrect optimizations.

Added `SchemaGeneratorConfiguration.Optimize` to control whether common subschemas are collected into `$defs`.

Exposed `SchemaGeneratorConfiguration.Current` static property for read use by custom generators, etc.

# [2.1.0](https://github.com/gregsdennis/json-everything/pull/267) {#release-schemagen-2.1.0}

[#264](https://github.com/gregsdennis/json-everything/issues/264) - Added support for `additionalProperties` and `additionalItems`.

Improved support for array types that inherit `IEnumerable<T>` but aren't generic themselves, e.g. `MyList : List<int>`.

Also updated return value of `SchemaGenerationContextBase.Apply()` from `JsonSchemaBuilder` to `JsonSchema`.  This is pretty hidden, so it _shouldn't_ break anyone, thus not strictly adhering to semver for this change.  Calling it out just in case.

# 2.0.1 (No PR) {#release-schemagen-2.0.1}

Exposed `SchemaGenerationContextBase.Hash` property.  Required to implement `IContextContainer.Replace()`.

Marked `IContextContainer.GetContexts()` obsolete as it's not used.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/261) {#release-schemagen-2.0.0}

[#202](https://github.com/gregsdennis/json-everything/issues/202) - Added support for read-only & write-only keywords for properties that only have getters or setters.  The attributes override this detection behavior.

[#256](https://github.com/gregsdennis/json-everything/pull/256) - Added support for type-level attributes.

## Breaking changes {#release-schemagen-2.0.0-breaks}

- Intents no longer require equality methods (`Equals()` & `GetHashCode()`). 🎉
- `SchemaGeneratorContext` replaced by `SchemaGenerationContextBase`, `TypeGenerationContext`, and `MemberGenerationContext`.
    - The "Type" version represents generation of a "pure" type; that is, it's not a member of another type.  This will only handle attributes that are placed on the type itself.
    - The "Member" version represents object members.  This will handle attributes placed on the member.  The `Attributes` property is only present on this version since the type carries the attributes for the other.
- `SchemaGenerationContextCache.Get()` has two overloads, only with member attributes, and one without.  If attributes are supplied, a `MemberGenerationContext` will be returned; otherwise a `TypeGenerationContext` will be returned.
- `SchemaGeneratorConfiguration` converted into a thread-static singleton (the static property is `Current`) for easier access.
- `IAttributeHandler` is now just a placeholder.  Attribute handlers will now need to implement `IAttributeHandler<T>` where `T` is the attribute type.
- `IAttributeHandler.AddConstraints()` now has an `attribute` parameter.

## Other fixes & enhancements {#release-schemagen-2.0.0-other}

[#206](https://github.com/gregsdennis/json-everything/issues/206) - Some XML comments were inaccurate or had copy/paste errors.

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

Most attributes can now be placed on:
- properties & fields
- all types (class, struct, enum, interface)

# [1.8.3](https://github.com/gregsdennis/json-everything/pull/200) {#release-schemagen-1.8.3}

Updated JsonSchema.Net reference to v2.0.0.  Please see [release notes](./json-schema.md) for that library as it contains breaking changes.

# [1.8.2](https://github.com/gregsdennis/json-everything/pull/182) {#release-schemagen-1.8.2}

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.8.1](https://github.com/gregsdennis/json-everything/pull/176) {#release-schemagen-1.8.1}

[#174](https://github.com/gregsdennis/json-everything/issues/174) - Switch top-level Humanizer package for more essential Humanizer.Core.  Supports English.  If other languages are required, the respective Humanizer package will need to be added to your application.

# [1.8.0](https://github.com/gregsdennis/json-everything/pull/144) {#release-schemagen-1.8.0}

[#141](https://github.com/gregsdennis/json-everything/issues/141) - Added `[Title()]` and `[Description()]` attributes.

Fixed `[Minimum()]`, `[ExclusiveMinimum()]`, `[Maximum()]`, and `[ExclusiveMaximum()]` attributes to take `double` instead of `uint` which was a copy/paste error.

# [1.7.1](https://github.com/gregsdennis/json-everything/pull/137) {#release-schemagen-1.7.1}

[#136](https://github.com/gregsdennis/json-everything/pull/136) - Better sorting for properties, taking into account inheritance hierarchy and putting inherited properties first.  Thanks to [@wazzamatazz](https://github.com/wazzamatazz) for working this up.

# [1.7.0](https://github.com/gregsdennis/json-everything/pull/135) {#release-schemagen-1.7.0}

[#134](https://github.com/gregsdennis/json-everything/issues/134) - Added custom property naming support.

# [1.6.0](https://github.com/gregsdennis/json-everything/pull/130) {#release-schemagen-1.6.0}

[#128](https://github.com/gregsdennis/json-everything/pull/128) - Added support for nullability.  Special thanks to [@JMPSequeira](https://github.com/JMPSequeira) who proposed the feature and worked through multiple designs ([#124](https://github.com/gregsdennis/json-everything/pull/124) and [#127](https://github.com/gregsdennis/json-everything/pull/127)) that lead to the final solution and for adding all the tests needed to ensure proper function.
- Added `NullableAttribute`.
- Added `Nullability`.
- Added `SchemaGeneratorConfiguration.Nullability`.

[#125](https://github.com/gregsdennis/json-everything/pull/125) - Bug fix: generation of object with no serializable properties still generates an empty `properties` keyword.

Also updated intent classes to be mutable.  Should have been this way to begin with, but was probably an oversight.

# [1.5.0](https://github.com/gregsdennis/json-everything/pull/115) {#release-schemagen-1.5.0}

[#114](https://github.com/gregsdennis/json-everything/issues/114) - `DateTime` properties not generating properly.  Also fixes `Guid`, `JsonPointer`, and  `Uri` properties.

Added `PropertyNamesIntent`.

# [1.4.0](https://github.com/gregsdennis/json-everything/pull/113) {#release-schemagen-1.4.0}

[#109](https://github.com/gregsdennis/json-everything/issues/109) - Add properties in the order they're declared in code.  Credit to [@wazzamatazz](https://github.com/jaysvoboda)

[#112](https://github.com/gregsdennis/json-everything/issues/112) - Add properties in the order they're declared in code.

# [1.3.2](https://github.com/gregsdennis/json-everything/pull/105) {#release-schemagen-1.3.2}

Fixes property name callout in `required` keyword.  Credit to [@jaysvoboda](https://github.com/jaysvoboda) for finding and fixing this.

# [1.3.1](https://github.com/gregsdennis/json-everything/pull/101) {#release-schemagen-1.3.1}

[#100](https://github.com/gregsdennis/json-everything/issues/100) - Don't optimize type-only subschemas.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/98) {#release-schemagen-1.3.0}

[#96](https://github.com/gregsdennis/json-everything/issues/96) - Added `ISchemaRefiner` to the schema generation process to allow users to inject custom logic after intents are created.

[#97](https://github.com/gregsdennis/json-everything/issues/97) - Fixed an issue with subsequent schema generations off of the same type.

# [1.2.3](https://github.com/gregsdennis/json-everything/pull/88) {#release-schemagen-1.2.3}

[#87](https://github.com/gregsdennis/json-everything/issues/87) - Generation for the same property type under different property names causes StackOverflowException.

# [1.2.2](https://github.com/gregsdennis/json-everything/pull/86) {#release-schemagen-1.2.2}

[#85](https://github.com/gregsdennis/json-everything/issues/85) - Generation for `List<TSelf>` producing StackOverflowException.

# [1.2.1](https://github.com/gregsdennis/json-everything/pull/75) {#release-schemagen-1.2.1}

Added support for nullable reference types.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/74) {#release-schemagen-1.2.0}

Added support for .Net 5 and remaining in-built JSON serialization attributes: `JsonInclude` & `JsonNumberHandling`.

Exposed static class `AttributeHandler` and added methods to allow for handlers of predefined attributes.  This mechanism should only be used for attributes that cannot also be made to implement `IAttributeHandler`.  Attributes which also implement this interface are implicitly handled.

# [1.1.1](https://github.com/gregsdennis/json-everything/pull/61) {#release-schemagen-1.1.1}

Signed the DLL for strong name compatibility.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/49) {#release-schemagen-1.1.0}

Added support for in-built JSON serialization attributes (Standard 2.0 & Core 3.x): `JsonPropertyName` & `JsonIgnore`.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/45) {#release-schemagen-1.0.1}

Added debug symbols to package.  No functional change.

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/41) {#release-schemagen-1.0.0}

Initial release.
