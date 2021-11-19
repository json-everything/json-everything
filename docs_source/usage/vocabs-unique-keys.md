# A Vocabulary for Identifying Uniqueness of Array Items

## 1. Purpose

This document describes a vocabulary defining keywords that can be used to validate that arrary instances contain unique values as determined by one or more given paths within each item.

The intent for these keywords is to cover the use cases discussed in the JSON Schema vocabularies GitHub issue [#22](https://github.com/json-schema-org/json-schema-vocabularies/issues/22).

## 2. Declarations

The ID for this vocabulary is `https://gregsdennis.github.io/json-everything/vocabs-unique-keys` (the URI to this document).

A draft 2020-12 meta-schema which includes this vocabulary has been defined for convenience.  The ID for the meta-schema is `https://gregsdennis.github.io/json-everything/meta/unique-keys`, and it can also be found at this address.

## 3. The `uniqueKeys` Keyword

The value of `uniqueKeys` must be an array of JSON Pointers as defined by [RFC 6901](https://tools.ietf.org/html/rfc6901).  At least one JSON Pointer MUST be specified.

These JSON Pointers, when resolved against each item, define a set of values for that item.  If an item does not contain the location specified by a JSON Pointer, the implementation MUST keep an internal placeholder to indicate a missing value.  (The placeholder cannot be a JSON `null` as there needs to be a distinction between the location existing and holding a `null` value and the location not existing at all.)

Validation for this keyword passes if the value sets for all of the items are distinct, that is, no two sets contain the same ordered collection.

## 4. Examples

### 4.1. Single Key

The following defines a schema to validate an array with items that have unique values at `/foo`.

```json
{
  "$schema": "https://gregsdennis.github.io/json-everything/meta/unique-keys",
  "type": "array",
  "items": {
    "type": "object",
    "properties": {
      "foo": { "type": "integer" }
    }
  },
  "uniqueKeys": [
    "/foo"
  ]
}
```

This value passes validation because all of the values located at `/foo` in each item are unique.

```json
[
  { "foo": 8 },
  { "foo": 12 },
  { "foo": 42 }
]
```

This value fails validation because the value `8` is repeated in two of the items at the `/foo` location.

```json
[
  { "foo": 8 },
  { "foo": 12 },
  { "foo": 8 }
]
```

This value passes validation because `/foo` doesn't return a value for `{"bar": 8}`, so the value sets are `(8)` and `(<missing>)`, which are unique.

```json
[
  { "foo": 8 },
  { "bar": 8 }
]
```

This value fails validation because the value `8` is repeated in two of the items at the `/foo` location.  This example highlights the difference between `uniqueKeys` and `uniqueItems`.  This value would pass `"uniqueItems": true` because `bar` is different between the two `"foo": 8` items.

```json
[
  { "foo": 8, "bar": true },
  { "foo": 12, "bar": true },
  { "foo": 8, "bar": false }
]
```

### 4.2. Multiple Keys

The following defines a schema to validate an array with items that have unique _combinations_ of values at `/foo` and `/bar`.

```json
{
  "$schema": "https://gregsdennis.github.io/json-everything/meta/unique-path",
  "type": "array",
  "items": {
    "type": "object",
    "properties": {
      "foo": { "type": "integer" },
      "bar": { "type": "boolean" }
    }
  },
  "uniqueKeys": [
    "/foo",
    "/bar"
  ]
}
```

This value passes validation because for all combinations of "foo" and "bar", the value sets are unique.

```json
[
  { "foo": 8, "bar": true },
  { "foo": 12, "bar": true },
  { "foo": 8, "bar": false }
]
```

This value fails validation because the first two items have the same combinations of "foo" and "bar", `(8, true)`, even though `"baz"` is different.  This example also would pass `uniqueItems`.

```json
[
  { "foo": 8, "bar": true, "baz": "yes" },
  { "foo": 8, "bar": true, "baz": "no" },
  { "foo": 8, "bar": false }
]
```
