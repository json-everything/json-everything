# Json.More<nsp>.Net

Json.More<nsp>.Net aims to fill gaps left by `System.Text.Json`.  To this end, it supplies four additional functions.

## Equality comparison

Sadly, it seems equality was considered unnecessary.  To remedy that, the `.IsEquivalentTo()` extension method is supplied for `JsonDocument` and `JsonElement`.

This extension method calculates the JSON-equality of the nodes.  This means that objects are key-matched (unordered) and arrays are sequence-matched (ordered).

From json.org:

> An *object* is an unordered set of name/value pairs.

and

> An *array* is an ordered collection of values.

Additionally, an `IEqualityComparer<JsonElement>` is supplied (`JsonElementEqualityComparer`) which has a convenient static `Instance` property.

## Enum serialization

The `EnumStringConverter<T>` class enables string encoding of enum values.  `T` is the enum.

The default behavior is to simply encode the C# enum value name.  This can be overridden with the `System.ComponentModel.DescripitionAttribute`.

```c#
public enum MyEnum
{
    Foo,                    // serializes as "Foo"
    [Description("bar")]
    Bar                     // serializes as "bar"
}
```

It also supports `[Flags]` enums by encoding the base values into an array.  It does not support composite values.

```c#
[Flags]
public enum MyFlagsEnum
{
    Foo = 1,
    Bar = 2,
    Composite = 3           // serializes as ["Foo", "Bar"]
}
```

To use this converter, apply the `[JsonConverter(typeof(EnumStringConverter<T>))]` to either the enum or an enum-valued property.

## Data conversions

### `.AsJsonElement()` extension

Sometimes you just want a `JsonElement` that represents a simple value, like a string, boolean, or number.  This library exposes several overloads of the `.AsJsonElement()` extension that can do this for you.

Supported types are:

- `bool`
- Numeric types (e.g. `double`, `decimal`, `int`, etc.)
- `string`
- `IEnumerable<JsonElement>` (for arrays)
- `IDicationary<string, JsonElement>` (for objects)

For example, to create an empty array, you can use

```c#
var emptyArray = new JsonElement[0].AsJsonElement();
```

To create an object with an `6` in the `myInt` property:

```c#
var obj = new Dictionary<string, JsonElement>{
    ["myInt"] = 6.AsJsonElement()
}
```

### Making methods that require `JsonElement` easier to call

The `JsonElementProxy` class allows the client to define methods that expect a `JsonElement` to be called with native types by defining implicit casts from those types into the `JsonElementProxy` and then also an implicit cast from the proxy into `JsonElement`.

Suppose you have this method:

```c#
void SomeMethod(JsonElement element)
{
    ...
    DoSomething(element);
    ...
}
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
    ...
    DoSomething(element); // still only accepts JsonElement; doesn't need an overload
    ...
}
```

to allow callers to just use the raw value:

```c#
myObject.SomeMethod(1);
myObject.SomeMethod("string");
```

To achieve this without `JsonElementProxy`, you could also create overloads for `short`, `int`, `long`, `float`, `double`, `decimal`, `string`, and `bool`.

## JSON model serialization

The .Net team did a great job of supporting fast serialization, but for whatever reason they didn't implement serializing their data model.  The `Utf8JsonWriterExtensions` class fills that gap.

This provides an extension method that writes a `JsonElement` to the stream.