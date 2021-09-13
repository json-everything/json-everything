# [1.4.4](https://github.com/gregsdennis/json-everything/pull/163)

Added special case for `.ToJsonDocument()` that first checks to see if the value is already a `JsonDocument`.

# [1.4.3](https://github.com/gregsdennis/json-everything/pull/133)

[#132](https://github.com/gregsdennis/json-everything/pull/132) - Fixed some memory management issues around `JsonDocument` and `JsonElement`.  Thanks to [@ddunkin](https://github.com/ddunkin) for finding and fixing these.

# [1.4.2](https://github.com/gregsdennis/json-everything/pull/105)

Fixes potential race condition in `EnumStringConverter`.  Credit to [@jaysvoboda](https://github.com/jaysvoboda) for finding and fixing this.

# [1.4.1](https://github.com/gregsdennis/json-everything/pull/78)

`JsonElementEqualityComparer` now uses `.GetEquivalenceHashCode()`.

# [1.4.0](https://github.com/gregsdennis/json-everything/pull/75)

Added support for nullable reference types.

Related to [#76](https://github.com/gregsdennis/json-everything/issues/76), added `.GetEquivalenceHashCode()` extension for `JsonElement`.  Credit to [@amosonn](https://github.com/amosonn) for pointing me to a good hash code method in the wild.

# [1.3.0](https://github.com/gregsdennis/json-everything/pull/65)

Added `JsonElementProxy`.  This class allows the client to define methods that expect a `JsonElement` to be called with native types by defining implicit casts from those types into the `JsonElementProxy`.

Suppose you have this method:

```c#
void SomeMethod(JsonElement element) { ... }
```

The only way to call this is by passing a `JsonElement` directly.  If you want to call it with a `string` or `int`, you have to resort to converting it with the `.AsJsonElement()` extension method:

```c#
myObject.SomeMethod(1.AsJsonElement());
myObject.SomeMethod("string".AsJsonElement());
```

This gets noisy pretty quickly.  But now we can define an overload that takes a `JsonElementProxy` argument instead:

```c#
void SomeMethod(JsonElementProxy element)
{
    SomeMethod((JsonElement) element);
}
```

to allow callers to just use the raw value:

```c#
myObject.SomeMethod(1);
myObject.SomeMethod("string");
```

# [1.2.3](https://github.com/gregsdennis/json-everything/pull/61)

Signed the DLL for strong name compatibility.

# [1.2.2](https://github.com/gregsdennis/json-everything/pull/45)

Added debug symbols to package.  No functional change.

# [1.2.1](https://github.com/gregsdennis/json-everything/pull/24)

`.ToJsonString()` now just calls the serializer.

# [1.2.0](https://github.com/gregsdennis/json-everything/pull/24)

Added `.ToJsonString()` extension for `JsonElement` as `.ToString()` [does not output JSON content](https://github.com/dotnet/runtime/issues/42502).

# 1.1.0

Not released; skipped for some reason.

<img src="https://i.imgflip.com/1myuho.jpg" style="height:100px"></img>

# 1.0.0

Initial release.