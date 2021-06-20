# Performing Custom Generation

Sometimes, you may need to have custom logic that changes the generated schema in a way that can't be fulfilled with Generators, Intents, or Attributes.

As an example, this library handles [nullability](../usage/schema-generation.md#nullability) outside of these mechanisms by making use of a _refiner_.

This example shows how this kind of custom logic can be accomplished.

It first looks at the generated schema to determine whether it can add a `null` to the `type` keyword.  To do this, it needs to look at a configuration option as well as a special `[Nullable(bool)]` attribute that is used to override the option.

```c#
internal class NullabilityRefiner : ISchemaRefiner
{
    public bool ShouldRun(SchemaGeneratorContext context)
    {
        // we only want to run this if the generated schema has a `type` keyword
        return context.Intents.OfType<TypeIntent>().Any();
    }

    public void Run(SchemaGeneratorContext context)
    {
        // find the type keyword
        var typeIntent = context.Intents.OfType<TypeIntent>().Firs();
        // determine if the property has an override attribute
        var nullableAttribute = context.Attributes.OfType<NullableAttribute>().FirstOrDefault();
        var nullabilityOverride = nullableAttribute?.IsNullable;

        // if there's an override, use it
        if (nullabilityOverride.HasValue)
        {
            if (nullabilityOverride.Value)
                typeIntent.Type |= SchemaValueType.Null;
            else
                typeIntent.Type &= ~SchemaValueType.Null;
            return;
        }

        // otherwise, look at the options to determine what to do
        if (context.Configuration.Nullability.HasFlag(Nullability.AllowForNullableValueTypes) &&
            context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            typeIntent.Type |= SchemaValueType.Null;

        if (context.Configuration.Nullability.HasFlag(Nullability.AllowForReferenceTypes) &&
            !context.Type.IsValueType)
            typeIntent.Type |= SchemaValueType.Null;
    }
}
```

Because this refiner is defined in the library, it's added automatically.  But to include your refiner in the generation process, you'll need to add it to the `Refiners` collection in the configuration options.

```c#
var configuration = new SchemaGeneratorConfiguration
{
    Refiners = {new MyRefiner()}
};
JsonSchema actual = new JsonSchemaBuilder().FromType<SomeType>(configuration);
```