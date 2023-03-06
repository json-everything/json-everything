# Supporting a New Keyword During Generation {#example-schemagen-intent}

This example shows how to extend schema generation to output a new keyword.

Suppose we've implemented the `maxDate` keyword (see the "Custom Vocabularies" example).  Now we need a way to generate schemas that contain it.

For this we need to create a _keyword intent_.

```c#
public class MaxDateIntent : ISchemaKeywordIntent
{
    // Define the data needed by the keyword.
    public DateTime Value { get; set; }

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

In this case, our intent will be applied by an attribute, but it could also be applied within a generator.
