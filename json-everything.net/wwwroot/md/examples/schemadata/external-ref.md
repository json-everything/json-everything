# Referencing external data {#example-schemadata-external}

Sometimes the values you want to use for schemas are stored in external files.  To reference these, you'll need to use a URI.  This URI may be combined with a pointer to indicate a location within the file.

## Requiring that `A` is one of the items in _values.json_ {#example-schemadata-element-of}

We want to specify an enum for `A`, and the values in that enum are contained in a separate file, _values.json_.  As with the instance data reference case, JSON Schema only allows for explicit lists for `enum`, so we can't source it from somewhere else.

```json
{
  "type": "object",
  "properties": {
    "A": { "enum": [ ??? ] }
  }
}
```

_values.json_
```json
{
  "values": [ "dog", "cat", "gerbil" ]
}
```

How do we define that `A` has to be one of the strings defined in _values.json_?

We need to define the keyword that needs the reference (`enum`) inside a `data` keyword and provide a plain JSON Pointer that points to where in the instance we want to get the values; in this case from the _values.json_ file at `/values`.

```json
{
  "$schema": "https://json-everything.net/meta/data-2022",
  "$id": "https://json-everything.net/example/external-ref",
  "type": "object",
  "properties": {
    "A": {
      "data": {
        "enum": "https://data.myserver.com/values.json#/values"
      }
    }
  }
}
```
However, before we can use it, we need to tell the `data` keyword how to find `_values.json`.  There are two ways to do this: registration and fetching.

Registration is the preferred method as it doesn't require that the implementation make any web calls.  The `DataKeyword` class exposes a static `ExternalDataRegistry` property that can house any `JsonNode` data.

```c#
var valuesText = File.ReadAllText("values.json");
var values = JsonNode.Parse("valuesText");
DataKeyword.ExternalDataRegistry["https://data.myserver.com/values.json"] = values;
```

Fetching the data incurs some security risk as you're calling out to the internet.  To set this up, you'll need to set the `DataKeyword.Fetch` static property to a method that will retrieve your data.  The `DataKeyword.SimpleDownload()` method has been provided for convenience, but it is quite basic.  It supports HTTP/S and `file` protocols.  If you want something more robust or different (e.g. fetching from a database), you'll need to write that method yourself.

```c#
DataKeyword.Fetch = DataKeyword.SimpleDownload;
```

The system will always check the registry for a matching URI before attempting to fetch it.

Now we can run it:

| Passes | Fails |
| :-: | :-: |
| `{ "A": "cat" }` | `{ "A": "giraffe" }` |