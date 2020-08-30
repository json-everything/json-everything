# Building a schema

There are two options when building a schema: defining it inline using the object model and defining it externally and deserializing.  Which method you use depends on your specific requirements.

## Deserialization

Manatee.Json schemas are fully serializable using the default serializer settings.  Just create a new `JsonSerializer` and deserialize as you would any other object.

```csharp
var serializer = new JsonSerializer();
var text = File.ReadAllText("mySchema.json");
var json = JsonValue.Parse(text);
var mySchema = serializer.Deserialize<JsonSchema>(json);
```

Done.

## Inline

To build a schema inline, you can either declare all of the keywords individually and add them to a `JsonSchema` instance:

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

```csharp
var schema = new JsonSchema {
    new PropertiesKeyword {
        ["myProperty"] = new JsonSchema {
            new TypeKeyword(JsonSchemaType.String),
            new MinLengthKeyword(10)
        }
    },
    new RequiredKeyword{ "myProperty" }
};
```

or you can use the fluent interface:

```csharp
var schema = new JsonSchema()
    .Property("myProperty", new JsonSchema()
        .Type(JsonSchemaType.String),
        .MinLength(10))
    .Required("myProperty");
```

***NOTE** The meta-schemas exposed by the library are built using the fluent syntax.*
