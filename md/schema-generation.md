# Generating JSON Schema from .Net Types

JsonSchema.Net.Generation is an extension package to [JsonSchema.Net](https://www.nuget.org/packages/JsonSchema.Net/) that provides JSON Schema generation from .Net types.

Using it is quite simple.  First you need a `JsonSchemaBuilder`.  Then...

```c#
var schema = schemaBuilder.FromType<MyType>().Build();
```

Done.

***NOTE** Ideally, this functionality should be used to create a starting point in authoring a schema.  The schemas output by this library should be reviewed by actual people prior to being put into a production system.*

## Making better schemas

The above will give you a basic schema that will include the `type` keyword, and maybe a listing of the properties.  But many times you have more detailed requirements of the data you're receiving.  For example,

- some properties may be required
- string properties may have length requirements
- numeric properties may have value range requirements

All of these and more are supplied via a set of attributes that can be applied to properties.  The following attributes are included in this package:

- Numeric values
    - `Minimum`
    - `ExclusiveMinimum`
    - `Maximum`
    - `ExclusiveMaximum`
    - `MultipleOf`
- Strings
    - `MinLength`
    - `MaxLength`
    - `Pattern`
- Arrays
    - `MinItems`
    - `MaxItems`
    - `UniqueItems`
- All
    - `Required` & `Nullable` (see below)
    - `Obsolete`\* (translates to `deprecated`)
    - `JsonExclude`\*\*
    - `Title`
    - `Description`
    - `ReadOnly`
    - `WriteOnly`

\* The `[Obsolete]` attribute is `System.Obsolete`.  All of the others have been defined within this library.  `System.ComponentModel.DataAnnotations` support is currently [in discussion](https://github.com/gregsdennis/json-everything/issues/143).

\*\* The `[JsonExclude]` attribute functions equivalently to `[JsonIgnore]` (see below).  It is included to allow generation to skip a property while allowing serialization to consider it.

Simply add the attributes directly to the properties and the corresponding keywords will be added to the schema.

For properties typed with generic collections, like `List<T>`, the schema will automatically generate an `items` keyword and an generate a schema for the indicated `T`.  If your `T` is a numeric value or a string, then you can also apply the relevant attributes and they'll be applied in the `items` subschema.

For example, this object:

```c#
class MyClass
{
    [UniqueItems(true)]
    [Minimum(10)]
    public List<int> MyList{ get; set; }
}
```

will be translated to this schema:

```json
{
  "type": "object",
  "properties": {
    "MyList": {
      "type": "array",
      "items": {
        "type": "integer",
        "minimum": 10
      },
      "uniqueItems": true
    }
  }
}
```

The `minimum` is applied to the `items` because that keyword is not relevant for an array.

***NOTE** This means that the generator will have trouble determining where to apply keywords to properties like `List<List<T>>` because the attributes could be relevant for both the outer and inner lists.*

The generator also supports these .Net-defined attributes:

- `JsonPropertyName` - supports custom property naming (more on naming below)
- `JsonNumberHandling`\* - supports allowing numeric values in strings or only as numbers as well as allowing the `NaN`, `Infinity`, and `-Infinity` values.
- `JsonIgnore`\* - ignores a property

\* These attributes were introduced with .Net 5.  The .Net Standard version of the library also provides a definition for them.

The generator will handle most common types:

- numeric types (`int`, `decimal`, etc.)
- `bool`
- `string`
- enumerations (mapped to strings)
- `Uri`
- `JsonPointer` (from [JsonPointer.Net](https://www.nuget.org/packages/JsonPointer.Net/))
- `Guid`
- `DateTime`
- collections (`IEnumerable<T>`)
- string-keyed dictionaries
- enumeration-keyed dictionaries (keys are mapped to strings)
- POCOs

For POCOs, currently only read/write properties are converted.  Future versions of this library may also support read-only or write-only by adding the `readOnly` and `writeOnly` keywords, respectively.

Lastly, property names will either be listed as declared in code (default) or sorted by name.  This is controlled via the `SchemaGenerationConfiguration.PropertyOrder` property.

### Nullability

There is a discrepancy between how .Net does validation and how JSON Schema does validation that centers primarily around nullable types and the `[Required]` attribute.

Those familiar with .Net validation will recognize that having `[Required]` on your models will also protect against null values when deserializing.  However, JSON Schema separates these two concepts, and this library strives to align with JSON Schema in order to give the most flexibility.

To this end, the `[Required]` attribute will only be represented in generated schemas in the `required` keyword.

However, for nullable types, it may or may not be appropriate to include `null` in the `type` keyword.  JsonSchema.Net.Generation controls this behavior via the `SchemaGenerationConfiguration.Nullability` option with individual properties being overrideable via the `[Nullable(bool)]` attribute.

There are four options:

- `Disabled` - This is the default.  The resulting schemas will not have `null` in the `type` keyword unless `[Nullable(true)]` is used.
- `AllowForNullableValueTypes` - This will add `null` to the `type` keyword for nullable value types (i.e. `Nullable<T>`) unless `[Nullable(false)]` is used.
- `AllowForReferenceTypes` - This will add `null` to the `type` keyword for reference types unless `[Nullable(false)]` is used.
- `AllowForAllTypes` - This is a combination of the previous two and will add `null` to the type keyword for any type unless `[Nullable(false)]` is used.

***NOTE** This library [cannot detect](https://stackoverflow.com/a/62186551/878701) whether the consuming code has nullable reference types enabled.  Therefore all reference types are considered nullable.*

***BONUS NOTE** The library makes a distinction between nullable value types and reference types because value types must be explicitly nullable.  This differs from reference types which are implicitly nullable, and there's not a way (via the type itself) to make a reference type non-nullable.*

### Property naming

In addition to the `[JsonPropertyName]` attribute, the configuration exposes a `PropertyNamingMethod` that allows you to define a custom method for altering property names from your code into the schema.  The `PropertyNamingMethods` static class defines a few commonly-used conventions:

- `camelCase`
- `PascalCase`
- `kebab-case`
- `UPPER-KEBAB-CASE`
- `snake_case`
- `UPPER_SNAKE_CASE`

Just set this property and the system will adjust property names accordingly.

Note that the `[JsonPropertyName]` attribute takes precedence, so you can still use this to get custom naming when you've configured a method.

### Additional built-in support

There are a couple advanced features that bear mentioning.

1. The system does have some loop detection logic in order to support self-referencing or loop-referencing types.
1. The system will collect common subschemas into a `$defs` keyword at the root.  Identification of a subschema is by its type and the collection of attributes it is processed with.  The locations with these common subschemas will be replaced by a `$ref` that points to the appropriate entry in `$defs`.

## Extending support

The above will work most of the time, but occasionally you may find that you need some additional support.  Happily, the library is configured for you to provide that support yourself.

There are four areas that can be utilized in order to get the results you're after.

- Generators
- Intents
- Attributes
- Refiners

These do not _all_ need to be implemented.

### Generators

These are the first phase of generation.  When encountering a type, the system will find the first registered generator that can handle that type.  The generator then creates keyword intents (see [Intents](#intents) below).  The supported types list above is merely a list of the built-in generators.

To create a new generator, you'll need to implement the `ISchemaGenerator` interface and register it using the `GeneratorRegistry.Register()` static method.  This will insert your generator at the top of the list so that it has priority.

***IMPORTANT** This means that the order your generators are registered is important: last one wins.  So if you want one generator to have priority over another, register the higher priority one last.*

This class doesn't need to be complex.  Here's the implementation for the `BooleanSchemaGenerator`:

```c#
internal class BooleanSchemaGenerator : ISchemaGenerator
{
    public bool Handles(Type type)
    {
        return type == typeof(bool);
    }

    public void AddConstraints(SchemaGeneratorContext context)
    {
        context.Intents.Add(new TypeIntent(SchemaValueType.Boolean));
    }
}
```

Very simple.  It says that it handles booleans and then it does.

To explain _how_ it does, we need to discuss intents.

### Intents

The `JsonSchema` type and its keywords are immutable.  So even if we _could_ get to the keyword list inside the `JsonSchemaBuilder`, we wouldn't be able to edit the keywords to perform optimizations.

Happily, this has led to a new type that mimics keywords to hold their data until the keywords are ready to be built.  That new type is the keyword intent, manifested in this library by the `ISchemaKeywordIntent` interface.

The intent is keyword-specific and holds the data needed to actually build the keyword.

Once the system has analyzed the type and collected all of the intents, it extracts any common subschemas into a `$defs` intent, then applies them to the builder that was provided at the beginning of the process.  This application of the intents generates the keywords.

Most intents are pretty simple to implement.  Here's the `TypeIntent` from above:

```c#
public class TypeIntent : ISchemaKeywordIntent
{
    public SchemaValueType Type { get; set; }

    public TypeIntent(SchemaValueType type)
    {
        Type = type;
    }

    public void Apply(JsonSchemaBuilder builder) => builder.Type(Type);

    public override bool Equals(object obj) => !ReferenceEquals(null, obj);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = GetType().GetHashCode();
            hashCode = (hashCode * 397) ^ Type.GetHashCode();
            return hashCode;
        }
    }
}
```

See?  The `Apply()` method just takes the builder, and adds a keyword with the data that it already collected.  Pretty easy.

Note that the equality methods are overridden.  This is ***extremely*** important as it's a pivotal part of how the optimization works.  You **must** override these methods, and it is strongly advised that you do it just like this.

This will work for most intents, but some keywords contain subschemas.  For these, we don't want to hold a subschema because, as mentioned before, they can't be edited.  Instead, we'll hold a context object that represents the subschema: its type, attribute set, and the intents required to build it.  For these intents, we *also* want to implement `IContextContainer`.  Here's the `ItemsIntent`:

```c#
public class ItemsIntent : ISchemaKeywordIntent, IContextContainer
{
    public SchemaGeneratorContext Context { get; private set; }

    public ItemsIntent(SchemaGeneratorContext context)
    {
        Context = context;
    }

    public IEnumerable<SchemaGeneratorContext> GetContexts() => new[] {Context};

    public void Replace(int hashCode, SchemaGeneratorContext newContext)
    {
        var hc = Context.GetHashCode();
        if (hc == hashCode)
            Context = newContext;
    }

    public void Apply(JsonSchemaBuilder builder) => builder.Items(Context.Apply());

    public override bool Equals(object obj) => !ReferenceEquals(null, obj);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = GetType().GetHashCode();
            hashCode = (hashCode * 397) ^ Context.GetHashCode();
            return hashCode;
        }
    }
}
```

There are two methods required by `IContextContainer`: `GetContexts()` and `Replace()`.

`GetContexts()` gets the context objects *directly* held by this intent.

***IMPORTANT** Don't dive into the context's `Intents` collection and get the contexts that those hold also.  The system will handle that.*

`Replace()` replaces a context with a given hash code with a new context.  This is the system creating `$ref` intents that point to the new `$defs` intent it's building and distributing them throughout the context tree.  Once all the `$ref`s are distributed, the system will add the `$defs` intent to the root context to be applied at the last step.

Generally intents for applicator keywords, which are keywords that have subschemas (`anyOf`, `allOf`, etc.), will need to implement this second interface.  In most cases, you can just copy this code.

### Attributes

The other source for intents are attributes.  These are handled once the generator has completed adding the intents it needs to.

When processing an object, the properties are analyzed for the presence of any of the above attributes.  Each of the attributes can then add its intents to the context.

To create your own attribute and have it processed by the system, it'll need to implement `IAttributeHandler` as well.

The attribute itself is pretty simple.  It's just a class that inherits from `Attribute`, implements `IAttributeHandler`, and carries some data.  Here's `MaximumAttribute`:

```c#
[AttributeUsage(AttributeTargets.Property)]
public class MaximumAttribute : Attribute, IAttributeHandler
{
    public uint Value { get; }

    public MaximumAttribute(uint value)
    {
        Value = value;
    }

    void IAttributeHandler.AddConstraints(SchemaGeneratorContext context)
    {
        var attribute = context.Attributes.OfType<MaximumAttribute>().FirstOrDefault();
        if (attribute == null) return;

        if (!context.Type.IsNumber()) return;

        context.Intents.Add(new MaximumIntent(attribute.Value));
    }
}
```

The `AddConstraints()` method works exactly the same as in the generator class.  A key difference here is that you will need to guard against unrelated types.  For instance, with `MaximumAttribute` above, it doesn't make sense to have `maximum` on a non-numeric type, so we check it before adding the intent.

***NOTE** `.IsNumber()` is an extension method on `Type` that determines if it's a numeric type.  There are a few more of these helper extensions as well.*

The occasion may arise where you want to handle an attribute that's defined in some other assembly, and you can't make it implement `IAttributeHandler`.  For these cases, just implement the handler class, and then add it using one of the `AttributeHandler.AddHandler()` static methods.  A handler can be removed using the `AttributeHandler.RemoveHandler<T>()` static method.

### Refiners

Sometimes you may need to make minor adjustments to the generated schemas dynamically.  For this you'll need to create an implementation of `ISchemaRefiner`.

Refiners are called after all intents have been generated for each type, recursively, throughout the process.

To implement a refiner, two methods will be needed:

- `bool ShouldRun(SchemaGenerationContext)` which determines whether the refiner needs to run for the current generation iteration.
- `void Run(SchemaGenerationContext)` which makes whatever modifications are needed.

Remember that a this point, you're stil working with intents.  You can add new ones as well as modify or remove existing ones.  You really have complete freedom within a refiner.

## That's it

That should get you generating schemas for all of your types.  If you come up with ideas or implement your own extensions and think they'd be handy if they were built in, feel free to open up an issue or a pull request.
