# JsonSchema<nsp>.Net Overview

The occasion may arise when you wish to validate that a JSON object is in the correct form (has the appropriate keys and the right types of values).  Enter JSON Schema.  Much like XML Schema with XML, JSON Schema defines a pattern for JSON data.  A JSON Schema validator can verify that a given JSON object meets the requirements as defined by the JSON Schema.  This validation can come in handy as a precursor step before deserializing.

More information about JSON Schema can be found at [json-schema.org](http://json-schema.org).

To support JSON Schema, JsonSchema<nsp>.Net exposes the `JsonSchema` type.  This type is implemented as a list of keywords, each of which correspond to one of the keywords defined in the JSON Schema specifications.

<p style="text-align: center;">
<a href="https://json-everything.net/json-schema" target="_block" style="color: rgb(255, 255, 255); background-color: rgb(13, 71, 161); display: inline-block; font-weight: 500; font-size: 2rem; text-align: center; vertical-align: middle; padding: 0.6rem 0.9rem; border-radius: 0.35rem; cursor: pointer; user-select: none; text-decoration: none; --darkreader-inline-color:#ffffff; --darkreader-inline-bgcolor:#06419c; margin-top: 1.5rem !important;">Try it online!</a>
</p>

## Drafts

There are currently six active drafts of the JSON Schema specification:

- Draft 3
- Draft 4
- Draft 6
- Draft 7
- Draft 2019-09
- Draft 2020-12

JsonSchema<nsp>.Net supports draft 6 and later.

### Meta-schemas

Each draft defines a meta-schema.  This is a special JSON Schema that describes all of the keywords available for that draft.  They are intended to be used to validate other schemas.  Usually, a schema will declare the draft it should adhere to using the `$schema` keyword.

JsonSchema<nsp>.Net declares the meta-schemas for the supported drafts as members of the `MetaSchemas` static class.

Draft 2019-09 introduced vocabularies.  As part of this new feature, the meta-schemas for this draft and those which follow it have been split into vocabulary-specific meta-schemas.  Additionally, the specification recognizes that the meta-schemas aren't perfect and may need to be updated occasionally.  To this end, the meta-schemas are versioned with the year and month they are published.  The schemas within this library are named accordingly.

## Keywords

JSON Schema is expressed as a collection of keywords, each of which provides a specific constraint on a JSON instance.  For example, the `type` keyword specifies what type of data an instance may be, whereas the `minimum` keyword specifies a minimum numeric value *for numeric data*.  These keywords can be combined to express the expected shape of any JSON instance.  Once defined, the schema validates the instance, providing feedback on errors that occurred, including what and where the error occurred.
