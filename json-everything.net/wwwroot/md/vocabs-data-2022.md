# A Vocabulary for Accessing Data Stored in JSON

## 1. Purpose

This document describes a vocabulary defining keywords that can be used to reference values stored in

- the instance data
- external JSON data
- the schema data

and use the dereferenced values as input for other keywords.

The intent for this keyword is to cover the use cases discussed across several issues in the JSON Schema specification GitHub repositories.  (A quick search for `"$data"` can readily summon these issues.)

## 2. Declarations

The ID for this vocabulary is `https://json-everything.net/vocabs-data-2022` (the URI to this document).

A draft 2020-12 meta-schema which includes this vocabulary has been defined for convenience.  The ID for the meta-schema is `https://json-everything.net/meta/data-2022`, and it can also be found at this address.

## 3. The `data` Keyword

### 3.1 Syntax and Semantics

The value of `data` must be an object.  The keys of the object correspond to valid JSON Schema keywords, and the values MUST be one of

- JSON Pointers per [RFC 6901](https://www.rfc-editor.org/rfc/rfc6901)
- [Relative JSON Pointers](https://json-schema.org/draft/2019-09/relative-json-pointer.html)
- URI references per [RFC 3986](https://datatracker.ietf.org/doc/html/rfc3986), potentially with JSON Pointer fragments

The set of keywords in the schema or subschema containing `data` SHOULD be distinct from the set of keys defined within `data`.  Behavior in the event of an overlap is not defined.

`data` operates in two phases:

1. All of the values are dereferenced per sections 3.2 and 3.3.
2. The keywords and values then form a schema which is applied to the instance at the current location.

The validation and annotation results of `data` are those of the formed schema.  More detail regarding output can be found in section 3.4.

### 3.2 Contextual Behavior

`data` MUST be processed contextually in accordance with the draft of the schema in which it is used.  For example, if `data` is used in a schema that declares draft 2019-09, then

- the keys SHOULD be recognized by draft 2019-09 or defined in other referenced vocabularies, and
- the URI references MUST be resolved following the same rules as `$ref` as defined by draft 2019-09.

If a key is not a recognized keyword in the schema's draft (e.g. `$dynamicAnchor` in a draft 2019-09 schema), then this key is ignored just as it would be ignored if it were in the schema or subschema containing `data`.

### 3.3 URI Resolution

The values of `data` are dereferenced in different ways depending on the format of the value.

If the value is a JSON Pointer, it is resolved against the instance root.

If the value is a Relative JSON Pointer, it is resolved against the instance at the location currently being evaluated.

If the value is a URI reference, it must be resolved in accordance with the rules of `$ref` resolution for the containing spec, as per section 3.2.  The primary different between resolving URIs for `data` and resolving URIs for `$ref` is that `$ref` requires that the indicated data must represent a valid schema object whereas `data` can reference any valid JSON value.

External document data can be referenced using an absolute URI base identifier along with a JSON Pointer fragment.

Implementations SHOULD be able to download external JSON documents, but MAY provide means to pre-load and cache such documents against the identifiers by which they will be referenced.  Documents downloaded from URIs which contain a JSON Pointer fragment MUST be interpreted using a media type that allows resolution of such fragments.

If any single reference cannot be resolved, validation MUST fail; otherwise the full value at the specified location is returned.

If the resolved value is not valid for the associated keyword, validation MUST fail.

### 3.4 Errors

The output formatting specified by the JSON Schema Core specification can only indicate that something failed at the `data` node, but there is no provision for providing further detail.

In summary, the `data` keyword can generate two validation failure states as specified by this document:

- a URI reference cannot be resolved, and
- a URI reference can be resolved but the returned value is invalid for the associated keyword.

To make debugging `data` simpler, implementations SHOULD provide an error message indicating what the failure was and for which key it occurred.

If both of these succeed, the validation output of the resulting subschema is reported into the overall schema output as if the subschema were actually a child of the `data` keyword.

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
