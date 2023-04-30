# Enhancing Deserialization with JSON Schema {#schema-deserialization}

JsonSchema.Net includes a JSON converter implementation that provides JSON validation support _during_ deserialization.

To enable this support, you'll need to include the `ValidatingJsonConverter` in the serialization options and then annotate any types that need validation with the `[JsonSchema()]` attribute, pointing the the schema for that type.

Let's walk through it.

## Setting up the converter {#schema-deserialization-setup}

Custom JSON converters are added via the `JsonSerializationOptions.Converters` property.  Any converters in this collection will have priority over the default set of converters that ship with .Net.  You can read more about custom converters in their [documentation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0).

When preparing to deserialize your payload, create an options object and add the `ValidatingJsonConverter` from JsonSchema.Net:

```c#
var options = new JsonSerializationOptions
{
    Converters = { new ValidatingJsonConverter() }
};
```

Whenever you deserialize with these options, this converter will be queried to see if the type to deserialize is configured with a `[JsonSchema()]` attribute.  If it is, then the payload will be validated against the schema prior to deserialization.

```c#
var myModel = JsonSerializer.Deserialize<MyModel>(jsonText, options);
```

### Error reporting {#schema-deserialization-errors}

If the data isn't valid, then a `JsonException` will be thrown.  The validation results will be in the `.Data` dictionary on the exception under the `"validation"` key.  (You'll need to cast it to `EvaluationResults`.)

### Configuration {#schema-deserialization-configuration}

The validation can be configured using properties on the converter.

- `OutputFormat` configures the JSON Schema output format to be used.  By default, this value is `Flag` (the same as on `EvaluationOptions`).
- `RequireFormatValidation` will validate the `format` keyword when set to true.  By default `format` is an annotation.
- `Log` will set an `ILog` implementation to report schema debugging information.  By default, no log is set.

## Declaring a JSON Schema for a type {#schema-deserialization-attribute-usage}

To declare a JSON Schema for any type, it needs to be decorated with the `[JsonSchema()]` attribute.

This attribute takes two parameters:

- the type in which the schema is defined
- the name of a public static property or field which returns a `JsonSchema`

For example, for `MyModel`

```c#
public class MyModel
{
    public string Foo { get; set; }
}
```

we need to define a schema.  It can be in the same class or another class.  For now, let's assume we're collecting our schemas in a single static class.

```c#
public static class Schemas
{
    public static JsonSchema MyModelSchema =
        new JsonSchemaBuilder()
            .Type(JsonSchemaType.Object)
            .Properties(
                (nameof(MyModel.Foo), new JsonSchemaBuilder()
                    .Type(JsonSchemaType.String)
                    .MinLength(10)
                    .MaxLength(50)
                )
            )
            .Required(nameof(MyModel.Foo))
            .AdditionalProperties(false);
}
```

`MyModelSchema` can be either a field, as shown, or a property, but it must be public and static.

In this example, we've declared that any JSON that represents `MyModel` must be an object that contains a single string property named `Foo`, which must be between 10 and 50 characters long, and no other properties are permitted.

The final step is to decorate `MyModel` with the `[JsonSchema()]` attribute.

```c#
[JsonSchema(typeof(Schemas), nameof(Schemas.MyModelSchema))]
public class MyModel
{
    public string Foo { get; set; }
}
```

That's it.

## Why not use `System.ComponentModel.DataAnnotations` to annotate and validate the model? {#schema-deserialization-why}

`System.Text.Json` doesn't actually support these annotations.

***HINT** The unit tests for the converter demonstrate several of these scenarios.*

ASP.Net has separate code that can validate the model _after_ it's been deserialized, but that has several downsides.

### Different error sources {#schema-deserialization-error-sources}

Some errors are caught by the serializer, but others can only be caught by model validation.

If a property in the JSON doesn't have the right type, the serializer will catch it because it can't deserialize the value into the required type.

But the serializer can't catch if a value is simply out of an acceptable range or if a required property is missing.

Having these two sources of errors can be confusing to the user/client and difficult to maintain for coders.

Using a JSON Schema to validate _during_ deserialization ensures that all of the model validation occurs in one place.

### Lackluster error reporting {#schema-deserialization-better-errors}

When there are multiple errors, the deserializer or model validation is only going to report the first one it encounters.

With JSON Schema's output formats, you can get all of the errors with a single deserialization attempt.
