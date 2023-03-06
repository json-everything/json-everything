# Referencing instance data {#example-schemadata-instance}

Most of the questions that center around referencing data involve comparing the values of properties.  This typically comes in a few flavors:

- How do I require that property `A` is less than property `B`?
- How do I require that property `A` is one of a list of things specified by property `B`?
- How do I require that...

## Requiring that `A < B` {#example-schemadata-comparative}

We want to specify a minimum for `B` that is the value of `A`.  Unfortunately, JSON Schema doesn't have a mechanism that allows this; it only has `minimum` which must be a static (unchanging) value.  Specifically, as enforced by the meta-schema, `minimum` must be a number.

```json
{
  "type": "object",
  "properties": {
    "A": { "type": "number" },
    "B": {
      "type": "number",
      "minimum": ???
    }
  }
}
```

We can't put a number here because we want that value to depend on the value of `A`.

To solve this, we remove the `minimum` keyword and add the `data` keyword.  Inside `data`, we specify `minimum` with a plain JSON Pointer (_not_ URI-encoded) that points to where in the instance we want to get the value; in this case, from the `A` property.

```json
{
  "$schema": "https://json-everything.net/meta/data-2022",
  "type": "object",
  "properties": {
    "A": { "type": "number" },
    "B": {
      "type": "number",
      "data": {
        "minimum": "/A"
      }
    }
  }
}
```

| Passes | Fails |
| :-: | :-: |
| `{ "A": 5, "B": 10 }` | `{ "A": 15, "B": 10 }` |

## Requiring that `A` is one of the items in `B` {#example-schemadata-element-of}

We want to specify an enum for `A`, and the values in that enum are contained in `B`.  Similarly to the numbers problem above, JSON Schema only allows for explicit lists for `enum`, so we can't source it from somewhere else.

```json
{
  "type": "object",
  "properties": {
    "A": { "enum": [ ??? ] },
    "B": {
      "type": "array",
      "items": { "type": "string" }
    }
  }
}
```

We know that `B` must be an array of strings, but how do we define that `A` has to be one of the strings defined in `B`?

Again, we need to define the keyword that needs the reference (`enum`) inside a `data` keyword and provide a plain JSON Pointer that points to where in the instance we want to get the values; in this case from the `B` property.

```json
{
  "$schema": "https://json-everything.net/meta/data-2022",
  "type": "object",
  "properties": {
    "A": {
      "data": {
        "enum": "/B"
      }
    },
    "B": {
      "type": "array",
      "items": { "type": "string" }
    }
  }
}
```

| Passes | Fails |
| :-: | :-: |
| `{ "A": "cat", "B": [ "dog", "cat", "gerbil" ] }` | `{ "A": "giraffe", "B": [ "dog", "cat", "gerbil" ] }` |