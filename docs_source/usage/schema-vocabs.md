# Extending schemas

## Vocabularies

JSON Schema draft 2019-09 introduced the idea of vocabularies to enable some spec support for custom keywords.

A vocabulary is just a collection of keywords.  It will be identified by a URI and should have an associated specification that describes the function of each of the keywords.  There *may* also be an associated meta-schema.

Creating a vocabulary in JsonSchema<nsp>.Net isn't strictly required in order to add custom keywords, but if you're using it to create a meta-schema that will consume and validate other draft 2019-09 or later schemas, it is strongly suggested.

### How vocabularies work

This is best explained with an example.  Suppose we have a meta-schema **M**, a schema **S** that uses **M** as its `$schema`, and a couple instances **I1** and **I2** to be validated by **S**.

```json
// meta-schema M
{
  "$schema": "https://myserver.net/meta-schema",                           // 1
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

1. We declare a meta-schema.  The meta-schema should validate itself, so we declare `$schema` to be the same as `$id`.
2. We list the vocabularies that the JsonSchema<nsp>.Net should know about in order to process schemas that declare this meta-schema as their `$schema` (see #5).  This includes all of the vocabularies from 2020-12 (because we want all of the 2020-12 capabilities) as well as the vocab for this meta-schema.  We'll explain a bit more about this later.
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

Now, if you look at the `$vocabulary` entry for `https://myserver.net/my-vocab`, the vocabulary has its ID as the key with a boolean value.  In this case, that value is `true`.  That means that if JsonSchema<nsp>.Net *doesn't* know about the vocabulary, it **must** refuse to process any schema that declares **M** as its `$schema` (as **S** does).  If this value were `false`, then JsonSchema<nsp>.Net would be allowed to continue, which means that only syntactic analysis (i.e. "Is `minDate` a date-formatted string?") would be performed.

So, back to the example, because we declare the vocabulary to be required (by giving it a value of `true`) *and* because JsonSchema<nsp>.Net knows about it, **I1** is reported as valid and **I2** is not.  If the vocabulary had not been required _and_ JsonSchema<nsp>.Net didn't know about the vocabulary, both **I1** and **I2** would be reported as valid because the `minDate` keyword would not have been enforced.

### Registering a vocabulary

To tell JsonSchema<nsp>.Net about a vocabulary, you need to create a `Vocabulary` instance and register it using `VocabularyRegistry.Add<T>()`.

The `Vocabulary` class is quite simple.  It defines the vocabulary's ID and lists the keywords which it supports.

The keywords must be [registered separately](schema-keywords.md).

It's not always necessary to have a meta-schema for your vocabulary.  However, if you want to enable `ValidationOptions.ValidateMetaschema`, you will need to register it.