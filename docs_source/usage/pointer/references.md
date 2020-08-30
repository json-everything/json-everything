# Managing references (`$ref`)

Manatee.Json handles all references as defined in the draft 2019-09 version of the JSON Schema specification.  This is *not* a change as of v11.0.0;  Manatee.Json has always behaved this way.  The only change for draft 2019-09 schemas in Manatee.Json is that `$ref` can now exist alongside other keywords; for earlier drafts, keywords as siblings to `$ref` will be ignored.

## Automatic resolution

Manatee.Json will automatically attempt to find schemas based on the current document path.  That is, if the `$id` URI is a local or network location, it will be loaded and registered without any additional work from you.  This is performed via the protocol defined by the `$id`.  The following are supported:

- `http`/`https` - Will attempt to download the schema from the specified online location.
- `file` - Will attempt to load the schema from the specified disk location.

Any other protocols will result in an error.

This behavior can be overridden, however.  The `JsonSchemaOptions.Download` static property is a function that takes a string (the `$id` URI) and returns the serialized JSON Schema content.  This method is used by the `JsonSchemaRegistry` static class (see [below](#schema-registration)) to obtain schema data.  This setting can also be used if the `$id` URIs are not located at the network address they specify.  See the [JSON Schema Test Suite runner](https://github.com/gregsdennis/Manatee.Json/blob/master/Manatee.Json.Tests/Schema/TestSuite/JsonSchemaTestSuite.cs#L77-L88) for an example of where localhost URIs are translated into file paths.

Another interesting use case for this is is loading schemas from a database, where the data is indexed by the `$id` URI.

## Schema registration

In order to prevent having to download schemas over and over, Manatee.Json maintains a registry for all schemas that it has encountered.  The first step in resolving a reference is to determine if it has already resolved it.

A `JsonSchema` instance will automatically register itself upon calling `Validate()`.  However, there are some cases where this may be insufficient.  For example, in cases where schemas are separated across multiple files, it may be necessary to register the schema instances explicitly.

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

If these schemas were located at the specified network locations, when `my-schema` is used to validate, it would

1. register itself
2. continue validation, hitting the reference to `random-string`
3. download `random-string`
4. start validation of `random-string`
5. register `random-string`
6. continue validation

This works fine because Manatee.Json can resolve `random-string` on its own.  This would be different if they were defined in code or if they weren't available at the network locations.

```c#
var mySchema = new JsonSchema()
    .Id("http://localhost/my-schema")
    .Type(JsonSchemaType.Object)
    .Property("refProp", new JsonSchema().Ref("http://localhost/random-string"));
var randomString = new JsonSchema()
    .Id("http://localhost/random-string")
    .Type(JsonSchemaType.String);
```

In this case, the registry wouldn't know about `randomString`, so we need to explicitly register it.

```c#
JsonSchemaRegistry.Register(mySchema);
JsonSchemaRegistry.Register(randomString);
```

Now Manatee.Json will be able to resolve the reference.