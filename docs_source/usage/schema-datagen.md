# Generation Sample JSON Data from a Schema

JsonSchema.Net.DataGeneration is a tool that can create JSON data instances using a JSON schema as a framework.

For example, given the schema:

```json
{
  "type": "object",
  "properties": {
    "foo": { "type": "string" }
  }
}
```

it can generate a JSON document like

```json
{
  "foo": "bar"
}
```

Under the covers, the library uses the fabulous [Bogus](https://github.com/bchavez/Bogus) library, which is commonly used to generate random test data, and a few other tricks.

## Capabilities

This library is quite powerful.  It supports most JSON Schema keywords, including `if`/`then`/`else` and aggregation keywords (`oneOf`, `allOf`, etc.).

It currently does not support:

- anything involving RegEx\*
- reference keywords (e.g. `$ref`, `$dynamicRef`, etc)
- annotation / metadata keywords (e.g. `title`, `description`)
- `content*` keywords
- `dependencies` / `dependent*` keywords

*\* There are some libraries which provide limited RegEx-based string generation, but these do not support look-aheads which are required to combine multiple RegEx's with boolean logic.  This functionality is required to support them alongside the aggregation keywords.  I opted to just not support them at all until I can find a sufficient library.*

Everything else _should_ be mostly supported.  Feel free to [open an issue](https://github.com/gregsdennis/json-everything/issues/new/choose) if you find something isn't working as you expect.

### Strings & `format`

All of the formats listed in the draft 2020-12 specification are supported, at least to the extent that they can be validated by JsonSchema.Net.

If a format is specified, it will be used.

If a format is not specified, Bogus's Lorem Ipsum generator is used to create some nice garbage (but oddly readable) text.

### Numerics

Integer and number generation each have custom algorithms that produce values that align with minimums, maximums, multiples, and even anti-multiples (numbers that should _not_ be divisors).

For this schema, 

```json
{
  "type": "integer",
  "minimum": 0,
  "maximum": 100,
  "multipleOf": 4,
  "allOf": [
    {
      "not": {
        "minimum": 40,
        "maximum": 60
      }
    },
    {
      "not": {
        "multipleOf": 3
      }
    }
  ]
}
```

the only valid integers are

- either in \[0-39] or \[61-100]
- a multiple of 4
- not a multiple of 3

The library will generate such values with ease.

### Arrays & Objects

Care needs to be taken when specifying arrays that can have additional items or objects that can have additional properties.  This library will unsubtly create moderatly deep trees of data if allowed.

For example, this schema doesn't specify what the items should look like:

```json
{
  "type": "array"
}
```

So, the generator will happily create literally any JSON value for the items, including unconstrained objects and arrays.

To combat this, there are some built-in limitations:

- Item and property counts default to 0-10.
- Arrays and objects have a lower chance of generating than the simpler types (null, integer, number, string).

# Generating Data

All you need to generate data is a schema object.  This can be built inline or read in from an external source.  The instructions for that are on the [Building a Schema](./schema-building.md) page.

Once you have your schema object, simply call the `.GenerateData()` extension method, and it will return a result to you.

```c#
var schema = JsonSchema.FromFile("myFile.json");
var generationResult = schema.GenerateData();
var sampleData = generationResult.Result;
```

The result object has several properties:

- `IsSuccess` indicates whether the system was able to generate a value
- `Result` holds the value as a `JsonElement`, if successful
- `ErrorMessage` holds any error message, if unsuccessful
- `InnerResults` holds result objects from nested generations.  This can be useful for debugging.

# Summary

So, uh, yeah.  I guess that's it really.

The generation isn't 100%, but most of the time it will succeed in producing a value for schemas that can have one.  You may want to validate the value against the schema as a sanity check.

Happy generating.