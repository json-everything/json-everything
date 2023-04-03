# Validated Deserialization {#schema-validated-deserialization}

Arguably the most beneficial aspect of using a schema comes when validating data before deserializing it into your data models.  Typically, you'd want to do this _during_ deserialization, while you're reading the data from a stream.

Unfortunately, this is isn't possible with JSON Schema.

## Why is streaming validation impossible? {#schema-validated-deserialization-impossible}

Some keywords require having knowledge of the entire instance (or at least enough of it) in order to perform their jobs.  `required` is one such keyword.  In the worst case, it needs to have the entire object parsed before it can know whether it's valid.

Let's look at this example.

```json
{
  "required": ["foo", "bar"]
}
```

All this schema does is require that `foo` and `bar` are properties, if the instance is an object.  So given this instance:

```json
{
  "a": 1,
  "bar": 2,
  "c": 3,
  "d": 4,
  "e": 5,
  "foo": 6
}
```

the schema can't know whether the `required` constraints have been satisfied until it reads both `foo` and `bar`.  Since `foo` is at the end, it has to parse the entire object.

This is the worst case. At best, the required properties would appear at the beginning of the stream, and `required` could satisfy its constraint early.  However, in general, we must account for the worst case.

## So how do we validate before deserialization? {#schema-validated-deserialization-how-to}

`JsonSchema` operates on `JsonNode`.  Because of this, we can split our deserialization into two steps and insert validation in the middle.

Deserialization is merely parsing text and populating the data model.  The `JsonSerializer` provided with `System.Text.Json` combines these step into a seamless single call that goes directly from text into your data model.  But we don't want that.

The first step is to parse the text into `JsonNode`.

```c#
var jsonNode = JsonNode.Parse(myJsonText);
```

Here, `myJsonText` could be a string or a stream; there are `.Parse()` overloads for both.  In either case, the entirety of the text will be read in to create a full, in-memory model of the JSON data.

Next, we validate.

```c#
var results = mySchema.Evaluate(jsonNode);

if (!results.Valid)
{
    // report the error however your application needs
    return;
}
```

Lastly, we deserialize.

```c#
var myDataModel = jsonNode.Deserialize<MyDataModel>();
```

`.Deserialize<T>()` is an extension method defined by `JsonSerializer` that deserializes from `JsonNode` into `T`.

***NOTE** This extension operates by getting the string text from the node and [parsing it again](https://github.com/dotnet/runtime/issues/84234) through deserialization.  Currently, the above is the only way to do this.*