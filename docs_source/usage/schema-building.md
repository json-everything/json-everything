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

There are many reasons why you would want to hard-code your schemas.  This library actually hard-codes all of the meta-schemas.  Whatever your reason, the `JsonSchemaBuilder` class is going to be your friend.

The builder class itself is pretty simple.  It just has an `.Add()` method which takes an instance of `IJsonSchemaKeyword`.  The real power comes from the multitudes of extension methods.  There's at least one for every keyword, and they all take the appropriate types for the data that the keyword expects.

Once you've added all of your properties, just call the `.Build()` method to get your schema object.

```c#
var builder = new JsonSchemaBuilder()
    // builder methods
    ;
var schema = builder.Build();
```

Let's take a look at some of the builder extension methods.

### Easy Mode

Some of the more straightforward builder methods are for like the `title` and `$comment` keywords, which just take a string:

```c#
builder.Comment("a comment")
    .Title("A title for my schema");
```

Notice that these methods implement a fluent interface so that you can chain them together.

### A Little Spice

Other extension methods can take multiple values.  These have been overloaded to accept both `IEnumerable<T>` and `params` arrays just to keep things flexible.

```c#
var required = new List<string>{"prop1", "prop2"};
builder.Required(required);
```

or just

```c#
builder.Required("prop1", "prop2");
```

### Now You're Cooking With Fire

Lastly, we have the extension methods which take advantage of C# 7 tuples.  These include keywords like `$defs` and `properties` which take objects in their JSON form.

```c#
builder.Properties(
        ("prop1", new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .MinLength(8)
        ),
        ("prop2", new JsonSchemaBuilder()
            .Type(SchemaValueType.Number)
            .MultipleOf(42)
        )
    );
```

Did you notice how the `JsonSchemaBuilder` is just included directly without the `.Build()` method?  These methods actually require `JsonSchema` objects.  This leads us into the next part.

### Conversions

`JsonSchemaBuilder` defines an implicit cast to `JsonSchema` which calls the `.Build()` method.

To help things further, `JsonSchema` also defines implicit conversions from `bool`.  This allows you to simply use `true` and `false` to create their respective schemas.

```c#
builder.Properties(
        ("prop1", new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .MinLength(8)
        ),
        ("prop2", new JsonSchemaBuilder()
            .Type(SchemaValueType.Number)
            .MultipleOf(42)
        ),
        ("prop3", true)
    );
```

This cast can be used anywhere a `JsonSchema` is needed, such as in the `additionalProperties` or `items` keywords.

```c#
builder.Properties(
        ("prop1", new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .MinLength(8)
        ),
        ("prop2", new JsonSchemaBuilder()
            .Type(SchemaValueType.Number)
            .MultipleOf(42)
        ),
        ("prop3", true),
        ("prop4", new JsonSchemaBuilder()
            .Type(SchemaValueType.Array)
            .Items(true)
        )
    )
    .AdditionalProperties(false);
```