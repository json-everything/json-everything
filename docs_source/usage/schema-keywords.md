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

- validation options (set by caller)
- root schema
- current keyword's location relative to the schema root
- local schema
- instance root
- current instance location relative to the instance root
- local instance
- current schema's URI
- current schema's anchor label (e.g. `#label`)
- local result object

Once you have validated the instance, you'll need to record the results.  These methods are available on the local result object.

- `Pass()` - Sets a successful validation.
- `Fail(string? message)` - Sets a failed validation along with an optional error message.
- `Ignore()` - Marks the resultss from this keyword to be excluded from the output.  This used for keywords such as `$defs`.  You probably won't need to set this.

If your keyword contains one or more subschemas, you may need to push a new context onto the stack.  This takes the data from the current context and applies changes based on the optional inputs.  To do this, use the `.Push()` method, which will allow you to update some of the properties.

- instance location
- instance
- subschema location
- subschema
- new base URI

Not all of these need to be updated, however.  Most keywords focus on the instance that is passed to them, however some keywords, like `properties`, need to provide updates for each subschema.  To do this, it [updates several of these properties](https://github.com/gregsdennis/json-everything/blob/master/JsonSchema/PropertiesKeyword.cs#L75-77).

If the instance passed validation, set any annotations by using `.SetAnnotation()` on the local result object.  This is stored as a key-value pair.  The convention is to use the keyword name as the key.  The value can be anything, but it _should_ be JSON-serializable in order to be rendered properly in the output.

### Annotation consolidation

With the introduction of keywords like `unevaluatedItems` and `unevaluatedProperties` from draft 2019-09, it's now necessary for applicator keywords (e.g. `anyOf`) to aggregate annotations.  To do this, some keywords define a consolidation method.

To have this consolidation method called, it needs to be registered.  This can be done via the `.RegisterConsolidationMethod()` method.

If you're implementing an applicator keyword, you'll need to invoke this consolidation before returning.  To this this call `.ConsolidateAnnotations()` on the context.

## 2. Implement `IRefResolvable`

If your keyword contains one or more subschemas, you'll want to implement this.  It allows `JsonSchema` to search within the keyword when resolving `$ref`, `$recursiveRef`, and `$dynamicRef`.

As of v2.1.2, `.ResolvePointerSegment()` is obsolete.  You can leave this as throwing a `NotImplementedException`.  It won't be called.  This method will be removed in the next major version.

For `.RegisterSubschemas()`, just use this:

```c#
void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
{
	Schema.RegisterSubschemas(registry, currentUri);
}
```

The internals of this library handle everything, but they need support via a callback in the keyword.

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