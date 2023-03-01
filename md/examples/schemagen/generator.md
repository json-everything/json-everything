# Generating a Schema for a Simple Type

This example shows how to extend schema generation to cover a specific type that isn't defined by the type's properties.  This is useful for many of the scalar-like value types, such as `bool`, `int`, `DateTime`, or `TimeSpan`.

A _generator_ is the appropriate tool for this job.

For this example, we will be implementing a custom generator that handles the `TimeSpan` type by creating a schema that expects a `duration`-formatted string.

```c#
// Generators must implement ISchemaGenerator
class TimeSpanSchemaGenerator : ISchemaGenerator
{
    public bool Handles(Type type)
    {
        return type == typeof(TimeSpan);
    }

    public void AddConstraints(SchemaGeneratorContext context)
    {
        context.Intents.Add(new TypeIntent(SchemaValueType.String));
        context.Intents.Add(new FormatIntent(Formats.Duration));
    }
}
```

Once the generator is complete, we must register an instance:

```c#
GeneratorRegistry.Register(new TimeSpanSchemaGenerator());
```

All done.  Generation can now handle `TimeSpan`s.
