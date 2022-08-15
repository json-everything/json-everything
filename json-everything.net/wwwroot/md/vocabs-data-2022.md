# A Vocabulary for Accessing Data Stored in JSON

## 1. Purpose

This document describes a vocabulary defining keywords that can be used to reference values stored in

- the instance data
- the schema data
- external JSON data

where the dereferenced values serve as input for keywords in a derived subschema.

The intent for this keyword is to cover the use cases discussed across several issues in the JSON Schema specification GitHub repositories.  (A quick search for `"$data"` can readily summon these issues.)

## 2. Declarations

The ID for this vocabulary is `https://json-everything.net/vocabs-data-2022` (the URI to this document).

A draft 2020-12 meta-schema which includes this vocabulary has been defined for convenience.  The ID for the meta-schema is `https://json-everything.net/meta/data-2022`, and it can also be found at this address.

## 3. The `data` Keyword

### 3.1 Syntax and Semantics

The value of `data` must be an object.  The keys of the object are interpreted as JSON Schema keywords, and the values MUST be one of

- JSON Pointers per [RFC 6901](https://www.rfc-editor.org/rfc/rfc6901)
- [Relative JSON Pointers](https://json-schema.org/draft/2019-09/relative-json-pointer.html)
- URI references per [RFC 3986](https://datatracker.ietf.org/doc/html/rfc3986), potentially with JSON Pointer fragments

`data` MUST NOT contain any keys which are defined by the JSON Schema Core Vocabulary.

`data` operates in two phases:

1. All of the values are dereferenced per sections 3.2 and 3.3.
2. The resolved object is then interpreted as a schema (the "formed schema") which is applied to the instance at the current location.

The validation and annotation results of `data` are those of the formed schema.  More detail regarding output can be found in section 3.4.

### 3.2 Contextual Behavior

`data` MUST be processed contextually in the same manner as the schema in which it is used.  Specifically,

- URI resolution MUST be performed in the same manner as `$ref` (per section 3.3).
- Keys that are ignored by the parent schema MUST also be ignored by the formed subschema.

Implementations SHOULD validate the formed schema against the containing schema's meta-schema (as specified by `$schema`) to ensure that it is a syntactically valid schema object.

### 3.3 URI Resolution

The values of `data` are dereferenced in different ways depending on the format of the value.

If the value is a JSON Pointer, it is resolved against the instance root.

If the value is a Relative JSON Pointer, it is resolved against the instance at the location currently being evaluated.

If the value is a URI reference, it must be resolved in accordance with the rules of `$ref` resolution for the relevant JSON Schema specification (e.g. [draft 2020-12, §8.2](https://json-schema.org/draft/2020-12/json-schema-core.html#name-base-uri-anchors-and-derefe)).  However, unlike `$ref` which requires that the indicated data must represent a valid schema object, a `data` reference can identify any value which is valid for the associated keyword.

Implementations SHOULD provide a means to pre-load and cache any external reference but MAY be configured to fetch external documents at evaluation time.  Documents fetched from URIs which contain a JSON Pointer fragment MUST be interpreted using a media type that allows resolution of such fragments.

If any single reference cannot be resolved, validation MUST fail; otherwise the full value at the specified location is returned.

If the resolved value is not valid for the associated keyword, validation MUST fail.

### 3.4 Errors

The `data` keyword can generate two validation failure states as specified by this document:

- a URI reference cannot be resolved, and
- a URI reference can be resolved but the returned value is invalid for the associated keyword.

To make debugging `data` simpler, implementations SHOULD provide an error message indicating what the failure was and for which key it occurred.

If both of these succeed, the evaluation output of the formed schema is reported into the overall schema output incorporating "data" into the evaluation path and following on with additional pointer segments as navigable within the formed schema.

Annotation results of the formed schema are retained so that they can be processed by other keywords such as `unevaluatedItems` and `unevaluatedProperties`.

## 4. A Short Example

The following defines a schema to validate an object instance with a `foo` property that must contain an integer value less than or equal to the value in the instance's `minValue` property.

```json
{
  "$schema": "https://json-everything.net/meta/data-2022",
  "type": "object",
  "properties": {
    "foo": {
      "type": "integer",
      "data": {
        "minimum": "/minValue"
      }
    },
    "minValue": {
      "type": "integer"
    }
  },
  "dependentRequired": {
    "foo": [ "minValue" ]
  }
}
```

The `data` property declares that its parent subschema should validate a `minimum` keyword whose value is the value in the `minValue` property of the instance.

Note also the use of `dependentRequired` to ensure that when `foo` is present in the instance, `minValue` is also present.  While this not explicitly required of schema authors, this type of insurance mechanism is recommended.

The following shows how a change in the `minValue` property can affect the validation result of the `foo` property and thus the entire instance.

```json
// passing
{
  "minValue": 5,
  "foo": 10
}

// failing
{
  "minValue": 15,
  "foo": 10
}
```
