# Vocabularies

JSON Schema draft 2019-09 introduced the idea of vocabularies to enable some spec support for custom keywords.

A vocabulary is just a collection of keywords.  It will be identified by a URI and should have an associated specification that describes the function of each of the keywords.  There *may* also be an associated meta-schema.

Creating a vocabulary in JsonSchema.Net isn't strictly required in order to add custom keywords, but if you're using it to create a meta-schema that will consume and validate other draft 2019-09 or later schemas, it is strongly suggested.

## How vocabularies work

This is best explained with an example.  Suppose we have a meta-schema **M**, a schema **S** that uses **M** as its `$schema`, and a couple instances **I1** and **I2** to be validated by **S**.

```json
// meta-schema M
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",               // 1
  "$id": "https://myserver.net/meta-schema",
  "$vocabulary": {
    "https://json-schema.org/draft/2020-12/vocab/core": true,              // 2
    "https://json-schema.org/draft/2020-12/vocab/applicator": true,
    "https://json-schema.org/draft/2020-12/vocab/validation": true,
    "https://json-schema.org/draft/2020-12/vocab/meta-data": true,
    "https://json-schema.org/draft/2020-12/vocab/format-annotation": true,
    "https://json-schema.org/draft/2020-12/vocab/content": true,
    "https://json-schema.org/draft/2020-12/vocab/unevaluated": true,
    "https://myserver.net/my-vocab": true
  },
  "allOf": [                                                                // 3
    { "$ref": "https://json-schema.org/draft/2020-12/schema" }
  ],
  "properties": {
    "minDate": {                                                            // 4
      "type": "string",
      "format": "date"
    }
  }
}

// schema S
{
  "$schema": "https://myserver.net/meta-schema",                           // 5
  "$id": "https://myserver.net/schema",
  "properties": {
    "publishedOnDate": {
      "minDate": "2019-01-01"                                               // 6
    }
  }
}

// instance I1
{
  "publishedOnDate": "2019-06-22"                                           // 7
}
// instance I2
{
  "publishedOnDate": "1998-06-22"                                           // 8
}
```

1. We declare a meta-schema.  In this case, it's the draft 2020-12 meta-schema.  This will validate our schema and declare the set of rules it should be processed with.
2. We list the vocabularies that the JsonSchema.Net should know about in order to process schemas that declare this meta-schema as their `$schema` (see #5).  This includes all of the vocabularies from 2020-12 (because we want all of the 2020-12 capabilities) as well as the vocab for this meta-schema.  We'll explain a bit more about this later.
3. We also need all of the syntactic validation from 2020-12, so we include it in an `allOf`.
4. We define a new keyword, `minDate`, that takes a date-formatted string value.
5. We create a schema that uses our new meta-schema (because we want to use the new keyword).
6. We use the new keyword to define a property to be found in the instance.
7. The first instance defines a date after the minimum required date.
8. The second date defines a date before the minimum required date.

The kicker here is that we can read "minDate" and know that its value represents the minimum acceptable date... because we're human, and we understand things like that.  However, a validator isn't going to be able to infer that.  It can only validate that a date-formatted string was given for `minDate` in the schema (**S**).

That's where the vocabulary comes in.  The vocabulary is a human-readable document that gives *semantic* meaning to `minDate`.  It is documentation of business logic that allows a programmer to code an extension that provides additional validation.  For example, this is the documentation for `minLength` in the Schema Validation specification:

> **6.3.2. minLength**
>
> The value of this keyword MUST be a non-negative integer.
>
> A string instance is valid against this keyword if its length is greater than, or equal to, the value of this keyword.
>
> The length of a string instance is defined as the number of its characters as defined by RFC 8259.
>
> Omitting this keyword has the same behavior as a value of 0.

It gives meaning to the keyword beyond how the meta-schema describes it: a non-negative integer.

Any validator can validate that `minDate` is a date-formatted string, but only a validator that understands `https://myserver.net/my-vocab` as a vocabulary will understand that `minDate` should validate that a date in the instance should be later than that in the schema.

Now, if you look at the `$vocabulary` entry for `https://myserver.net/my-vocab`, the vocabulary has its ID as the key with a boolean value.  In this case, that value is `true`.  That means that if JsonSchema.Net *doesn't* know about the vocabulary, it **must** refuse to process any schema that declares **M** as its `$schema` (as **S** does).  If this value were `false`, then JsonSchema.Net would be allowed to continue, which means that only syntactic analysis (i.e. "Is `minDate` a date-formatted string?") would be performed.

So, back to the example, because we declare the vocabulary to be required (by giving it a value of `true`) *and* because JsonSchema.Net knows about it, **I1** is reported as valid and **I2** is not.  If the vocabulary had not been required _and_ JsonSchema.Net didn't know about the vocabulary, both **I1** and **I2** would be reported as valid because the `minDate` keyword would not have been enforced.

## Registering a vocabulary

To tell JsonSchema.Net about a vocabulary, you need to create a `Vocabulary` instance and register it using `VocabularyRegistry.Add<T>()`.

The `Vocabulary` class is quite simple.  It defines the vocabulary's ID and lists the keywords which it supports.

The keywords must be registered separately (see "Defining Custom Keywords" below).

It's not always necessary to have a meta-schema for your vocabulary.  However, if you want to enable `ValidationOptions.ValidateMetaschema`, you will need to register it.

# Defining Custom Keywords

`JsonSchema` has been designed to allow you to create your own keywords.  There are several steps that need to be performed to do this.

1. Implement `IJsonSchemaKeyword`.
1. Optionally implement `IRefResolvable` if your keyword contains a schema as a value.
1. Apply some attributes.
1. Register the keyword.
1. Create a JSON converter.

And your new keyword is ready to use.

Lastly, remember that the best resource building keywords is [the code](https://github.com/gregsdennis/json-everything/tree/master/JsonSchema) where all of the built-in keywords are defined.

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
- instance\*
- subschema location
- subschema
- new base URI

Not all of these need to be updated, however.  Most keywords focus on the instance that is passed to them, however some keywords, like `properties`, need to provide updates for each subschema.  To do this, it [updates several of these properties](https://github.com/gregsdennis/json-everything/blob/master/JsonSchema/PropertiesKeyword.cs#L75-77).

\* _If the instance is changing to a JSON null value, it's important to pass `JsonNull.SignalNode` here instead of merely a null value.  This way the system knows explicitly that a JSON null is being passed rather than merely not being provided.  The context will still save a null value, however.  `JsonNull.SignalNode` is merely a signal._

If the instance passed validation, set any annotations by using `.SetAnnotation()` on the local result object.  This is stored as a key-value pair.  The convention is to use the keyword name as the key.  The value can be anything, but it _should_ be JSON-serializable in order to be rendered properly in the output.

### Annotation consolidation

With the introduction of keywords like `unevaluatedItems` and `unevaluatedProperties` from draft 2019-09, it's now necessary for applicator keywords (e.g. `anyOf`) to aggregate annotations.  To do this, some keywords define a consolidation method.

To have this consolidation method called, it needs to be registered.  This can be done via the `.RegisterConsolidationMethod()` method.

If you're implementing an applicator keyword, you'll need to invoke this consolidation before returning.  To this this call `.ConsolidateAnnotations()` on the context.

## 2. Implement `IRefResolvable`

If your keyword contains one or more subschemas, you'll want to implement this.  It allows `JsonSchema` to search within the keyword when resolving `$ref`, `$recursiveRef`, and `$dynamicRef`.

For `.ResolvePointerSegment()`, if the pointer segment matches a path where a schema might be found, return it.  Otherwise return null.

For `.RegisterSubschemas()`, just use this:

```c#
void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
{
	Schema.RegisterSubschemas(registry, currentUri);
}
```

The internals of this library handle everything, but they need support via a callback in the keyword.

## 3. Apply some attributes

JsonSchema.Net contains several attributes that you should use to specify some metadata about your keyword.

- `SchemaKeyword` - Defines the keyword as it appears in the schema.
- `SchemaPriority` - Defines a priority that will be used to order keyword validation properly.  Keywords with the same priority are validated in the order they appear in the schema.
- `SchemaDraft` - Declares a draft that supports the keyword.  This can be used multiple times to declare additional drafts.
- `Vocabulary` - Declares the ID of the vocabulary which defines the the keyword.
- `Applicator` - Identifies the keyword as an applicator.  This signals `JsonSchema` that this node must be kept when building the output structure.

## 4. Register your keyword

To make JsonSchema.Net aware of your keyword, you must register it with `SchemaKeywordRegistry.Register<T>()`.  This will enable deserialization.

### Now make it nice to use

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
