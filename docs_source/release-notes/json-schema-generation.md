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