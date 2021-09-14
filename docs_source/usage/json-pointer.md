# JsonPointer<nsp>.Net

[JSON Pointer](https://tools.ietf.org/html/rfc6901) is a mechanism that allows you to isolate a single element within a JSON document by navigating down a series of object properties and array indices.

<p style="text-align: center;">
<a href="https://json-everything.net/json-pointer" target="_block" style="color: rgb(255, 255, 255); background-color: rgb(13, 71, 161); display: inline-block; font-weight: 500; font-size: 2rem; text-align: center; vertical-align: middle; padding: 0.6rem 0.9rem; border-radius: 0.35rem; cursor: pointer; user-select: none; text-decoration: none; --darkreader-inline-color:#ffffff; --darkreader-inline-bgcolor:#06419c; margin-top: 1.5rem !important;">Try it online!</a>
</p>

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

The `JsonPointer` struct is the model for JSON Pointer.

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

All of these options will give you an instance of the model that can be used to evaluate instances. 

```c#
var element = JsonDocument.Parse("{\"objects\":{\"and\":[\"item zero\",null,2,{\"arrays\":\"found me\"}]}}");
var result = pointer.Evaluate(element); // contains a JsonElement with a "found me" value
```

## Relative JSON Pointers

[JSON Hyperschema](https://datatracker.ietf.org/doc/draft-handrews-json-schema-hyperschema/) relies on a variation of JSON Pointers called [Relative JSON Pointers](https://tools.ietf.org/id/draft-handrews-relative-json-pointer-00.html) that also includes the number of parent navigations.  This allows the system to start at an internal node in the JSON document and navigate to another node potentially on another subtree.

Relative JSON Pointers are implemented with the `RelativeJsonPointer` struct.  Interactions with this struct are very similar to `JsonPointer`.

Unfortunately, since evaluation of these pointers require parent navigation, a feature which is [currently unsupported by `System.Text.Json`](https://github.com/dotnet/runtime/issues/40452), only the model is available at this time; evaluation is non-functional.
