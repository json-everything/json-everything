# [3.0.2](https://github.com/gregsdennis/json-everything/pull/301)

[#296](https://github.com/gregsdennis/json-everything/pull/296) - Updated handling logic for `[JsonIgnore]` to consider the attribute's `Condition` property.

# 3.0.1 (no PR)

[#288](https://github.com/gregsdennis/json-everything/issues/288) - Just bumping version to pick up the latest Json.More.Net by default.  This package pull Json.More.Net transitively via JsonPointer.Net which wasn't updated with the move to `JsonNode`.

# [3.0.0](https://github.com/gregsdennis/json-everything/pull/280)

Updated all functionality to use `JsonNode` instead of `JsonElement` in line with JsonSchema.Net v3.

## Breaking Changes

_`JsonElement` -> `JsonNode` type exchange changes not listed._

- `IContextContainer.GetContexts()` and implementations removed as it was obsolete
- `TypeExtensions.IsInteger()`, `.IsFloatingPoint()`, and `.IsNumber()` moved to Json.More.Net

# [2.1.1](https://github.com/gregsdennis/json-everything/pull/275)

[#274](https://github.com/gregsdennis/json-everything/issues/274) - Added support for `JsonObject`, `JsonArray`, and `JsonValue`.

# [2.2.0](https://github.com/gregsdennis/json-everything/pull/273)

[#272](https://github.com/gregsdennis/json-everything/issues/272) - Miscellaneous attributes can cause incorrect optimizations.

Added `SchemaGeneratorConfiguration.Optimize` to control whether common subschemas are collected into `$defs`.

Exposed `SchemaGeneratorConfiguration.Current` static property for read use by custom generators, etc.

# [2.1.0](https://github.com/gregsdennis/json-everything/pull/267)

[#264](https://github.com/gregsdennis/json-everything/issues/264) - Added support for `additionalProperties` and `additionalItems`.

Improved support for array types that inherit `IEnumerable<T>` but aren't generic themselves, e.g. `MyList : List<int>`.

Also updated return value of `SchemaGenerationContextBase.Apply()` from `JsonSchemaBuilder` to `JsonSchema`.  This is pretty hidden, so it _shouldn't_ break anyone, thus not strictly adhering to semver for this change.  Calling it out just in case.

# 2.0.1 (No PR)

Exposed `SchemaGenerationContextBase.Hash` property.  Required to implement `IContextContainer.Replace()`.

Marked `IContextContainer.GetContexts()` obsolete as it's not used.

# [2.0.0](https://github.com/gregsdennis/json-everything/pull/261)

[#202](https://github.com/gregsdennis/json-everything/issues/202) - Added support for read-only & write-only keywords for properties that only have getters or setters.  The attributes override this detection behavior.

[#256](https://github.com/gregsdennis/json-everything/pull/256) - Added support for type-level attributes.

## Breaking changes

- Intents no longer require equality methods (`Equals()` & `GetHashCode()`). 🎉
- `SchemaGeneratorContext` replaced by `SchemaGenerationContextBase`, `TypeGenerationContext`, and `MemberGenerationContext`.
    - The "Type" version represents generation of a "pure" type; that is, it's not a member of another type.  This will only handle attributes that are placed on the type itself.
    - The "Member" version represents object members.  This will handle attributes placed on the member.  The `Attributes` property is only present on this version since the type carries the attributes for the other.
- `SchemaGenerationContextCache.Get()` has two overloads, only with member attributes, and one without.  If attributes are supplied, a `MemberGenerationContext` will be returned; otherwise a `TypeGenerationContext` will be returned.
- `SchemaGeneratorConfiguration` converted into a thread-static singleton (the static property is `Current`) for easier access.
- `IAttributeHandler` is now just a placeholder.  Attribute handlers will now need to implement `IAttributeHandler<T>` where `T` is the attribute type.
- `IAttributeHandler.AddConstraints()` now has an `attribute` parameter.

## Other fixes & enhancements

[#206](https://github.com/gregsdennis/json-everything/issues/206) - Some XML comments were inaccurate or had copy/paste errors.

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

Most attributes can now be placed on:
- properties & fields
- all types (class, struct, enum, interface)

# [1.8.3](https://github.com/gregsdennis/json-everything/pull/200)

Updated JsonSchema.Net reference to v2.0.0.  Please see [release notes](./json-schema.md) for that library as it contains breaking changes.

# [1.8.2](https://github.com/gregsdennis/json-everything/pull/182)

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [1.8.1](https://github.com/gregsdennis/json-everything/pull/176)

[#174](https://github.com/gregsdennis/json-everything/issues/174) - Switch top-level Humanizer package for more essential Humanizer.Core.  Supports English.  If other languages are required, the respective Humanizer package will need to be added to your application.

# [1.8.0](https://github.com/gregsdennis/json-everything/pull/144)

[#141](https://github.com/gregsdennis/json-everything/issues/141) - Added `[Title()]` and `[Description()]` attributes.

Fixed `[Minimum()]`, `[ExclusiveMinimum()]`, `[Maximum()]`, and `[ExclusiveMaximum()]` attributes to take `double` instead of `uint` which was a copy/paste error.

# [1.7.1](https://github.com/gregsdennis/json-everything/pull/137)

[#136](https://github.com/gregsdennis/json-everything/pull/136) - Better sorting for properties, taking into account inheritance hierarchy and putting inherited properties first.  Thanks to [@wazzamatazz](https://github.com/wazzamatazz) for working this up.

# [1.7.0](https://github.com/gregsdennis/json-everything/pull/135)

[#134](https://github.com/gregsdennis/json-everything/issues/134) - Added custom property naming support.

# [1.6.0](https://github.com/gregsdennis/json-everything/pull/130)

[#128](https://github.com/gregsdennis/json-everything/pull/128) - Added support for nullability.  Special thanks to [@JMPSequeira](https://github.com/JMPSequeira) who proposed the feature and worked through multiple designs ([#124](https://github.com/gregsdennis/json-everything/pull/124) and [#127](https://github.com/gregsdennis/json-everything/pull/127)) that lead to the final solution and for adding all the tests needed to ensure proper function.
- Added `NullableAttribute`.
- Added `Nullability`.
- Added `SchemaGenerationConfiguration.Nullability`.

[#125](https://github.com/gregsdennis/json-everything/pull/125) - Bug fix: generation of object with no serializable properties still generates an empty `properties` keyword.

Also updated intent classes to be mutable.  Should have been this way to begin with, but was probably an oversight.

# [1.5.0](https://github.com/gregsdennis/json-everything/pull/115)

[#114](https://github.com/gregsdennis/json-everything/issues/114) - `DateTime` properties not generating properly.  Also fixes `Guid`, `JsonPointer`, and  `Uri` properties.

Added `PropertyNamesIntent`.

# [1.4.0](https://github.com/gregsdennis/json-everything/pull/113)

[#109](https://github.com/gregsdennis/json-everything/issues/109) - Add properties in the order they're declared in code.  Credit to [@wazzamatazz](https://github.com/jaysvoboda)

[#112](https://github.com/gregsdennis/json-everything/issues/112) - Add properties in the order they're declared in code.

# [1.3.2](https://github.com/gregsdennis/json-everything/pull/105)

Fixes property name callout in `required` keyword.  Credit to [@jaysvoboda](https://github.com/jaysvoboda) for finding and fixing this.

# [1.3.1](https://github.com/gregsdennis/json-everything/pull/101)

[#100](https://github.com/gregsdennis/json-everything/issues/100) - Don't optimize type-only subschemas.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/98)

[#96](https://github.com/gregsdennis/json-everything/issues/96) - Added `ISchemaRefiner` to the schema generation process to allow users to inject custom logic after intents are created.

[#97](https://github.com/gregsdennis/json-everything/issues/97) - Fixed an issue with subsequent schema generations off of the same type.

# [1.2.3](https://github.com/gregsdennis/json-everything/pull/88)

[#87](https://github.com/gregsdennis/json-everything/issues/87) - Generation for the same property type under different property names causes StackOverflowException.

# [1.2.2](https://github.com/gregsdennis/json-everything/pull/86)

[#85](https://github.com/gregsdennis/json-everything/issues/85) - Generation for `List<TSelf>` producing StackOverflowException.

# [1.2.1](https://github.com/gregsdennis/json-everything/pull/75)

Added support for nullable reference types.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/74)

Added support for .Net 5 and remaining in-built JSON serialization attributes: `JsonInclude` & `JsonNumberHandling`.

Exposed static class `AttributeHandler` and added methods to allow for handlers of predefined attributes.  This mechanism should only be used for attributes that cannot also be made to implement `IAttributeHandler`.  Attributes which also implement this interface are implicitly handled.

# [1.1.1](https://github.com/gregsdennis/json-everything/pull/61)

Signed the DLL for strong name compatibility.

# [1.1.0](https://github.com/gregsdennis/json-everything/pull/49)

Added support for in-built JSON serialization attributes (Standard 2.0 & Core 3.x): `JsonPropertyName` & `JsonIgnore`.

# [1.0.1](https://github.com/gregsdennis/json-everything/pull/45)

Added debug symbols to package.  No functional change.

# [1.0.0](https://github.com/gregsdennis/json-everything/pull/41)

Initial release.