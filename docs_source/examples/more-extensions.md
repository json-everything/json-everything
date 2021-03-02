# `JsonElement` (and `JsonDocument`) Extensions

`System.Text.Json` is great for serialization needs, but the Document Object Model (DOM) needs a bit of work.  To this end, `Json.More` supplies several extensions that make working with the `JsonElement` struct easier.

***NOTE** Many of these extensions exist for `JsonDocument` as well by proxying to the `.RootElement` property.*

## JSON-ifying

One might expect `JsonElement.ToString()` to return valid JSON.  I sure did.  It doesn't.

So we created the `.ToJsonString()` extension method.  This method basically just serializes the element, but

```c#
element.ToJsonString();
```

is a lot more readable than

```c#
JsonSerializer.Serialize(element);
```

## Equality

Equality isn't implemented for `JsonElement` _at all_!  So anytime you do

```c#
element1 == element2
```

or even

```c#
Equals(element1, element2)
```

you just get the base `object` implementation, which just compares references.  That's not really useful.

To get JSON-equivalence, you need to use `.IsEquivalentTo()`.  This will compare primitive values (`bool`, `number`, `string`, and `null`) directly and as expected.  For objects and arrays, it follows the JSON specification for equality:

- **objects** - unordered key-matching with value comparison
- **arrays** - sequential value comparison

There's even a `.GetEquivalenceHashCode()` method that follows the same rules!  

*Credit for `.GetEquivalenceHashCode()` goes to StackOverflow user [dbc](https://stackoverflow.com/users/3744182/dbc) for their [wonderful answer](https://stackoverflow.com/a/60592310/878701).*

These extension methods have also been integrated into `JsonElementEqualityComparer` which implements `IEqualityComparer<JsonElement>` and can be used in, e.g., Linq queries.

## Conversions

A staple of this library suite's predecessor was the ability to easily build JSON in C# code.  Sadly, type conversion is a feature that was left out for `JsonElement`.

Now you have `.ToJsonElement()`.  This will convert the appropriate types into corresponding `JsonElement` representations.

- `bool`
- number types (`int`, `double`, `decimal`, etc.)
- `string` (also supports `null`!)
- `IEnumerable<JsonElement>` for arrays
- `IDictionary<string, JsonElement>` for objects
