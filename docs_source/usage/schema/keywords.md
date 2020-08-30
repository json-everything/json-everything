# Defining Custom Keywords

`JsonSchema` has been designed to allow you to create your own keywords.  There are several steps that need to be performed to do this.

1. Implement `IJsonSchemaKeyword`.
1. Optionally implement `IRefResolvable` if your keyword contains a schema as a value.
1. Apply some attributes.
1. Register the keyword.
1. Create a JSON converter.

And your new keyword is ready to use.

Lastly, remember that the best resource building keywords is [the code](https://github.com/gregsdennis/json-everything/tree/master/JsonSchema).

## 1. Implement `IJsonSchemaKeyword`

This defines the `Validate()` method.  Implement your validation logic.

### The `ValidationContext`

The validation context contains all of the data that you need to perform the validation:

- root schema
- current keyword's location relative to the schema root
- local schema
- instance root
- current instance location relative to the instance root
- local instance
- current schema's URI
- current schema's anchor label (e.g. `#label`)
- context objects from sibling keywords that have already validated
- annotations from sibling keywords that have already been validated

Once you have validated the instance, you'll need to record the results.

- `IsValid` - Indicates whether the validation succeeded.  This property must be set.
- `NestedContexts` - This contains any context objects you created and used in order to validate the instance.  This is typically used when your keyword contains a schema (e.g. `allOf` or `contains`).
- `Ignore` - Set this to exclude these results from the output.  This used for keywords such as `$id` or `$defs`.  You probably won't need to set this.
- `Message` - Set this to return an error message.

If your keyword contains a schema, you'll need to create a context to validate it.  To do this, use the `.From()` method.  This will copy a context while allowing you to update some of the properties.

- instance location
- instance
- subschema location

Not all of these need to be updated, however.  Most keywords focus on the instance that is passed to them, however some, like `properties` need to build contexts for each property.  To do this, it [updates all of these properties](https://github.com/gregsdennis/json-everything/blob/master/JsonSchema/PropertiesKeyword.cs#L64-L67).

If the instance passed validation, set any annotations by using `.SetAnnotation()`.  This is stored as a key-value pair.  The convention is to use the keyword name as the key.

### Annotation consolidation

With the introduction of keywords like `unevaluatedItems` and `unevaluatedProperties` from draft 2019-09, it's now necessary for applicator keywords (e.g. `anyOf`) to aggregate annotations.  To do this, some keywords define a consolidation method.

To have this consolidation method called, it needs to be registered.  This can be done via the `.RegisterConsolidationMethod()` method.

If you're implementing an applicator keyword, you'll need to invoke this consolidation before returning.  To this this call `.ConsolidateAnnotations()` on the context.

## 2. Implement `IRefResolvable`

If your keyword contains a schema, you'll want to implement this.  It allows `JsonSchema` to search within the keyword when resolving `$ref` and `$recursiveRef`.

For `.ResolvePointerSegment()`, if the pointer segment matches a path where a schema might be found, return it.  Otherwise return null.

For `.RegisterSubschemas()`, just use this:

```c#
void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
{
	Schema.RegisterSubschemas(registry, currentUri);
}
```

## 3. Apply some attributes

JsonSchema<nsp>.Net contains several attributes that you should use to specify some metadata about your keyword.

- `SchemaKeyword` - Defines the keyword as it appears in the schema.
- `SchemaPriority` - Defines a priority that will be used to order keyword validation properly.  Keywords with the same priority are validated in the order they appear in the schema.
- `SchemaDraft` - Declares a draft that supports the keyword.  This can be used multiple times to declare additional drafts.
- `Vocabulary` - Declares the ID of the vocabulary which defines the the keyword.
- `Applicator` - Identifies the keyword as an applicator.  This signals `JsonSchema` that this node must be kept when building the output structure.

## 4. Register your keyword

To make JsonSchema<nsp>.Net aware of your keyword, you must register it with `SchemaKeywordRegistry.Register<T>()`.  This will enable deserialization.

## Now make it nice to use

To enable the fluent construction interface for your keyword, simply create an extension method on `JsonSchemaBuilder` that adds the keyword and returns the builder.  For example, adding a `description` keyword is implemented by this method:

```c#
public static JsonSchemaBuilder Description(this JsonSchemaBuilder builder, string description)
{
    builder.Add(new DescriptionKeyword(description));
    return builder;
}
```

## 5. Create a JSON converter

To enable serialization and deserialization, you'll need to provide the converter for it.

Implement a `JsonConverter<T>` and apply a `JsonConverter` attribute to the keyword.