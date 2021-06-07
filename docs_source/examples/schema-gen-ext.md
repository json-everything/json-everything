# Extending JSON Schema Generation

This example shows the requirements to implement various components of the JSON Schema generation system.

For a more detailed explanation about the concepts behind these examples, please see the [Schema Generation page](../usage/schema-generation.md).

## Generator

For this example, we will be implementing a custom generator that handles the `TimeSpan` type by creating a schema that expects a `duration`-formatted string.

```c#
class DateTimeSchemaGenerator : ISchemaGenerator
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

## Intent

This example shows how to implement a custom `ISchemaKeywordIntent` for a hypothetical [`maxDate` keyword](schema-vocabs.md#defining-a-keyword).

```c#
public class MaxDateIntent : ISchemaKeywordIntent
{
    // Define the data needed by the keyword.
    public DateTime Value { get; }

    public MaxDateIntent(decimal value)
    {
        Value = value;
    }

    // Implements ISchemaKeywordIntent
    // Given a builder, we're going to apply any keywords that we need to.
    public void Apply(JsonSchemaBuilder builder)
    {
        builder.Add(new MaxDateKeyword(Value));
    }

    // Equality stuff.
    // This is VERY important.  Implement as shown here.
    public override bool Equals(object obj)
    {
        return !ReferenceEquals(null, obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = GetType().GetHashCode();
            hashCode = (hashCode * 397) ^ Value.GetHashCode();
            return hashCode;
        }
    }
}
```

## Attributes

Now that we have the intent, we need a way to indicate that the keyword should apply to a given property.  For value validation keywords like `maxDate`, these are best applied as attributes.

```c#
// The system currently only supports attributes on properties.
[AttributeUsage(AttributeTargets.Property)]
public class MaxDateAttribute : Attribute, IAttributeHandler
{
    // Again, we need the keyword's value.
    public DateTime Value { get; }

    public MaxDateAttribute(uint value)
    {
        Value = value;
    }

    // It's not necessary to implement this explicitly, but I like to.
    void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
    {
        var attribute = context.Attributes.OfType<MaxDateAttribute>()
                                          .FirstOrDefault();
        // Protect against this being run when the attribute isn't applied.
        if (attribute == null) return;

        // Ensure the property is a date; otherwise this requirement
        // doesn't apply.
        if (!context.Type == typeof(DateTime)) return;

        // Add the intent.
        context.Intents.Add(new MaxDateIntent(attribute.Value));
    }
}
```