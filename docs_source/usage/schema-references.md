# Managing references (`$ref`)

JsonSchema<nsp>.Net handles all references as defined in the draft 2020-12 version of the JSON Schema specification.  What this means for draft 2019-09 and later schemas is that `$ref` can now exist alongside other keywords; for earlier drafts, keywords as siblings to `$ref` will be ignored.

## Schema registration

In order to resolve references more quickly, JsonSchema<nsp>.Net maintains two schema registries for all schemas and subschemas that it has encountered.  The first is a global registry, and the second is a local registry that is passed around on the validation context.  If a schema is not found in the local registry, it will automatically fall back to the global registry.

A `JsonSchema` instance will automatically register itself upon calling `Validate()`.  However, there are some cases where this may be insufficient.  For example, in cases where schemas are separated across multiple files, it is necessary to register the schema instances prior to validation.

For example, given these two schemas

```json
{
  "$id": "http://localhost/my-schema",
  "$type": "object",
  "properties": {
    "refProp": { "$ref": "http://localhost/random-string" }
  }
}

{
  "$id": "http://localhost/random-string",
  "type": "string"
}
```

> Here's the schema build inline:
> 
> ```c#
> var schema = new JsonSchemaBuilder()
>     .Id("http://localhost/my-schema")
>     .Type(SchemaValueType.Object)
>     .Properties(("refProp", new JsonSchemaBuilder().Ref("http://localhost/random-string")))
>     .Build();
> ```

You must register `random-string` before you attempt to validate with `my-schema`.

```c#
var randomString = JsonSchema.FromFile("random-string.json");
SchemaRegistry.Global.Register("http://localhost/random-string", randomString);
```

Now JsonSchema<nsp>.Net will be able to resolve the reference.

## Automatic resolution

In order to support scenarios where schemas cannot be registered ahead of time, the `SchemaRegistry` class exposes the `Fetch` property which is defined as `Func<Uri, JsonSchema>`.  This property can be set to a method which downloads the content from the supplied URI and deserializes it into a `JsonSchema` object.

The URI that is passed may need to be transformed, based on the schemas you're dealing with.  For instance if you're loading schemas from a local filesystem, and the schema `$ref`s use relative paths, you may need to prepend the working folder to the URI in order to locate it.