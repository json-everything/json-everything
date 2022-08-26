# A Vocabulary for Accessing Data Stored in JSON

## 1. Purpose

This document describes a vocabulary defining keywords that can be used to reference values stored in

- the instance data
- the schema data
- external JSON data

where the dereferenced values serve as input for keywords in a derived subschema.

The intent for this keyword is to cover the use cases discussed across several issues in the JSON Schema specification GitHub repositories.  (A quick search for `"$data"` can readily summon these issues.)

## 2. Declarations

The ID for this vocabulary is `https://json-everything.net/vocabs-data-2022`.

A draft 2020-12 meta-schema which includes this vocabulary has been defined for convenience.  The ID (`$id`) for the meta-schema is `https://json-everything.net/meta/data-2022`, and it can also be found at this address.

## 3. Definitions

### 3.1 Formed Schema

The schema object created as a result of dereferencing all of the values in the `data` keyword as described in section 4.1.

### 3.2 Host Schema

The schema object which contains the `data` keyword.  The processing rules that govern this schema also govern the formed schema, as specified by section 4.2.

## 4. The `data` Keyword

### 4.1 Syntax and Semantics

The value of `data` must be an object.  The keys of the object are interpreted as JSON Schema keywords, and the values MUST be one of

- JSON Pointers per [RFC 6901](https://www.rfc-editor.org/rfc/rfc6901) 
- [Relative JSON Pointers](https://json-schema.org/draft/2019-09/relative-json-pointer.html)
- fragment-only IRI using IRI-encoded JSON Pointer identifier per [RFC 6901, §6](https://www.rfc-editor.org/rfc/rfc6901#section-6)
- Absolute IRIs per [RFC 3987](https://datatracker.ietf.org/doc/html/rfc3987), optionally including an additional IRI-encoded JSON Pointer fragment identifier

`data` MUST NOT contain any keys which are defined by the JSON Schema Core Vocabulary.

`data` operates in two phases:

1. All of the values are dereferenced per sections 4.2 and 4.3.
2. The resolved object is then interpreted as a schema which is applied to the instance at the current location.

The validation and annotation results of `data` are those of the formed schema.  More detail regarding output can be found in section 3.4.

### 4.2 Contextual Behavior

`data` MUST be processed contextually in the same manner as the host schema.  Specifically,

- IRI resolution MUST be performed in the same manner as `$ref` (per section 3.3).
- Keys that are ignored by the host schema MUST also be ignored by the formed schema.

Implementations SHOULD validate that the resolved data forms a valid schema against the host schema's meta-schema (as specified by `$schema`).

***NOTE** It is not necessary for an implementation to validate using the meta-schema.  Other mechanisms internal to the implementation (such as deserialization) may suffice to perform this task.*

### 4.3 IRI Resolution

The values of `data` are dereferenced in different ways depending on the format of the value.

If the value is a JSON Pointer, it is resolved against the instance root.

If the value is a Relative JSON Pointer, it is resolved against the instance at the location currently being evaluated.

Otherwise, it must be resolved in accordance with the rules of `$ref` resolution for the relevant JSON Schema specification (e.g. [draft 2020-12, §8.2](https://json-schema.org/draft/2020-12/json-schema-core.html#name-base-uri-anchors-and-derefe)).  However, unlike `$ref` which requires that the indicated data must represent a valid schema object, a `data` reference can identify any value which is valid for the associated keyword.

Because JSON Pointers and Relative JSON Pointers are syntactically valid IRIs, the value MUST be checked in the sequence indicated above in order to properly identify the pointer types from other IRIs.

For each successfully resolved reference, the full value at the specified location MUST be returned.

If a reference cannot be resolved, or if a resolved value is not valid for the associated keyword, evaluation MUST halt.  Implementations SHOULD use native features of their language to report the failure as appropriate.  Implementations MAY continue to attempt to resolve other references so that multiple resolution failures can be reported together, however further schema evaluation MUST NOT continue.

#### 4.3.1 External Data Access

Implementations SHOULD provide a means to pre-load and cache any external references prior to evaluation but MAY be configured to fetch external documents at evaluation time.  Documents fetched from IRIs which contain a JSON Pointer fragment MUST be interpreted using a media type, such as `application/schema-instance+json`, that allows resolution of such fragments.

Users should be aware that fetching data from external locations may carry certain security risks not covered by this document.

### 4.4 Output

The evaluation output of the formed schema is reported into the overall schema output incorporating "data" into the evaluation path and following on with additional pointer segments as navigable within the formed schema.

Annotation results of the formed schema are retained as per the host schema so that they can be processed by other keywords such as `unevaluatedItems` and `unevaluatedProperties`.

## 5. A Short Example

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
