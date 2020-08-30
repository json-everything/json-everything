# Building a schema

There are two options when building a schema: defining it inline using the fluent builder and defining it externally and deserializing.  Which method you use depends on your specific requirements.

## Deserialization

JsonSchema<nsp>.Net schemas are fully serializable.

```c#
var mySchema = JsonSchema.FromText(content);
```

which just does

```c#
var mySchema = JsonSerializer.Deserialize<JsonSchema>(content);
```

Done.

## Inline

To build a schema inline, you will utilize the `JsonSchemaBuiler` class and its fluent-style extension methods.

```json 
{
    "properties":{
        "myProperty":{
            "type":"string",
            "minLength":10
        }
    },
    "required":["myProperty"]
}
```

```c#
// JsonSchemaBuilder has an implicit cast for convenience that calls
// the .Build() method.  To use this you need to specify the type in
// the declaration.
JsonSchema schema = new JsonSchemaBuilder()
    .Properties(
        (
            "myProperty", new JsonSchemaBuilder()
                .Type(SchemaValueType.String),
                .MinLength(10)
        )
    )
    .Required("myProperty");
```

The fluent-style interface is designed to be able to express the schema in C# in such a way that it mimics the JSON representation.

***NOTE** The meta-schemas exposed by the library are built using the fluent syntax.*
