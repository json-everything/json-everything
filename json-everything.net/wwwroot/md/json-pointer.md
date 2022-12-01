***NOTE** This documentation is based on the the latest non-beta version.  Updated documentation is in progress and will be available soon.*

[JSON Pointer](https://tools.ietf.org/html/rfc6901) is a syntax that allows you to isolate a single element within a JSON document by navigating down a series of object properties and array indices.

## The syntax

The syntax is really simple:

```
/objects/and/3/arrays
```

This pointer has four segments.  Each segment specifies either an object property or, if the segment is a number, an array index.  Interestingly, the `3` above could be either an object property *or* an array index.  There's nothing about the pointer that specifies a distinction.  It will resolve for both of these documents:

```json
{
  "objects": {
    "and": [
      "item zero",
      null,
      2,
      {
        "arrays": "found me"
      }
    ]
  }
}

{
  "objects": {
    "and": {
      "3": {
        "arrays": "found me"
      }
    }
  }
}
```

If a property contains a `/`, it must be escaped by replacing it with `~1`.  Additionally, the escape character `~` must be escaped by replacing it with `~0`.

It also supports a URL format, which is essentially the same thing, except that it starts with a `#`, then followed by the standard pointer.  This format also will `%`-encode any URL-reserved characters, like `=` and `?`.

## In code

The `JsonPointer` class is the model for JSON Pointer.

There are three ways create pointers:

- parsing with either `Parse()` or `TryParse()`
  ```c#
  var pointer = JsonPointer.Parse("/objects/and/3/arrays");
  ```
- building with `Create()` and supplying the segments explicitly
  ```c#
  var pointer = JsonPointer.Create("object", "and", 3, "arrays");
  ```
- building with `Create<T>()` and supplying a LINQ expression
  ```c#
  var pointer = JsonPointer.Create<MyObject>(x => x.objects.and[3].arrays);
  ```

All of these options will give you an instance of the model that can be used to evaluate JSON data.

```c#
using var element = JsonDocument.Parse("{\"objects\":{\"and\":[\"item zero\",null,2,{\"arrays\":\"found me\"}]}}");
var result = pointer.Evaluate(element); // result contains a JsonElement with a "found me" value
```

or

```c#
var element = JsonNode.Parse("{\"objects\":{\"and\":[\"item zero\",null,2,{\"arrays\":\"found me\"}]}}");
var success = pointer.TryEvaluate(element, out var result);
// success is true
// result contains a JsonNode with a "found me" value
```

***ASIDE** The designers of the `JsonNode` API have elected (for [reasons](https://github.com/dotnet/designs/blob/40794be63ecd8b35e9596412050a84dedd575b99/accepted/2020/serializer/WriteableDomAndDynamic.md#missing-vs-null) I [disagree](https://github.com/dotnet/runtime/issues/66948#issuecomment-1080148457) with) to consider JSON null and .Net null to be equivalent.  This goes against both my personal experience building Manatee.Json and the `JsonElement` API, in which these are distinct concepts.  Because of this, it is impossible to determine whether a returned `JsonNode` value of `null` represents a value that is present but null or it is merely absent from the data.  To accomodate this, the evaluation method can only support the familiar `TryParse()` signature.  A return of `true` indicates the value was found, and `false` indicates it was not.  In the case of a `true` return, `result` may still be null, indicating the value was found and was a JSON null.*

## Relative JSON Pointers

[JSON Hyperschema](https://datatracker.ietf.org/doc/draft-handrews-json-schema-hyperschema/) relies on a variation of JSON Pointers called [Relative JSON Pointers](https://tools.ietf.org/id/draft-handrews-relative-json-pointer-00.html) that also includes the number of parent navigations.  This allows the system to start at an internal node in the JSON document and navigate to another node potentially on another subtree.

Relative JSON Pointers are implemented with the `RelativeJsonPointer` struct.  Interactions with this struct are very similar to `JsonPointer`.

Since evaluation of these pointers require parent navigation, a feature which is [unsupported by `JsonElement`s](https://github.com/dotnet/runtime/issues/40452), only the `JsonNode`s can be processed.
