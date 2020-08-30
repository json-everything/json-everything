# Extending schemas

## Vocabularies

JSON Schema draft-08 has introduced the idea of vocabularies to enable some spec support for custom keywords.

A vocabulary is a collection of keywords.  It will be identified by a URI and should have an associated specification that describes the function of each of the keywords.  There _may_ also be an associated meta-schema.

Because Manatee.Json supported extending schemas with custom keywords before vocabularies were introduced, creating a vocabulary isn't strictly required.  But if you're using it to consume and validate draft-08 schemas, it is strongly suggested.

This is best explained with an example.  Suppose we have a meta-schema **M**, a schema **S** that uses **M** as its `$schema`, and a couple instances **I1** and **I2** to be validated by **S**.

```json
// meta-schema M
{
  "$schema": "https://myserver.net/meta-schema#",                           // 1
  "$id": "https://myserver.net/meta-schema#",
  "$vocabulary": {
    "https://json-schema.org/draft/2019-WIP/vocab/core": true,              // 2
    "https://json-schema.org/draft/2019-WIP/vocab/applicator": true,
    "https://json-schema.org/draft/2019-WIP/vocab/validation": true,
    "https://json-schema.org/draft/2019-WIP/vocab/meta-data": true,
    "https://json-schema.org/draft/2019-WIP/vocab/format": true,
    "https://json-schema.org/draft/2019-WIP/vocab/content": true,
    "https://myserver.net/my-vocab": true
  },
  "allOf": [                                                                // 3
    { "$ref": "https://json-schema.org/draft/2019-06/schema#" }
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
  "$schema": "https://myserver.net/meta-schema#",                           // 5
  "$id": "https://myserver.net/schema#",
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

1. We declare a meta-schema.  The meta-schema should validate itself, so we declare `$schema` to be the same as `$id`.
2. We list the vocabularies that the Manatee.Json should know about in order to process schemas that declare this meta-schema as their `$schema` (see #5).  This includes all of the vocabularies from draft-08 (because we want all of the draft-08 capabilities) as well as the vocab for this meta-schema.  We'll explain a bit more about this later.
3. We also need all of the syntactic validation from draft-08, so we include it in an `allOf`.
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

Now, if you look at the `$vocabulary` entry for `https://myserver.net/my-vocab`, the vocabulary has its ID as the key with a boolean value.  In this case, that value is `true`.  That means that if Manatee.Json *doesn't* know about the vocabulary, it **must** refuse to process any schema that declares **M** as its `$schema` (as **S** does).  If this value were `false`, then Manatee.Json would be allowed to continue, which means that only syntactic analysis (i.e. "Is `minDate` a date-formatted string?") would be performed.

So, back to the example, because we declare the vocabulary to be required (by giving it a value of `true`) *and* because Manatee.Json knows about it, **I1** is reported as valid and **I2** is not.  If the vocabulary had not been required _and_ Manatee.Json didn't know about the vocabulary, both **I1** and **I2** would be reported as valid because the `minDate` keyword would not have been enforced.

### Registering a vocabulary

To tell Manatee.Json about a vocabulary, you just need to create a `SchemaVocabulary` instance, return it [from a new keyword](#Vocabulary), and register the new keyword using `SchemaKeywordCatalog.Add<T>()`.  The vocabulary will automatically register.

The `SchemaVocabulary` class is quite simple.  It just links the vocabulary URI to the associated meta-schema URI.  These URIs are both required in the constructor.

If you have multiple keywords defined by the same vocabulary, *you need to add them all*.  Manatee.Json learns about vocabularies via the keywords that are added.  There is currently no mechanism defined by JSON Schema that makes all of the keywords defined by a vocabulary known to the implementation.  (This is functionality that will likely come in later drafts as a machine-readable vocabulary document.)

## Write a keyword

`JsonSchema` has been designed to allow you to create your own keywords.  There are several steps that need to be performed to do this.

1. Implement `IJsonSchemaKeyword`.
1. Add the keyword implementation to the catalog.
1. ... um... yeah, I guess that's it.

This is what you'll need to implement:

### `Name`

This property returns the keyword's string form (e.g. `$id` or `maximum`)

### `SupportedVersions`

This property returns the official draft versions that are supported by this keyword.  For example, if you want to support all of the official schema drafts, you can return `JsonSchemaVersion.All`, but if you only want draft-06 and draft-07 supported, you can return `JsonSchemaVersion.Draft06 | JsonSchemaVersion.Draft07`.

### `ValidationSequence`

This property becomes important when you have several keywords that must be evaluated in a specific order.  A prime example of this are the `properties`, `patternProperties`, and `additionalProperties` (and soon `unevaluatedProperties` from draft-08).  These keywords must be evaluated in that order.  Generally, if you are creating an independent keyword (as most of them should be), this can simply return 1.

### `Vocabulary`

If the keyword is to be supported as part of JSON Schema draft-08, then this should return the `SchemaVocabulary` object that defines the keyword.  Otherwise it should just return `SchemaVocabularies.None`.

### `RegisterSubschemas(Uri)`

The validation logic is performed iteratively.  This means that it can't  look forward when resolving `$ref` keywords (e.g. if the `definitions` keyword appears at the end of the schema).  To account for this, each schema will do an initialization pass when validating for the first time.  This ensures that any subschemas have been registered prior to any attempt at `$ref` resolution.

If your keyword does not contain a schema as part of its value, then this method can be a no-op.  Otherwise, simply call `JsonSchema`'s corresponding `RegisterSubschemas(Uri)` method and just pass the parameter.  `JsonSchema` will take care of the rest.

### `ResolveSubschema(JsonPointer, Uri)`

This method is called during `$ref` resolution.  If your keyword doesn't contain a schema, just return null.  Otherwise, simply pass the call onto `JsonSchema`'s corresponding `ResolveSubschema(JsonPointer, Uri)` method.

### `Validate(JsonSchemaContext)`

This is the guts of the keyword, where all of the actual validation takes place.

#### The validation context

The context fulfills two purposes: provide all of the required validation information to the keyword, and provide all of the location information needed to report any annotations and errors.  Below is a breakdown of all of the properties on the context and their roles in validation.

- `Local` - This property indicates the local schema that is being validated.  It's automatically set by the `JsonSchema` class, so you don't ever need to set this or modify it.
- `Root` - This is the root schema.  This is set at the beginning of the validation process and should never be updated.
- `RecursiveAnchor` - This defines the schema that is currently pointed to by the `$recursiveAnchor` and `$recursiveRoot` keywords.
- `Instance` - This is the instance being validated *at the current level*.  This may be a nested value inside the original instance.
- `EvaluatedPropertyNames` - This is a list of property names that have been validated by either your keyword or by subschemas.  You will need to add any properties your keyword processes as well as any properties in the contexts you send to subschemas to this list.
- `LocallyEvaluatedPropertyNames` - Similar to `EvaluatedPropertyNames` except that it only tracks those properties which have been evaluated by keywords at the current tier of the schema.
- `LastEvaluatedIndex` - Indicates the last evaluated index in an array.
- `LocalTierLastEvaluatedIndex` - Indicates the last evaluated index in an array by keywords at the current tier of the schema.
- `BaseUri` - This is the current base URI.  It will change throughout the validation process as subschemas declare `$id` or `id` keywords.
- `InstanceLocation` - This is the location of the `Instance` property within the original instance.
- `RelativeLocation` - This is the location of the current subschema with respect to the root schema.  It will contain `$ref` segments for any references that have been processed.
- `BaseRelativeLocation` - This is the location of the current subschema relative to the `BaseUri`.
- `IsMetaSchemaValidation` - Indicates that the current validation pass is a meta-schema validation (validation of a schema by another schema).
- `Misc` - This is just a string-based lookup for any additional data you may need passed between keywords.  The `if`/`then`/`else` keywords do this.

If your keyword contains a nested schema, it's important that you create a new context based on the properties in the context that was given to your keyword.  Changing the existing context will affect the validation of sibling or cousin keywords.  Some of the context properties will have to be modified based on how your keyword behaves.

To create a new context, pass your keyword's name and the context you've been given into the `SchemaValidationContext(string, SchemaValdiationContext)` constructor.  This will create a new context, copying all of the properties while adjusting all of the pointers accordingly.  If there are special cases, the properties are settable so that you can make any changes you need to.

An example of further modification can be found in the `items` keyword, which modifies the instance path to which the subschema applies by adding the appropriate item index.

#### Building a result

The result object is defined by a current proposal for draft-08 which seeks to standardize the output produced by a schema.  Manatee.Json builds the verbose hierarchy format then condenses it according to the `JsonSchemaOptions.OutputFormat` static property.  The location properties are taken care of simply by passing the context into the constructor.  You'll need to set the validation-oriented ones yourself.

- `IsValid` - This property defaults to `true`, so you'll need to set it to false when validation fails.
- `AnnotationValue` - If your keyword generates annotations, set this property when the validation passes.
- `ErrorMessage` - This is the error message generated by the schema.  It is intended to have all values inline rather than being a format string.
- `AdditionalInfo` - This is just a `JsonObject` that you can use to pass any other pertinent information.
- `NestedResults` - If your keyword has one or more subschemas, this property is for the validation results that they produce.

#### Short-circuiting the validation

For schema keywords that contain subschemas, there are two cases when a validation should be short-circuited: if the output format (specified by `JsonSchemaOptions.OutputFormat` is `Flag`) or if the user has configured the keyword or specific location (via the `JsonSchemaOptions.IgnoreErrorsForChildren()` methods).

If the output is configured as `Flag`, then simply check whether the final validation result can be determined (e.g. at the first failing subschema of the `allOf` keyword).  If it _can_ be determined, you can immediately return the correct result.  (Note also that for this case, nested results don't need to be collected.)

Otherwise, check if the keyword should collect errors/annotations from subschemas by calling `JsonSchemaOptions.ShouldReportChildErrors(IJsonSchema, SchemaValidationContext)` and pass your keyword and the context that was passed to it.  If so, then include any nested results.

In implementing the keywords contained in the library, I generally found that manually looping through the subschemas and tracking the valid state and nested results was facilitated by this pattern, though you may come up with your own.

```csharp
var valid = true;
for(var currentSchema in subschemas)
{
    var newContext = ...;  // copy the context then modify it for subschemas
    var localResults = currentSchema.Validate(newContext);
    valid &= localResults.IsValid;  // if valid initializes as true
    //valid |= localResults.IsValid;  // if valid initializes as false

    ... // other stuff you may need to do

    if (JsonSchemaOptions.OutputFormat == SchemaValidationOutputFormat.Flag)
    {
        if (!valid) break;
    }
    else if (reportChildErrors)
        nestedResults.Add(localResults);
}
```

For full examples, you can view the [keyword source files](https://github.com/gregsdennis/Manatee.Json/tree/master/Manatee.Json/Schema/Keywords).

## Add your keyword to the catalog

The `SchemaKeywordCatalog` static class is the curator of all of the keywords.  It also provides the keyword instances during deserialization.

To make your keyword available for use, call the `Add<T>()` method using your keyword type as the type parameter.  The method *does* contain type constraints that required `T` to implement `IJsonSchemaKeyword` and to have a parameterless constructor.

***NOTE** All of the built-in keywords also follow this convention, exposing parameterless constructors.  These constructors have been marked with a `DeserializationUseOnlyAttribute` as a reminder not to use them.*

## Now make it nice to use

To enable the fluent construction interface for your keyword, simply create an extension method on `JsonSchema` that adds the keyword and returns the schema.  For example, adding a `description` keyword is implemented by this method:

```csharp
public static JsonSchema Description(this JsonSchema schema, string description)
{
    schema.Add(new DescriptionKeyword(description));
    return schema;
}
```

