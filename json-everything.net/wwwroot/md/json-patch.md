[JSON Patch](https://tools.ietf.org/html/rfc6902) is a language for modifying JSON documents.  Like JSON Schema, it is also expressed in JSON.

## Syntax

A patch consists of an array containing one or more operations.  Each operation may also contain one or more arguments.

An operation must be one of

- `add`
- `remove`
- `replace`
- `copy`
- `move`
- `test`

The arguments vary among them, though all must contain at least an `op` and a `path`.  ("Path" as used here is colloqiual.  It's actually a [JSON Pointer](json-pointer.md), not [JSON Path](json-path.md).)

- `op` specifies the operation and must be one of the values above.
- `path` specifies the location within the JSON document which will receive changes.
- `from` specifies a source location within the JSON document from which to pull a value.
- `value` specifies an explicit value.

## Applying Patches

In JsonPatch<nsp>.Net, a `JsonPatch` object can be deserialized directly from the JSON document string.

```c#
var patch = JsonSerializer.Deserialize<JsonPatch>(patchString);
```

`JsonPatch` operates on `JsonElement` values.  To apply the patch, parse the document and pass the root element into the `.Apply()` method.

```c#
var doc = JsonDocument.Parse(docString);
var result = patch.Apply(doc.RootElement);
```

The result contains the altered document, either fully patched or up to the point an error occurred, and possibly an error message.

You can even apply a patch from one object to another!  They don't even need to be of the same type!!

```c#
var myPatchedObject = patch.Apply(myObject);
var myDifferentTypeObject = patch.Apply<MyObject, MyDifferentObject>(myObject);
```

## Inline Patching

The `JsonPatch` class can also be built in code using by creating a series of `PatchOperation`s through its static constructor methods.  There's one for each operation.

```c#
var patch = new JsonPatch(PatchOperation.Add("/foo/bar", "baz"),
                          PatchOperation.Test("/foo/biz", false));
```

That's it!

## Generating Patches

If you know what your start and end states are, but you want to find the differences, you can do that by generating a patch.

```c#
var start = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,4]}]");
var target = JsonDocument.Parse("[{\"test\":\"test123\"},{\"test\":\"test32132\"},{\"test1\":\"test321\"},{\"test\":[1,2,3]},{\"test\":[1,2,3]}]");

var patch = start.CreatePatch(target);
```

This results in the JSON Patch

```json
[
  {"op":"replace","path":"/1/test","value":"test32132"},
  {"op":"remove","path":"/2/test"},
  {"op":"add","path":"/2/test1","value":"test321"},
  {"op":"replace","path":"/3/test/2","value":3},
  {"op":"add","path":"/4","value":{"test":[1,2,3]}}
]
```

This patch can be tested on the first object and should generate the second.

Other related features:

- Generate reverse patches: `target.CreatePatch(start)`
- Generate patches between .Net objects: `myObject1.CreatePatch(myObject2)`