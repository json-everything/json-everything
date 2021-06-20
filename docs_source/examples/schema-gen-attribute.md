# Using Attributes to Add Constraints

In the [previous example](schema-gen-intent.md) we created a keyword intent to represent a new `maxDate` keyword during generation.

Now we need a way to add it into the generation.  This keyword is a validation constraint that you might expect to see as an attribute, so we'll do that.

For simplicity, the attribute itself will handle adding the keyword.  The generation system actually looks for attributes that also implement `IAttributeHandler`, so this is a requirement.

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

Now we can apply this attribute to a property and a `maxDate` keyword will be added to the schema.
